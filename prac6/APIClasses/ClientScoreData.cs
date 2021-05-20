using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIClasses
{
    public class ClientScoreData
    {
        public string Address;
        public uint Port;
        public int Score;

        public ClientScoreData()
        {
        }

        public ClientScoreData(string address, uint port)
        {
            Address = address;
            Port = port;
            Score = 0;
        }

        public ClientScoreData(ClientData clientData)
        {
            Address = clientData.Address;
            Port = clientData.Port;
            Score = 0;
        }
    }
}
