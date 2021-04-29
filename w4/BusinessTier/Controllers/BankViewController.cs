using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BusinessTier.Controllers
{
    public class BankViewController : Controller
    {
        [Route("User")]
        public ActionResult Users()
        {
            return View();
        }
    }
}