using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using APIClasses;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
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
        /// Continually run job performing operations in distributed application.
        /// </summary>
        public async void Run()
        {
            while (true)
            {
                await Iteration();
            }
        }

        /// <summary>
        /// Run one iteration of the network thread (look for existing _clients and make requests to them)
        /// </summary>
        private async Task Iteration()
        {
            //Refresh data on existing clients
            await UpdateClients();

            //Provide service (look for jobs and complete them) on each client
            await ProvideService();
        }

        /// <summary>
        /// Get list of available clients from web server and update the local list
        /// </summary>
        private async Task UpdateClients()
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

        private async Task ProvideService()
        {
            //Find the a random job that needs completing (iterate randomly through each client until a valid job is found)
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
                    job = await GetFirstJob(clientServer);
                }
                catch (ArgumentException)
                {
                    //If you couldn't get a job, try the next client
                    continue;
                }

                //Do the job, then exit
                await DoJob(clientServer, job);

                break;
            }
        }

        private static async Task<JobData> GetFirstJob(IServer clientServer)
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

        /// <summary>
        /// Performs a job for a given client, posting the result back to the client.
        /// </summary>
        /// <param name="clientServer">Server that had job listed</param>
        /// <param name="job">Job to perform</param>
        private async Task DoJob(IServer clientServer, JobData job)
        {
            //Do job via Iron Python //TODO make this async?
            ScriptEngine engine = Python.CreateEngine();


            dynamic result = await Task.Run(() => engine.Execute(job.Python, engine.CreateScope()));

            //Change result to string in case it isn't
            if (result != null)
            {
                result = result.ToString();
            }

            //Post result back to client
            clientServer.PostCompletedJob(job.Id, result);
        }
    }
}
