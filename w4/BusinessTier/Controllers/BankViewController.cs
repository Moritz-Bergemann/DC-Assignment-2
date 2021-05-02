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
        private RestClient _client = new RestClient("https://localhost:44347/");

        //Web utility access
        private UserData GetUserData(int userId)
        {
            RestRequest request = new RestRequest("user/" + userId); 
            IRestResponse response = _client.Get(request);

            UserData data = JsonConvert.DeserializeObject<UserData>(response.Content);

            //TODO error handling (what to do here??)

            return data;
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
            ViewBag.User = GetUserData(userId);

            return View();
        }
    }
}