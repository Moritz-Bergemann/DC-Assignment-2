using System;
using System.Globalization;
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

                //Brute-forcing time
                string concatString = "";
                concatString += Id.ToString();
                concatString += FromWallet.ToString();
                concatString += ToWallet.ToString();
                concatString += Amount.ToString(CultureInfo.InvariantCulture);
                concatString += BlockOffset.ToString();
                concatString += PrevHash.ToString();

                SHA256 sha256 = SHA256.Create();
                hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(concatString));

                //Validate computed hash
                validHash = ValidateHash(hash);
            }

            return hash;
        }

        private static bool ValidateHash(byte[] hash)
        {
            if (hash.Length < 5)
            {
                throw new ArgumentException("Hash too short to be valid");
            }

            int[] validator1 = new int[] { 1, 2, 3, 4, 5 };
            int[] validator2 = new int[] { 5, 4, 3, 2, 1 };

            bool validHash = true;
            for (int ii = 0; ii < 5; ii++)
            {
                if (validator1[ii] != Convert.ToInt32(hash[ii]))
                {
                    validHash = false;
                    break;
                }
            }

            if (validHash) //Only do second hash if string is still valid
            {
                int jj = 0;
                for (int ii = hash.Length - 1; ii >= hash.Length - 5; ii--)
                {
                    if (validator2[jj] != Convert.ToInt32(hash[ii]))
                    {
                        validHash = false;
                        break;
                    }
                    jj++;
                }
            }

            return validHash;
        }
    }
}
