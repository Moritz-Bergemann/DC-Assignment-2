using System;
using System.Collections.Generic;
using System.IO;
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
        public static string ResPath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName, "_resources");

        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("user")]
        public ActionResult UserPage()
        {
            return View();
        }

        [Route("account")]
        public ActionResult AccountPage()
        {
            return View();
        }

        [Route("transaction")]
        public ActionResult TransactionPage()
        {
            return View();
        }
    }
}