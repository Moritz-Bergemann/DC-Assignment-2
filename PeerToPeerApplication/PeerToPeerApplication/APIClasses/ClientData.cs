using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIClasses
{
    public class ClientData
    {
        public string Address;
        public uint Port;

        public ClientData(string address, uint port)
        {
            Address = address;
            Port = port;
        }

        public ClientData(ClientScoreData scoreData)
        {
            Address = scoreData.Address;
            Port = scoreData.Port;
        }
    }
}
