using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace APIClasses
{
    public class JobData
    {
        public uint Id;
        public string Python;

        public JobData(uint id, string python)
        {
            Id = id;
            Python = python;
        }
    }

    public class TransmitJobData
    {
        public uint Id;

        public string PythonEncoded
        {
            get;
            private set; //Read-only for outside world
        }
        public byte[] Hash
        {
            get;
            private set; //Read-only for outside world
        }

        public TransmitJobData(uint id)
        {
            Id = id;
            PythonEncoded = null;
            Hash = null;
        }

        /// <summary>
        /// Stores Base64-encoded python code and its corresponding SHA-256 hash in the object, ready for transmission
        /// </summary>
        /// <param name="python">Python 2 code</param>
        public void SetEncodedPython(string python)
        {
            //Byte-encode python script
            byte[] textBytes = Encoding.UTF8.GetBytes(python);

            //Base64 encode
            PythonEncoded = Convert.ToBase64String(textBytes);

            //Set SHA-256 hash
            byte[] encodedTextBytes = Encoding.UTF8.GetBytes(PythonEncoded);
            SHA256 sha256Hash = SHA256.Create();
            Hash = sha256Hash.ComputeHash(encodedTextBytes);
        }

        /// <summary>
        /// Checks if a newly calculated SHA-256 hash for the Base64 python script 
        /// </summary>
        /// <returns></returns>
        public bool CheckHash()
        {
            //Re-calculate SHA-256 hash for B64-encoded python script
            byte[] encodedBytes = Encoding.UTF8.GetBytes(PythonEncoded);
            SHA256 sha256Hash = SHA256.Create();
            byte[] newHash = sha256Hash.ComputeHash(encodedBytes);

            //Compare to existing SHA-256 hash
            return newHash.SequenceEqual(Hash);
        }

        public string GetDecodedPython()
        {
            if (PythonEncoded == null)
            {
                throw new ArgumentException("Base-64 encoded python has not been set!");
            }

            byte[] encodedBytes = Convert.FromBase64String(PythonEncoded);
            return Encoding.UTF8.GetString(encodedBytes);
        }
    }
}