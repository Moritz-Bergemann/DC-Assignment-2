using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace APIClasses
{
    /// <summary>
    /// Represents a block in the blockchain. Contains all hashing functionality.
    /// </summary>
    public class Block
    {
        public uint Id;
        public List<Transaction> Transactions;
        public uint BlockOffset;
        public byte[] PrevHash;
        public byte[] Hash;

        /// <summary>
        /// Calculates the hash for this block based on all of its other fields.
        /// </summary>
        /// <returns>The hash for this block if it could be created</returns>
        /// <exception cref="ArgumentException">If block details are invalid</exception>
        public byte[] FindHash()
        {
            bool validHash = false;
            byte[] hash = null;

            while (!validHash)
            {
                BlockOffset += 5;

                hash = HashValues(this);

                //Validate computed hash
                validHash = CheckHashRule(hash);
            }

            return hash;
        }

        /// <summary>
        /// Returns a SHA-256 hash for the given block based on its current values. It does NOT modify the block offset, and this hash is NOT guaranteed to be valid.
        /// </summary>
        /// <param name="block">Block to hash the values of</param>
        /// <returns></returns>
        public static byte[] HashValues(Block block)
        {
            //Brute-forcing time
            string concatString = "";
            concatString += block.Id.ToString();
            foreach (Transaction transaction in block.Transactions)
            {
                concatString += transaction.ToString();
            }
            concatString += block.BlockOffset.ToString();
            if (block.PrevHash != null)
            {
                concatString += block.PrevHash.ToString();
            }

            SHA256 sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(concatString));
        }

        /// <summary>
        /// Verifies the imported hash follows the hashing rule (the first 5 digits of the hash converted to int32 are '12345')
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static bool CheckHashRule(byte[] hash)
        {
            if (hash.Length < 5)
            {
                throw new ArgumentException("Hash too short to be valid");
            }

            //Checking first 5 digits of hash are the same
            string validator = "12345";
            int[] first5Bytes = (from hashByte in hash.Take(5) select Convert.ToInt32(hashByte)).ToArray();
            string first5Digits = string.Join("", first5Bytes).Substring(0, 5);

            bool valid = first5Digits.Equals(validator);

            if (valid)
            {
                string bytesString = Convert.ToBase64String(hash);
                Debug.WriteLine(bytesString);
            }

            return first5Digits.Equals(validator);
        }

        public override string ToString()
        {
            string transactionString = "";
            foreach (Transaction transaction in Transactions)
            {
                transactionString += transaction.ToString();
            }

            return $"ID {Id}, Transactions {{{transactionString}}}, Offset{BlockOffset}, PrevHash {Convert.ToBase64String(Hash)}, Hash {Convert.ToBase64String(PrevHash)}";
        }
    }
}
