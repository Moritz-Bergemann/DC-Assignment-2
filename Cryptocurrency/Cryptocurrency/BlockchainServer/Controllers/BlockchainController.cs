using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BlockchainServer.Models;
using APIClasses;

namespace BlockchainServer.Controllers
{
    public class BlockchainController : ApiController
    {
        [Route("api/blockchain")]
        [HttpGet]
        public IList<Block> GetBlockchain()
        {
            return BlockchainModel.Instance.Blockchain;
        }

        [Route("api/last-block")]
        [HttpGet]
        public Block GetLastBlock()
        {
            return BlockchainModel.Instance.LastBlock;
        }

        [Route("api/add-block")]
        [HttpPost]
        public string AddBlock(Block block)
        {
            try
            {
                BlockchainModel.Instance.AddBlock(block);
            }
            catch (ArgumentException a)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(a.Message)
                });
            }

            return "OK";
        }

        [Route("api/wallet/{walletId}")]
        [HttpGet]
        public Wallet GetWallet(uint walletId)
        {
            return BlockchainModel.Instance.GetWallet(walletId);
        }
    }
}