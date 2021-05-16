using APIClasses;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Miner.Models
{
    public class MinerModel
    {
        public RestClient _blockchainClient;

        public static MinerModel Instance
        {
            get;
        } = new MinerModel();

        private Queue<Transaction> _transactions;
        private  bool _mining;

        private MinerModel()
        {
            _transactions = new Queue<Transaction>();
            _mining = false;
        }

        public void AddTransaction(Transaction transaction)
        {
            //Validate transactions
            if (transaction.Amount < 0)
            {
                throw new ArgumentException("Transaction amount cannot be negative");
            }

            Wallet senderWallet = GetWalletFromServer(transaction.WalletFrom);

            if (senderWallet.Balance < transaction.Amount)
            {
                throw new ArgumentException("Sender cannot afford transaction");
            }

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

                    //Check transaction is still valid with wallet sender
                    Wallet walletFrom = GetWalletFromServer(transaction.WalletFrom);
                    if (walletFrom.Balance < transaction.Amount) //If the sender can't afford the transaction anymore
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
                        FromWallet = transaction.WalletFrom,
                        ToWallet = transaction.WalletTo,
                        PrevHash = lastBlock.Hash
                    };

                    //Calculate hash for block (this will take a long time)
                    block.Hash = Block.CalculateHash(block);

                    //Try posting block to blockchain
                    RestRequest addBlockRequest = new RestRequest("api/add-block");
                    addBlockRequest.AddJsonBody(block);
                    IRestResponse addBlockResponse = _blockchainClient.Post(addBlockRequest);

                    if (addBlockResponse.StatusCode != HttpStatusCode.OK) //If blockchain rejected request
                    {
                        //Check if the last block has changed (i.e. another miner has beat us to submission)
                        if (!GetLastBlockFromServer().Hash.Equals(block.PrevHash))
                        {
                            //Try again (WITHOUT discarding the)
                            continue;
                        }
                        else //If some other error occurs
                        {
                            //Something must have been invalid with this block. Discard it and try again with the next
                            _transactions.Dequeue();
                            continue;
                        }
                    }

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
            RestRequest request = new RestRequest("api/get-wallet");

            IRestResponse response = _blockchainClient.Get(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ArgumentException($"Failed to get wallet - {response.Content}");
            }

            return JsonConvert.DeserializeObject<Wallet>(response.Content);
        }

        private Block GetLastBlockFromServer()
        {
            RestRequest request = new RestRequest("api/");

            IRestResponse response = _blockchainClient.Get(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new MinerException("Could not get latest block from blockchain server");
            }

            return JsonConvert.DeserializeObject<Block>(response.Content);
        }
    }
}