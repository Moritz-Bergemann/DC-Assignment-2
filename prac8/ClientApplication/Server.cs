using APIClasses;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.ServiceModel;

namespace ClientApplication
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    class Server : IServer
    {
        public static Server Instance
        {
            get;
        } = new Server();

        private ServiceHost _host;
        private RestClient _registryServer;
        private ClientData _myClientData; //Stores client data for this client

        private List<JobData> _jobs;
        private List<JobData> _doneJobs;
        private uint _jobIdCounter;

        private string _statusString;


        private Server()
        {
            _host = null;
            _jobs = new List<JobData>();
            _doneJobs = new List<JobData>();
            _jobIdCounter = 0;
            
            _statusString = "Not Started";

            _myClientData = null;
            _registryServer = new RestClient("https://localhost:44392/");
        }

        public int NumJobs
        {
            get => _jobs.Count;
        }

        public ReadOnlyCollection<JobData> CompletedJobs
        {
            get => _doneJobs.AsReadOnly();
        }

        public string Status
        {
            get => _statusString;
        }

        public ClientData EndpointData
        {
            get => new ClientData(_myClientData.Address, _myClientData.Port);
        }

        public void Open(string address, uint port)
        {
            _statusString = "Starting";

            if (_host != null)
            {
                throw new ArgumentException("Server already running");
            }

            string url = $"net.tcp://{address}:{port}/PeerServer";

            //Create host service
            NetTcpBinding tcp = new NetTcpBinding();

            //Bind service to this singleton & add endpoint
            _host = new ServiceHost(this);

            try
            {
                _host.AddServiceEndpoint(typeof(IServer), tcp, url);
            }
            catch (UriFormatException)
            {
                _statusString = "Error";
                _host = null;
                throw new ArgumentException($"Invalid server URL '{url}'");
            }

            //Post self to registry server
            RestRequest request = new RestRequest("api/register");
            request.AddJsonBody(new ClientData(address, port));
            IRestResponse response = _registryServer.Post(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                _statusString = "Error";
                throw new ArgumentException($"Server failed to open - '{response.Content}'");
            }

            //Initialise the blockchain
            InitialiseBlockchain();

            //Open to receive communications
            _host.Open();

            _myClientData = new ClientData(address, port);

            //Set status to open
            _statusString = "Open";
        }

        public void CloseServer()
        {
            if (_host == null)
            {
                throw new ArgumentException("Server is not running");
            }

            //Close host and reset to null
            _host.Close();
            _host = null;

            _myClientData = null;

            _statusString = "Closed";
        }

        /// <summary>
        /// Add a new job to the list of pending jobs
        /// </summary>
        /// <param name="python">Python code for the job</param>
        public void AddNewJob(string python)
        {
            JobData job = new JobData(_jobIdCounter, python);
            
            //Increase job counter for next job
            _jobIdCounter++;

            _jobs.Add(job);
        }

        /// <summary>
        /// Initialises blockchain for this server, either by retrieving it from another client or by creating the first (dummy) block if no others are available
        /// </summary>
        private void InitialiseBlockchain()
        {
            //See if there are other clients in the registry server
            RestRequest request = new RestRequest("api/get-registered");
            IRestResponse response = _registryServer.Get(request);

            List<ClientData> registered = JsonConvert.DeserializeObject<List<ClientData>>(response.Content);

            if (registered.Count > 0)
            {
                //Initialise the blockchain to be the most common blockchain
                Blockchain.Instance.Chain = Miner.Instance.GetMostCommonBlockchain();
            }
            else
            {
                //We are the first blockchain client
                //Construct genesis block
                Block genesis = new Block()
                {
                    Id = 0,
                    Amount = 0,
                    BlockOffset = 0,
                    WalletFrom = 0,
                    WalletTo = 0,
                    PrevHash = null
                };
                genesis.Hash = genesis.FindHash();

                List<Block> blockchain = new List<Block>();
                blockchain.Add(genesis);

                Blockchain.Instance.Chain = blockchain;
            }
        }

        //SERVER FUNCTIONALITY
        //TODO
        public Block GetLastBlock()
        {
            return Blockchain.Instance.LastBlock;
        }

        public List<Block> GetBlockchain()
        {
            return Blockchain.Instance.Chain;
        }

        public void PutTransaction(Transaction transaction)
        {
            Miner.Instance.AddTransaction(transaction);
        }
    }
}
