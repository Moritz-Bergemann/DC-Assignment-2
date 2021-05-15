using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using APIClasses;

namespace BlockchainServer.Models
{
    public class BlockchainModel
    {
        public static BlockchainModel Instance
        {
            get;
        } = new BlockchainModel();

        private List<Block> _blockchain;

        private BlockchainModel()
        {
            _blockchain = new List<Block>();

            //Add first block to blockchain
            Block firstBlock = new Block()
            {
                Id = 0,
                Amount = 0,
                BlockOffset = 0,
                FromWallet = 0,
                ToWallet = 0,
                PrevHash = null
            };

            //Make hash for first block
            firstBlock.Hash = firstBlock.CalculateHash();

            _blockchain.Add();
        }

        private AddBlock(Block block)
        {

        }
    }
}