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

        private Server()
        {
            _host = null;
            
            Status = "Not Started";

            _myClientData = null;
            _registryServer = new RestClient("https://localhost:44392/");
        }

        public string Status
        {
            get;
            private set;
        }

        public ClientData EndpointData
        {
            get => new ClientData(_myClientData.Address, _myClientData.Port);
        }

        public void Open(string address, uint port)
        {
            Status = "Starting";

            if (_host != null)
            {
                throw new ArgumentException("Server already running");
            }

            string url = $"net.tcp://{address}:{port}/BlockchainServer";

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
                Status = "Error";
                _host = null;
                throw new ArgumentException($"Invalid server URL '{url}'");
            }

            //Initialise the blockchain
            InitialiseBlockchain();

            //Open to receive communications
            _host.Open();

            //Store your own data
            _myClientData = new ClientData(address, port);

            //Post self to registry server
            RestRequest request = new RestRequest("api/register");
            request.AddJsonBody(new ClientData(address, port));
            IRestResponse response = _registryServer.Post(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Status = "Error";
                throw new ArgumentException($"Failed to register server - '{response.Content}'");
            }

            //Set status to open
            Status = "Open";
        }

        public void Close()
        {
            if (_host == null)
            {
                throw new ArgumentException("Server is not running");
            }

            //Close host and reset to null
            _host.Close();
            _host = null;

            _myClientData = null;

            Status = "Closed";
        }

        /// <summary>
        /// Initializes blockchain for this server, either by retrieving it from another client or by creating the first (dummy) block if no others are available
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
                Blockchain.Instance.SetBlockchain(Miner.Instance.GetMostCommonBlockchain());
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

                Blockchain.Instance.SetBlockchain(blockchain);
            }
        }

        //SERVER FUNCTIONALITY
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
