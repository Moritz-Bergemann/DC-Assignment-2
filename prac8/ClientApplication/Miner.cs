using APIClasses;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Scripting.Debugging;

namespace ClientApplication
{
    class Miner
    {
        private Queue<Transaction> _transactions;
        //private bool _mining;
        private readonly object _miningLock = new object();

        private Timer _blockchainChecker;
        private bool _pendingCheck = false;

        private RestClient _registryServer;

        public static Miner Instance
        {
            get;
        } = new Miner();

        private Miner()
        {
            Status = "Starting";

            //Instantiate REST client
            _registryServer = new RestClient("https://localhost:44392/");

            _transactions = new Queue<Transaction>();

            //Start task of validating blockchain during downtime
            _blockchainChecker = new Timer(VerifyBlockchainInDowntime, null, 1000, 1000);

            Status = "Waiting for transactions";
        }

        public string Status { get; private set; }

        public int MinedBlocks { get; private set; } = 0;

        private void VerifyBlockchainInDowntime(Object o)
        {

            //If the local blockchain has been initialized, we aren't mining and aren't already doing a check verify the chain
            if (!_pendingCheck && Blockchain.Instance.Chain != null)
            {
                _pendingCheck = true;

                lock (_miningLock)
                {
                    VerifyPopularBlockchain();
                }

                _pendingCheck = false;
            }
        }

        /// <summary>
        ///Shut down the miner thread.
        /// </summary>
        public void Close()
        {
            //Close checking timer
            _blockchainChecker.Dispose();
        }


        /// <summary>
        /// Verifies this client has the most popular blockchain. Downloads most popular blockchain if it does not.
        /// </summary>
        public void VerifyPopularBlockchain()
        {
            //Get the most common blockchain
            List<Block> commonBlockchain = GetMostCommonBlockchain();

            //Check most common blockchain is the same as this client's one, override it if not
            if (!Blockchain.Instance.LastBlock.Hash.SequenceEqual(commonBlockchain.Last().Hash))
            {
                Logger.Instance.Log($"Found did not have most common blockchain (our hash is '{Convert.ToBase64String(Blockchain.Instance.LastBlock.Hash)}', most common is '{Convert.ToBase64String(commonBlockchain.Last().Hash)}'), downloading");
                Blockchain.Instance.SetBlockchain(commonBlockchain);
            }
        }

        /// <summary>
        /// Retrieves the most common blockchain from all clients in the registry.
        /// </summary>
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
                IServer clientServer = CreateBlockchainServer(client);

                Block lastBlock;
                try
                {
                    lastBlock = clientServer.GetLastBlock();
                }
                catch (Exception e)
                {
                    if (e is EndpointNotFoundException || e is CommunicationException)
                    {
                        Logger.Instance.Log($"Getting most common blockchain access to client '{client} failed, reporting as downed to registry'");

                        ReportClientDowned(client);
                        continue;
                    }
                    else
                    {
                        throw;
                    }
                }

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
            IServer mostCommonClientServer = CreateBlockchainServer(mostCommonClient);

            return mostCommonClientServer.GetBlockchain();
        }

        private IServer CreateBlockchainServer(ClientData client)
        {
            string url = $"net.tcp://{client.Address}:{client.Port}/BlockchainServer";
            NetTcpBinding tcp = new NetTcpBinding();
            ChannelFactory<IServer> serverChannelFactory = new ChannelFactory<IServer>(tcp, url);
            return serverChannelFactory.CreateChannel();
        }

        private void ReportClientDowned(ClientData downedClient)
        {
            RestRequest request = new RestRequest("api/report-downed");
            request.AddJsonBody(downedClient);
            _registryServer.Post(request);
        }

        /// <summary>
        /// Adds a transaction to the queue of transactions. It will either be mined immediately, or mined as soon as other pending transactions have been mined
        /// </summary>
        /// <param name="transaction"></param>
        public void AddTransaction(Transaction transaction)
        {
            //Add transaction to mining queue
            _transactions.Enqueue(transaction);

            //Try mining (will mine unless already mining)
            Task.Run(Mine);
        }

        private void Mine()
        {
            //Set mining lock
            lock (_miningLock)
            {
                try
                {
                    while (_transactions.Count > 0)
                    {
                        Status = "Mining";

                        //Get first element
                        Transaction transaction = _transactions.Peek();

                        //Get latest block from blockchain
                        Block lastBlock = Blockchain.Instance.LastBlock;

                        //Create block for the transaction
                        Block block = new Block()
                        {
                            Id = lastBlock.Id + 1,
                            Amount = transaction.Amount,
                            BlockOffset = 0,
                            WalletFrom = transaction.WalletFrom,
                            WalletTo = transaction.WalletTo,
                            PrevHash = lastBlock.Hash
                        };

                        //Calculate hash for block (this will take a long time)
                        block.Hash = block.FindHash();

                        //Try adding block to local blockchain
                        try
                        {
                            Blockchain.Instance.AddBlock(block);
                        }
                        catch (BlockchainException b)
                        {
                            Logger.Instance.Log($"Tried to add block '{block}' to blockchain but got error - {b.Message}, discarding");

                                _transactions.Dequeue();
                            continue;
                        }

                        MinedBlocks++;

                        Logger.Instance.Log($"Added block '{block}' to local blockchain");

                        Status = "Validating most popular blockchain";

                        //Check to see if we still have the most common blockchain
                        VerifyPopularBlockchain();

                        //If transaction was submitted to blockchain successfully, remove it from the queue so we can work on the next
                        _transactions.Dequeue();
                    }
                }
                finally
                {
                    Status = "Waiting for transactions";
                }
            }
        }
    }
}
