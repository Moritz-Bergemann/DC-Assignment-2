using System;

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