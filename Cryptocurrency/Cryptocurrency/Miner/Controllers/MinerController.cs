using System.Web.Http;
using APIClasses;

namespace Miner.Controllers
{
    public class MinerController : ApiController
    { 
        [Route("api/make-transaction")]
        [HttpPost]
        public void AddTransaction(Transaction transaction)
        {
            MinerModel.Instance.AddTransaction();
        }
    }
}
