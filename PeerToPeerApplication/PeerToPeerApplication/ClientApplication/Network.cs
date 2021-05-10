using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using APIClasses;
using Newtonsoft.Json;
using RestSharp;

namespace ClientApplication
{
    class Network
    {
        //TODO better commenting
        private List<RegistryData> _clients;

        private RestClient _webServerClient;

        public static Network Instance
        {
            get;
        } = new Network();

        private Network()
        {
            _clients = null;
            //TODO instantiate REST client
        }
        
        /// <summary>
        /// Run one iteration of the network thread (look for existing _clients and make requests to them)
        /// </summary>
        public async void Iteration()
        {
            //Refresh data on existing clients
            UpdateClients();

            //Provide service (look for jobs and complete them) on each client
            ProvideService();
        }

        /// <summary>
        /// Get list of available clients from web server and update the local list
        /// </summary>
        private void UpdateClients()
        {
            RestRequest request = new RestRequest("api/get-registered");

            IRestResponse response = _webServerClient.Get(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                //Make clients empty list to maintain validity //TODO is this the right approach
                _clients = new List<RegistryData>();

                throw new Exception(response.Content); //TODO proper error handling
            }

            List<RegistryData> clientData = JsonConvert.DeserializeObject<List<RegistryData>>(response.Content);

            if (clientData == null)
            {
                throw new Exception("Why null???"); //TODO fix this
            }

            _clients = clientData;
        }

        private void ProvideService() //TODO this is fucking awful, restructure it
        {
            //Find the first job that needs completing
            foreach (RegistryData client in _clients.OrderBy(a => Guid.NewGuid()).ToList())
            {
                //Connect to client's server
                string url = $"{client.Address}:{client.Port}";
                
                NetTcpBinding tcp = new NetTcpBinding();
                ChannelFactory<IServer> serverChannelFactory = new ChannelFactory<IServer>(tcp, url);
                IServer clientServer = serverChannelFactory.CreateChannel();


                JobData job = null;
                try
                {
                    job = GetFirstJob(clientServer);
                }
                catch (ArgumentException)
                {
                    continue;
                }

                //Do the job
                //TODO
            }
        }

        private JobData GetFirstJob(IServer clientServer)
        {
            //Get the first available job

            TransmitJobData transmitJob = null;
            bool valid = false;

            while (!valid)
            {
                transmitJob = clientServer.DownloadFirstJob();

                if (transmitJob == null)
                {
                    throw new ArgumentException("No jobs available");
                }

                //Check the hash
                valid = transmitJob.CheckHash();
            }

            //Construct new job
            return new JobData(transmitJob.Id, transmitJob.GetDecodedPython());
        }
    }
}
