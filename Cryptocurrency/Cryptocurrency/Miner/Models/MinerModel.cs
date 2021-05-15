using System;
using APIClasses;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using RestSharp;

namespace Miner.Models
{
    public class MinerModel
    {
        public RestClient _blockchainClient;

        public static MinerModel Instance
        {
            get;
        } = new MinerModel();

        private List<Transaction> _transactions;

        private MinerModel()
        {
            _transactions = new List<Transaction>();
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
            _transactions.Add(transaction);
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
    }
}