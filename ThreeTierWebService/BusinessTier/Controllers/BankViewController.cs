using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using APIClasses;
using Newtonsoft.Json;
using RestSharp;

namespace BusinessTier.Controllers
{
    public class BankViewController : Controller
    {
        private RestClient _businessClient = new RestClient("https://localhost:44347/");

        //TODO remove all this
        //Web utility access
        private UserData GetUserData(int userId)
        {
            RestRequest request = new RestRequest("api/user/" + userId); 
            IRestResponse response = _businessClient.Get(request);

            UserData data = JsonConvert.DeserializeObject<UserData>(response.Content);

            //TODO error handling (what to do here??)

            return data;
        }

        private AccountData GetAccountData(int accountId)
        {
            RestRequest request = new RestRequest("api/account/" + accountId);
            IRestResponse response = _businessClient.Get(request);
            
            AccountData data = JsonConvert.DeserializeObject<AccountData>(response.Content);

            //TODO error handling (what to do here??)

            return data;
        }

        private List<TransactionData> GetAccountTransactions(int accountId)
        {
            RestRequest request = new RestRequest($"api/account/{accountId}/transactions");
            IRestResponse response = _businessClient.Get(request);

            List<TransactionData> data = JsonConvert.DeserializeObject<List<TransactionData>>(response.Content);

            //TODO error handling (what to do here??)

            return data;

        }

        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("UserPage")]
        public ActionResult Users()
        {
            return View();
        }

        [Route("signin")]
        public ActionResult SignIn()
        {
            return View();
        }

        [Route("user/{userId}")]
        public ActionResult UserPage(int userId)
        {
            ViewBag.UserData = GetUserData(userId);

            return View();
        }

        [Route("account/{accountId}")]
        public ActionResult AccountPage(int accountId)
        {
            ViewBag.AccountData = GetAccountData(accountId);
            ViewBag.Transactions = GetAccountTransactions(accountId);

            return View();
        }
    }
}