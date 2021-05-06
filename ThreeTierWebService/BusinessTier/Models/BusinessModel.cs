using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using APIClasses;
using Newtonsoft.Json;
using RestSharp;

namespace BusinessTier.Models
{
    public class BusinessModel
    {
        private static readonly string dataTierUri = "https://localhost:44311/";
        private RestClient _client;

        public static BusinessModel Instance
        {
            get;
        } = new BusinessModel();

        private BusinessModel()
        {
            _client = new RestClient(dataTierUri);
        }

        public AccountData CreateAccount(CreateAccountData createData)
        {
            //Request account creation and get the account ID
            RestRequest request = new RestRequest("api/Account");
            request.AddJsonBody(createData);
            IRestResponse response = _client.Post(request);
            uint accountId = JsonConvert.DeserializeObject<uint>(response.Content);

            SaveData();

            //Get the details of created account
            return GetAccount(accountId);
        }

        public UserData CreateUser(CreateUserData createData)
        {
            //Request User creation and get the User ID
            RestRequest request = new RestRequest("api/User");
            request.AddJsonBody(createData);
            IRestResponse response = _client.Post(request);
            uint userId = JsonConvert.DeserializeObject<uint>(response.Content);

            SaveData();

            //Get the details of created User
            return GetUser(userId);
        }

        public AccountData GetAccount(uint accountId)
        {
            RestRequest request = new RestRequest("api/Account/" + accountId);
            IRestResponse response = _client.Get(request);

            return JsonConvert.DeserializeObject<AccountData>(response.Content);
        }

        public List<AccountData> GetAccountsByUser(uint userId)
        {
            RestRequest request = new RestRequest("api/Account/User/" + userId);
            IRestResponse response = _client.Get(request);

            return JsonConvert.DeserializeObject<List<AccountData>>(response.Content);
        }

        public UserData GetUser(uint userId)
        {
            RestRequest request = new RestRequest("api/User/" + userId);
            IRestResponse response = _client.Get(request);

            return JsonConvert.DeserializeObject<UserData>(response.Content);
        }

        public AccountData Deposit(uint accountId, uint amount)
        {
            RestRequest request = new RestRequest("api/Account/" + accountId + "/deposit");
            IRestResponse response = _client.Post(request);

            SaveData();

            return GetAccount(accountId);
        }

        public AccountData Withdraw(uint accountId, uint amount)
        {
            RestRequest request = new RestRequest("api/Account/" + accountId + "/withdraw");
            IRestResponse response = _client.Post(request);

            SaveData();

            return GetAccount(accountId);
        }

        /// <summary>
        /// Return a list of transactions that the given account has sent.
        /// </summary>
        /// <param name="accountId"></param> ID of account that was creator of transactions to get.
        /// <returns></returns>
        public List<TransactionData> GetSentTransactions(uint accountId)
        {
            RestRequest request = new RestRequest("api/Transaction/");
            IRestResponse response = _client.Get(request);
            List<TransactionData> allTransactions = JsonConvert.DeserializeObject<List<TransactionData>>(response.Content);

            //Get all transactions that the user sent
            return allTransactions.Where(transaction => transaction.SenderAccountId == accountId).ToList();
        }

        public TransactionData MakeTransaction(CreateTransactionData createData)
        {
            //Make the transaction
            RestRequest request = new RestRequest("api/Transaction/");
            request.AddJsonBody(createData);
            IRestResponse response = _client.Post(request);

            uint transactionId = JsonConvert.DeserializeObject<uint>(response.Content);

            //Now Process the Transaction
            RestRequest processRequest = new RestRequest("api/admin/process-all");
            _client.Post(processRequest);

            SaveData();

            return GetTransaction(transactionId);
        }

        private void SaveData()
        {
            RestRequest request = new RestRequest("api/Admin/save");
            _client.Post(request);
        }

        private TransactionData GetTransaction(uint transactionId) //Private as not everyone should just be able to access transactions
        {
            RestRequest request = new RestRequest("api/Transaction/" + transactionId);
            IRestResponse response = _client.Get(request);

            return JsonConvert.DeserializeObject<TransactionData>(response.Content);
        }
    }
}