using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApplication
{
    public class ServerLaunchException : Exception
    {
        public ServerLaunchException()
        {
        }

        public ServerLaunchException(string message)
            : base(message)
        {
        }
    }
}