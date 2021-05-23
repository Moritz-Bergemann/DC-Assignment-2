using APIClasses;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Miner.Models
{
    public class MinerModel
    {
        private static readonly string LOGS_PATH = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName, "miner-log.log");

        public RestClient _blockchainClient;

        public static MinerModel Instance
        {
            get;
        } = new MinerModel();

        private Queue<Transaction> _transactions;
        private  bool _mining;

        private MinerModel()
        {
            _blockchainClient = new RestClient("https://localhost:44367/");

            _transactions = new Queue<Transaction>();
            _mining = false;
        }

        public bool Mining
        {
            get => _mining;
        }

        public void AddTransaction(Transaction transaction)
        {
            //Add transaction to pending transactions list
            _transactions.Enqueue(transaction);
            
            //Try starting mining process (if it hasn't already)
            Task.Run(Mine);
        }

        private void Mine()
        {
            //Abort if already running
            if (_mining)
            {
                return;
            }

            //Set mining lock
            _mining = true;
            try
            {
                while (_transactions.Count > 0)
                {
                    //Get first element
                    Transaction transaction = _transactions.Peek();

                    //Get latest block from blockchain
                    Block lastBlock = GetLastBlockFromServer();

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

                    //Try posting block to blockchain
                    RestRequest addBlockRequest = new RestRequest("api/add-block");
                    addBlockRequest.AddJsonBody(block);
                    IRestResponse addBlockResponse = _blockchainClient.Post(addBlockRequest);

                    if (addBlockResponse.StatusCode != HttpStatusCode.OK) //If blockchain rejected request
                    {
                        //Check if the last block has changed (i.e. another miner has beat us to submission)
                        if (GetLastBlockFromServer().Hash.Equals(block.PrevHash))
                        {
                            Log($"Attempted to add transaction for block '{block}', but another block has been added since mining start - retrying");
                            //Another miner beat us to adding a block - try again (WITHOUT discarding the block)
                            continue;
                        }
                        else //If some other error occurs
                        {
                            //Something must have been invalid with this block. Discard it and try again with the next
                            Log($"Attempted to add transaction for block '{block}', rejected by server");
                            _transactions.Dequeue();
                            continue;
                        }
                    }

                    Log($"Block '{block}' mined successfully");

                    //If transaction was submitted to blockchain successfully, remove it from the queue so we can work on the next
                    _transactions.Dequeue();
                }
            }
            finally //End mining lock even if error occurs
            {
                _mining = false;
            }
        }

        private Wallet GetWalletFromServer(uint walletId)
        {
            RestRequest request = new RestRequest($"api/wallet/{walletId}");

            IRestResponse response = _blockchainClient.Get(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ArgumentException($"Failed to get wallet - {response.Content}");
            }

            return JsonConvert.DeserializeObject<Wallet>(response.Content);
        }

        private Block GetLastBlockFromServer()
        {
            RestRequest request = new RestRequest("api/last-block");

            IRestResponse response = _blockchainClient.Get(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new MinerException("Could not get latest block from blockchain server");
            }

            return JsonConvert.DeserializeObject<Block>(response.Content);
        }
        
        private void Log(string message)
        {
            //Add log number and increment log number
            StreamWriter logsFileWriter = File.AppendText(LOGS_PATH);
            logsFileWriter.WriteLine(message);
            logsFileWriter.Close();
        }
    }
}