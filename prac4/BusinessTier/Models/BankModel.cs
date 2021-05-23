using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using APIClasses;
using Newtonsoft.Json;
using RestSharp;

namespace BusinessTier.Models
{
    public class BankModel
    {
        private static readonly string LOGS_PATH = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName, "blockchain-logs.log");

        private static readonly string dataTierUri = "https://localhost:44311/";
        private RestClient _client;

        public static BankModel Instance
        {
            get;
        } = new BankModel();

        private BankModel()
        {
            _client = new RestClient(dataTierUri);
        }

        public AccountData CreateAccount(CreateAccountData createData)
        {
            //Request account creation and get the account ID
            RestRequest request = new RestRequest("api/Account");
            request.AddJsonBody(createData);
            IRestResponse response = _client.Post(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log($"Error creating account {createData.UserId} - {response.Content}");
                throw new BankException(response.Content);
            }
            
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

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log($"Error creating user {createData.FName} {createData.LName} - {response.Content}");
                throw new BankException(response.Content);
            }

            uint userId = JsonConvert.DeserializeObject<uint>(response.Content);

            SaveData();

            //Get the details of created User
            return GetUser(userId);
        }

        public AccountData GetAccount(uint accountId)
        {
            RestRequest request = new RestRequest("api/Account/" + accountId);
            IRestResponse response = _client.Get(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log($"Error getting account {accountId} - {response.Content}");
                throw new BankException(response.Content);
            }

            return JsonConvert.DeserializeObject<AccountData>(response.Content);
        }

        public List<AccountData> GetAccountsByUser(uint userId)
        {
            RestRequest request = new RestRequest("api/Account/User/" + userId);
            IRestResponse response = _client.Get(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log($"Error getting accounts by user ID {userId} - {response.Content}");
                throw new BankException(response.Content);
            }

            return JsonConvert.DeserializeObject<List<AccountData>>(response.Content);
        }

        public UserData GetUser(uint userId)
        {
            RestRequest request = new RestRequest("api/User/" + userId);
            IRestResponse response = _client.Get(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log($"Error getting user by ID {userId} - {response.Content}");
                throw new BankException(response.Content);
            }

            return JsonConvert.DeserializeObject<UserData>(response.Content);
        }

        public AccountData Deposit(uint accountId, MoneyData amount)
        {
            RestRequest request = new RestRequest("api/Account/" + accountId + "/deposit");
            request.AddJsonBody(amount);
            IRestResponse response = _client.Post(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log($"Error depositing ${amount.Amount} to account {accountId} - {response.Content}");
                throw new BankException(response.Content);
            }

            SaveData();

            return GetAccount(accountId);
        }

        public AccountData Withdraw(uint accountId, MoneyData amount)
        {
            RestRequest request = new RestRequest("api/Account/" + accountId + "/withdraw");
            request.AddJsonBody(amount);
            IRestResponse response = _client.Post(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log($"Error withdrawing ${amount.Amount} from account {accountId} - {response.Content}");
                throw new BankException(response.Content);
            }

            SaveData();

            return GetAccount(accountId);
        }

        public TransactionData MakeTransaction(CreateTransactionData createData)
        {
            //Make the transaction
            RestRequest request = new RestRequest("api/Transaction/");
            request.AddJsonBody(createData);
            IRestResponse response = _client.Post(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log($"Error making transaction of ${createData.Amount} from {createData.SenderAccountId} to {createData.ReceiverAccountId} - {response.Content}");
                throw new BankException(response.Content);
            }

            uint transactionId = JsonConvert.DeserializeObject<uint>(response.Content);

            //Now Process the Transaction
            RestRequest processRequest = new RestRequest("api/admin/process-all");
            IRestResponse processResponse = _client.Post(processRequest);

            if (processResponse.StatusCode != HttpStatusCode.OK)
            {
                Log($"Error processing transaction of ${createData.Amount} from {createData.SenderAccountId} to {createData.ReceiverAccountId} - {response.Content}");
                throw new BankException(response.Content);
            }

            SaveData();

            return GetTransaction(transactionId);
        }

        private void SaveData()
        {
            RestRequest request = new RestRequest("api/Admin/save");
            IRestResponse response = _client.Post(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log($"Error saving bank data - {response.Content}");
                throw new BankException(response.Content);
            }
        }

        private TransactionData GetTransaction(uint transactionId) //Private as not everyone should just be able to access transactions
        {
            RestRequest request = new RestRequest("api/Transaction/" + transactionId);
            IRestResponse response = _client.Get(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log($"Error getting transaction with ID {transactionId} - {response.Content}");
                throw new BankException(response.Content);
            }

            return JsonConvert.DeserializeObject<TransactionData>(response.Content);
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