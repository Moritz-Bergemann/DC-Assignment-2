using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DataTier.Models;

namespace DataTier.Controllers
{
    public class AdminController : ApiController
    {
        [Route("api/Admin/process-all")]
        [HttpPost]
        public string ProcessAllTransactions()
        {
            try
            {
                DataModel.Instance.ProcessAllTransactions();
            }
            catch (BankDbInvalidException b)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("Failed to process transaction. Some or all transactions have not been processed.")
                });
            }

            return "OK";
        }

        [Route("api/Admin/save")]
        [HttpPost]
        public string Save()
        {
            DataModel.Instance.Save();

            return "OK";
        }
    }
}