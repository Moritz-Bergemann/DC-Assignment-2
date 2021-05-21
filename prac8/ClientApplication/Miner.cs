using APIClasses;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading.Tasks;

namespace ClientApplication
{
    class Miner
    {
        private List<ClientData> _clients;
        private string _statusString;

        private RestClient _registryServer;
        private int _numJobsDone;

        public static Miner Instance
        {
            get;
        } = new Miner();

        private Miner()
        {
            _clients = null;

            _statusString = "Starting";
            _numJobsDone = 0;

            //Instantiate REST client
            _registryServer = new RestClient("https://localhost:44392/");
        }

        public string Status
        {
            get => _statusString;
        }

        public int NumJobsDone
        {
            get => _numJobsDone;
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
            _statusString = "Looking for Jobs to Do";

            //Refresh data on existing clients
            await UpdateClients();

            //Provide service (look for jobs and complete them) on each client
            await ProvideService();
        }

        /// <summary>
        /// Get list of available clients from web server and update the local list
        /// </summary>
        private async Task UpdateClients() //TODO implement async
        {
            RestRequest request = new RestRequest("api/get-registered");

            IRestResponse response = await Task.Run(() => _registryServer.Get(request));

            if (response.StatusCode != HttpStatusCode.OK)
            {
                //Make clients empty list to maintain validity
                _clients = new List<ClientData>();

                throw new ArgumentException(response.Content);
            }

            List<ClientData> clientData = JsonConvert.DeserializeObject<List<ClientData>>(response.Content);

            _clients = clientData;
        }

        private async Task ProvideService()
        {
            //Find the a random job that needs completing (iterate through each client until a valid job is found)
            foreach (ClientData client in _clients)
            {
                //Connect to client's server
                string url = $"net.tcp://{client.Address}:{client.Port}/PeerServer";
                
                NetTcpBinding tcp = new NetTcpBinding();
                ChannelFactory<IServer> serverChannelFactory = new ChannelFactory<IServer>(tcp, url);
                IServer clientServer = serverChannelFactory.CreateChannel();

                JobData job = null;

                try
                {
                    job = await GetFirstJob(clientServer);
                }
                catch (CommunicationException) //In case connection with the client failed
                {
                    ReportDowned(client);
                }

                if (job == null)
                {
                    //If you couldn't get a job, try the next client
                    continue;
                }

                //Do the job, then exit
                try
                {
                    await DoJob(clientServer, job);
                }
                catch (CommunicationException) //In case connection with the client failed
                {
                    ReportDowned(client);
                }

                break;
            }
        }

        /// <summary>
        /// Gets the first available job on the given server, returns null if none available.
        /// </summary>
        /// <param name="clientServer">The first available job, or null if there are none</param>
        /// <returns></returns>
        private static async Task<JobData> GetFirstJob(IServer clientServer)
        {
            //Get the first available job

            TransmitJobData transmitJob = null;
            bool valid = false;

            while (!valid)
            {
                transmitJob = await Task.Run(clientServer.DownloadFirstJob);

                if (transmitJob == null)
                {
                    return null;
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
            _statusString = "Doing Job";

            //Do job via Iron Python
            ScriptEngine engine = Python.CreateEngine();

            dynamic result;

            try
            {
                result = await Task.Run(() => engine.Execute(job.Python, engine.CreateScope()));
            }
            catch (Exception e) //Must catch base Exception as IronPython may throw any exception
            {
                result = $"EXCEPTION THROWN - {e.Message}";
            }

            //Change result to string in case it isn't
            if (result != null)
            {
                result = result.ToString();
            }

            //Post result back to client
            bool accepted = clientServer.PostCompletedJob(job.Id, result, Server.Instance.EndpointData);

            if (accepted) //If server accepted job as completed
            {
                //Update the "finished jobs" count
                _numJobsDone++;
            }
        }

        /// <summary>
        /// Report the given client to the central registry as downed.
        /// </summary>
        /// <param name="client">Client to report as downed</param>
        private bool ReportDowned(ClientData client)
        {
            RestRequest request = new RestRequest("api/report-downed");
            request.AddJsonBody(client);

            IRestResponse response = _registryServer.Post(request);

            return response.Content.Equals("OK");
        }

        public List<Block> GetMostCommonBlockchain()
        {
            //Get list of all miners from the registry
            RestRequest pollRequest = new RestRequest("api/get-registered");
            IRestResponse pollResponse = _registryServer.Get(pollRequest);

            List<ClientData> clients = JsonConvert.DeserializeObject<List<ClientData>>(pollResponse.Content);

            if (clients.Count == 0)
            {
                throw new ArgumentException("No available clients in registry");
            }

            List<byte[]> blockHashes = new List<byte[]>(); //Last block hash
            List<ClientData> blockClients = new List<ClientData>(); //A client endpoint with this hash
            List<int> blockCounts = new List<int>(); //Number of clients with this hash

            foreach (ClientData client in clients)
            {
                //Get the latest block hash for the client
                //Connect to client's server
                IServer clientServer = CreateClientServer(client);

                Block lastBlock = clientServer.GetLastBlock();
                byte[] lastBlockHash = lastBlock.Hash;

                //If this last hash is new
                if (!blockHashes.Any(h => h.Equals(lastBlockHash)))
                {
                    //Update all data for this new hash
                    blockHashes.Add(lastBlockHash);
                    blockClients.Add(client);
                    blockCounts.Add(1);
                }
                else
                {
                    //Find index where the matching hash is stored
                    int index = blockHashes.FindLastIndex(h => h.Equals(lastBlockHash));

                    //Add 1 to blockchain common-ness count for this hash
                    blockCounts[index]++;
                }
            }

            //Get index of the most common last block hash
            int largestIndex = blockCounts.IndexOf(blockCounts.Max());

            //Get a client that has the most common blockchain
            ClientData mostCommonClient = blockClients[largestIndex];

            //Get that client's version of the blockchain
            IServer mostCommonClientServer = CreateClientServer(mostCommonClient);

            return mostCommonClientServer.GetBlockchain();
        }

        private IServer CreateClientServer(ClientData client)
        {
            string url = $"net.tcp://{client.Address}:{client.Port}/PeerServer";
            NetTcpBinding tcp = new NetTcpBinding();
            ChannelFactory<IServer> serverChannelFactory = new ChannelFactory<IServer>(tcp, url);
            return serverChannelFactory.CreateChannel();
        }
    }
}
