using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIClasses
{
    public class RegistryConfirmData
    {
        public bool Success;
        public string Message;

        public RegistryConfirmData(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }

    public class RegistryData
    {
        public string Address;
        public uint Port;

        public RegistryData(string address, uint port)
        {
            Address = address;
            Port = port;
        }
    }
}
