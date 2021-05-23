using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApplication
{
    public class BlockchainException : Exception
    {
        public BlockchainException()
        {
        }

        public BlockchainException(string message)
            : base(message)
        {
        }
    }
}
