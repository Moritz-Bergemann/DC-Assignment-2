using System;
using APIClasses;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Block = APIClasses.Block;

namespace BlockchainServer.Models
{
    public class BlockchainModel
    {
        public static BlockchainModel Instance
        {
            get;
        } = new BlockchainModel();

        private List<Block> _blockchain;
        private Block _lastBlock;

        private Dictionary<uint, Wallet> _wallets;

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
            _blockchain.Add(firstBlock);

            _lastBlock = firstBlock;

            _wallets = new Dictionary<uint, Wallet>();
        }

        public IList<Block> Blockchain
        {
            get => _blockchain;
        }

        public Block LastBlock
        {
            get => _lastBlock;
        }

        public void AddBlock(Block block)
        {
            if (!ValidateBlock(block, out string reason))
            {
                throw new ArgumentException($"Invalid block - {reason}");
            }

            _blockchain.Add(block);
        }

        private bool ValidateBlock(Block block, out string reason)
        {
            reason = null;

            //Block ID must be higher than all others
            //Assuming the blockchain is valid, we can just check the latest
            if (block.Id <= _lastBlock.Id)
            {
                reason = "ID must be higher than all others";
                return false;
            }

            //From wallet ID must have enough coins
            if (GetWallet(block.Id).Balance < block.Amount)
            {
                reason = "From wallet has insufficient coins";
                return false;
            }

            //All values must be non-negative (implicit)
            if (block.Amount < 0)
            {
                reason = "Negative amounts";
                return false;
            }

            //Block offset must be divisible by 5
            if (block.BlockOffset % 5 != 0)
            {
                reason = "Block offset must be multiple of 5";
                return false;
            }

            //Previous block hash must match previous block's hash
            if (!block.PrevHash.Equals(_lastBlock.Hash))
            {
                reason = "Previous hash does not match actual previous hash";
                return false;
            }

            //Hash must start with '12345' & end with '54321'
            if (!Block.CheckHashRule(block.Hash))
            {
                reason = "Hash does not start with '12345' & end with '54321'";
                return false;
            }

            //Calculated hash must be the same
            if (Block.CalculateHash(block).Equals(block.Hash))
            {
                reason = "Calculated hash gave different result";
                return false;
            }

            return true;
        }

        public Wallet GetWallet(uint id)
        {
            Wallet wallet;

            bool found = _wallets.TryGetValue(id, out wallet);

            //If wallet wasn't found, create it
            if (!found)
            {
                _wallets[id] = new Wallet(id);
            }

            return wallet;
        }
    }
}