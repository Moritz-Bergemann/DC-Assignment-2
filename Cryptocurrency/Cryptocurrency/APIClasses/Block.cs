using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace APIClasses
{
    public class Block
    {
        public uint Id;
        public uint FromWallet;
        public uint ToWallet;
        public float Amount;
        public uint BlockOffset;
        public byte[] PrevHash;
        public byte[] Hash;

        /// <summary>
        /// Calculates the hash for this block based on all of its other fields.
        /// </summary>
        /// <returns>The hash for this block if it could be created</returns>
        /// <exception cref="ArgumentException">If block details are invalid</exception>
        public byte[] CalculateHash()
        {
            bool validHash = false;
            byte[] hash = null;

            while (!validHash)
            {
                BlockOffset += 5;

                hash = CalculateHash(this);

                //Validate computed hash
                validHash = CheckHashRule(hash);
            }

            return hash;
        }

        public static byte[] CalculateHash(Block block)
        {
            //Brute-forcing time
            string concatString = "";
            concatString += block.Id.ToString();
            concatString += block.FromWallet.ToString();
            concatString += block.ToWallet.ToString();
            concatString += block.Amount.ToString(CultureInfo.InvariantCulture);
            concatString += block.BlockOffset.ToString();
            concatString += block.PrevHash.ToString();

            SHA256 sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(concatString));
        }

        public static bool CheckHashRule(byte[] hash)
        {
            if (hash.Length < 5)
            {
                throw new ArgumentException("Hash too short to be valid");
            }

            int[] validator = { 1, 2, 3, 4, 5 };
            int[] first5Ints = (from hashByte in hash.Take(5) select Convert.ToInt32(hashByte)).ToArray();

            return first5Ints.Equals(validator);
        }
    }
}
