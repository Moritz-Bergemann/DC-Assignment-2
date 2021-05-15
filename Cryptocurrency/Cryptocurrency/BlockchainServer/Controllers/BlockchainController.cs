using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BlockchainServer.Controllers
{
    public class BlockchainController : Controller
    {
        [Route("api/blockchain")]
        [HttpGet]
        public dynamic /*TODO what return type??*/ GetBlockchain()
        {
            return BlockchainModel.Instance.GetBlockchain();
        }
    }
}