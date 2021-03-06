using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using APIClasses;
using Miner.Models;

namespace Miner.Controllers
{
    public class MinerController : ApiController
    { 
        [Route("api/add-transaction")]
        [HttpPost]
        public void AddTransaction(Transaction transaction)
        {
            try
            {
                MinerModel.Instance.AddTransaction(transaction);
            }
            catch (ArgumentException a)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent($"Could not add transaction - {a.Message}")
                });
            }
        }

        [Route("api/status")]
        [HttpGet]
        public string GetStatus()
        {
            return MinerModel.Instance.Mining ? "Mining" : "Idle";
        }
    }
}