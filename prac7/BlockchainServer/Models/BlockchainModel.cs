using System;
using APIClasses;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RestSharp;
using Block = APIClasses.Block;

namespace BlockchainServer.Models
{
    public class BlockchainModel
    {
        private static readonly string LOGS_PATH = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName, "blockchain-logs.log");

        public static BlockchainModel Instance
        {
            get;
        } = new BlockchainModel();

        private List<Block> _blockchain;
        private Block _lastBlock;

        private Dictionary<uint, Wallet> _wallets;

        private List<RestClient> _otherBlockchains;

        private BlockchainModel()
        {
            _blockchain = new List<Block>();

            //Add first block to blockchain
            Block firstBlock = new Block()
            {
                Id = 0,
                Amount = 0,
                BlockOffset = 0,
                WalletFrom = 0,
                WalletTo = 0,
                PrevHash = null
            };
            //Make hash for first block (pre-calculated for server responsiveness)
            firstBlock.Hash = Convert.FromBase64String("ey2/qy8RjUTjebZ8P26S1INJ6r/89qvtjDpoBNs5wIU="); //NOTE: CHANGE THIS IF YOU CHANGE THE INITIAL BLOCK

            _blockchain.Add(firstBlock);

            _lastBlock = firstBlock;

            _wallets = new Dictionary<uint, Wallet>();

            //Give wallet 0 infinite money
            Wallet wallet0 = new Wallet(0) {Balance = float.PositiveInfinity};
            _wallets[0] = wallet0;

            _otherBlockchains = new List<RestClient>();
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
            //Validate block
            if (!ValidateBlock(block, out string reason))
            {
                Log($"Requested to add block {block} but could not add - {reason}");
                throw new ArgumentException($"Invalid block - {reason}");
            }

            //Add block to blockchain
            _blockchain.Add(block);

            //Update wallets
            GetWallet(block.WalletFrom).Balance -= block.Amount;
            GetWallet(block.WalletTo).Balance += block.Amount;

            //Check other Blockchains to make sure we have the most common one
            CheckMostCommonBlockchain();
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
            if (GetWallet(block.WalletFrom).Balance < block.Amount)
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
            if (!block.PrevHash.SequenceEqual(_lastBlock.Hash))
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

        public Wallet GetWallet(uint id)
        {
            Wallet wallet;

            bool found = _wallets.TryGetValue(id, out wallet);

            //If wallet wasn't found, create it
            if (!found)
            {
                wallet = new Wallet(id);
                _wallets[id] = wallet;
            }

            return wallet;
        }

        public void AddBlockchainServer(BlockchainServerData data)
        {
            //Create rest client for blockchain server
            RestClient client = new RestClient($"https://{data.Address}:{data.Port}");
            _otherBlockchains.Add(client);
        }

        /// <summary>
        /// Checks the server has the most common version of the blockchain, and updates it if it does not
        /// </summary>
        private void CheckMostCommonBlockchain()
        {
            //Don't perform the check if there aren't any other blockchains
            if (_otherBlockchains.Count == 0)
            {
                return;
            }

            //Get the most common blockchain
            List<Block> commonBlockchain = GetMostCommonBlockchain();

            //Check most common blockchain is the same as this client's one, override it if not
            if (!_lastBlock.Hash.SequenceEqual(commonBlockchain.Last().Hash))
            {
                Log($"Found did not have most common blockchain (our hash is '{Convert.ToBase64String(_lastBlock.Hash)}', most common is '{Convert.ToBase64String(commonBlockchain.Last().Hash)}'), downloading");

                _blockchain = commonBlockchain;
                _lastBlock = commonBlockchain.Last();
            }
        }

        /// <summary>
        /// Retrieves the most common blockchain from all clients in the registry.
        /// </summary>
        public List<Block> GetMostCommonBlockchain()
        {
            List<byte[]> blockHashes = new List<byte[]>(); //Last block hash
            List<RestClient> blockClients = new List<RestClient>(); //A client endpoint with this hash
            List<int> blockCounts = new List<int>(); //Number of clients with this hash

            foreach (RestClient client in _otherBlockchains)
            {
                //Get the latest block hash for the client
                //Connect to client's server
                RestRequest lastBlockRequest = new RestRequest("api/last-block");

                IRestResponse response = client.Get(lastBlockRequest);
                Block lastBlock = JsonConvert.DeserializeObject<Block>(response.Content);

                byte[] lastBlockHash = lastBlock.Hash;

                //If this last hash is new
                if (!blockHashes.Any(h => h.Equals(lastBlockHash)))
                {
                    //Update all data for this new hash
                    blockHashes.Add(lastBlockHash);
                    blockClients.Add(client);
                    blockCounts.Add(1);
                }
                else
                {
                    //Find index where the matching hash is stored
                    int index = blockHashes.FindLastIndex(h => h.Equals(lastBlockHash));

                    //Add 1 to blockchain common-ness count for this hash
                    blockCounts[index]++;
                }
            }

            //Get index of the most common last block hash
            int largestIndex = blockCounts.IndexOf(blockCounts.Max());

            //Get a client that has the most common blockchain
            RestClient mostCommonClient = blockClients[largestIndex];

            //Get that client's version of the blockchain
            RestRequest mostCommonChainRequest = new RestRequest("api/blockchain");
            IRestResponse mostCommonChainResponse = mostCommonClient.Get(mostCommonChainRequest);

            List<Block> mostCommonChain = JsonConvert.DeserializeObject<List<Block>>(mostCommonChainResponse.Content);

            return mostCommonChain;
        }

        private void Log(string message)
        {
            //Add log number and increment log number
            StreamWriter logsFileWriter = File.AppendText(LOGS_PATH);
            logsFileWriter.WriteLine(message);
            logsFileWriter.Close();
        }
    }
}