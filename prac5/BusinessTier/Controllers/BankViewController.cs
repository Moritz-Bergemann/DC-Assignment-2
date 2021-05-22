using System;
using System.IO;
using System.Web.Mvc;

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
        public ContentResult UserPage()
        {
            return GetPage("UserPage.html");
        }

        [Route("account")]
        public ContentResult AccountPage()
        {
            return GetPage("AccountPage.html");
        }

        [Route("transaction")]
        public ContentResult TransactionPage()
        {
            return GetPage("TransactionPage.html");
        }

        private ContentResult GetPage(string filename)
        {
            //Read HTML data in from file
            string pagePath = Path.Combine(ResPath, filename);
            string pageContent = System.IO.File.ReadAllText(pagePath);

            return new ContentResult()
            {
                ContentType = "text/html",
                Content = pageContent
            };
        }
    }
}