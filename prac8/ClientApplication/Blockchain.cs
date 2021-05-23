using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using APIClasses;

namespace ClientApplication
{
    class Blockchain
    {
        public static Blockchain Instance
        {
            get;
        } = new Blockchain();

        private List<Block> _chain;
        private Dictionary<uint, float> _wallets;

        private Blockchain()
        {
            _chain = null;
            LastBlock = null;

            _wallets = new Dictionary<uint, float>();
            _wallets[0] = float.PositiveInfinity;
        }


        public Block LastBlock { get; private set; }

        public List<Block> Chain
        {
            get => _chain;
            private set
            {
                _chain = value;
                LastBlock = value[value.Count - 1];
            }
        }

        /// <summary>
        /// Adds a block to the blockchain
        /// </summary>
        /// <exception cref="BlockchainException">
        /// If imported block is not valid to be added to the blockchain
        /// </exception>
        public void AddBlock(Block block)
        {
            //Validate block
            if (!ValidateBlock(block, out string reason))
            {
                throw new BlockchainException($"Invalid block - {reason}");
            }

            //Add block to blockchain
            _chain.Add(block);

            //Update wallets
            SetBalance(block.WalletFrom, GetBalance(block.WalletFrom) - block.Amount);
            SetBalance(block.WalletTo, GetBalance(block.WalletTo) + block.Amount);
        }

        private bool ValidateBlock(Block block, out string reason)
        {
            reason = null;

            //Block ID must be higher than all others
            //Assuming the blockchain is valid, we can just check the latest
            if (block.Id <= LastBlock.Id)
            {
                reason = "ID must be higher than all others";
                return false;
            }

            //From wallet ID must have enough coins
            if (GetBalance(block.WalletFrom)< block.Amount)
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
            if (!block.PrevHash.SequenceEqual(LastBlock.Hash))
            {
                reason = "Previous hash does not match actual previous hash";
                return false;
            }

            //Hash must start with '12345' & end with '54321'
            if (!Block.CheckHashRule(block.Hash))
            {
                reason = "Hash does not start with '12345'";
                return false;
            }

            //Calculated hash must be the same
            if (Block.HashValues(block).Equals(block.Hash))
            {
                reason = "Calculated hash gave different result";
                return false;
            }

            return true;
        }

        public void SetBlockchain(List<Block> newChain)
        {
            //Set blockchain
            Chain = newChain;

            //Reset wallets
            _wallets = new Dictionary<uint, float>();
            _wallets[0] = float.PositiveInfinity;

            //Reconstruct wallets based on blockchain
            foreach (Block block in Chain)
            {
                SetBalance(block.WalletFrom, GetBalance(block.WalletFrom) - block.Amount);

                SetBalance(block.WalletTo, GetBalance(block.WalletTo) + block.Amount);
            }
        }

        public float GetBalance(uint id)
        {
            float balance;

            bool found = _wallets.TryGetValue(id, out balance);

            //If wallet wasn't found, create it
            if (!found)
            {
                balance = 0;
                _wallets[id] = 0;
            }

            return balance;
        }

        public void SetBalance(uint id, float balance)
        {
            _wallets[id] = balance;
        }
    }
}
