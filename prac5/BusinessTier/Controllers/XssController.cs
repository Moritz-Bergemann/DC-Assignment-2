using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BusinessTier.Controllers
{
    public class XssController : Controller
    {
        public static string ResPath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName, "_resources");

        [Route("xss")]
        public ContentResult XssDemo()
        {
            //Read HTML data in from file
            string pagePath = Path.Combine(ResPath, "XssDemo.html");
            string pageContent = System.IO.File.ReadAllText(pagePath);

            return new ContentResult()
            {
                ContentType = "text/html",
                Content = pageContent
            };
        }
    }
}