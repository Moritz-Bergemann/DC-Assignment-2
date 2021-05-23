using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIClasses
{
    public class BlockchainServerData
    {
        public string Address;
        public uint Port;

        public BlockchainServerData()
        {
        }

        public BlockchainServerData(string address, uint port)
        {
            Address = address;
            Port = port;
        }
    }
}
