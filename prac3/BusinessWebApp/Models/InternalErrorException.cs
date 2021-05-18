using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BusinessWebApp.Models
{
    [Serializable]
    class InternalErrorException : Exception
    {
        public InternalErrorException()
        {
        }

        public InternalErrorException(string name) : base(name)
        {
        }
    }
}