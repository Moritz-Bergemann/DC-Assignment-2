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
        public void ProcessAllTransactions()
        {
            DataModel.Instance.ProcessAllTransactions();
        }

        [Route("api/Admin/save")]
        [HttpPost]
        public void Save()
        {
            DataModel.Instance.Save();
        }
    }
}