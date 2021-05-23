﻿using APIClasses;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ClientApplication
{
    class Miner
    {
        private Queue<Transaction> _transactions;
        //private bool _mining;
        private object _miningLock = new object();

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

            ////Start task of validating blockchain during downtime
            //DispatcherTimer dTimer = new DispatcherTimer();
            //dTimer.Tick += new EventHandler(VerifyBlockchainInDowntime);
            //dTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            //dTimer.Start();

            Status = "Waiting for transactions";
        }

        public string Status { get; private set; }

        public int MinedBlocks { get; private set; } = 0;

        //private void VerifyBlockchainInDowntime(Object o, EventArgs args)
        //{
        //    //If the local blockchain has been initialized and we aren't mining, verify it the chain
        //    if (Blockchain.Instance.Chain != null && !_mining)
        //    {
        //        lock (_miningLock)
        //        {

        //        }
        //    }
        //}

        public void VerifyPopularBlockchain()
        {
            //Get the most common blockchain
            List<Block> commonBlockchain = GetMostCommonBlockchain();

            //Check most common blockchain is the same as this client's one, override it if not
            if (!Blockchain.Instance.LastBlock.Hash.SequenceEqual(commonBlockchain.Last().Hash))
            {
                Blockchain.Instance.SetBlockchain(commonBlockchain);
            }
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
                IServer clientServer = CreateBlockchainServer(client);

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

                        //Check transaction is valid with wallet sender
                        float walletBalance = Blockchain.Instance.GetBalance(transaction.WalletFrom);
                        if (walletBalance < transaction.Amount) //If the sender can't afford the transaction anymore
                        {
                            //The transaction is now invalid. We have no way of contacting the original sender, so the transaction is just discarded
                            _transactions.Dequeue();
                            continue;
                        }

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
                            //TODO log message
                            _transactions.Dequeue();
                            continue;
                        }

                        MinedBlocks++;

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
