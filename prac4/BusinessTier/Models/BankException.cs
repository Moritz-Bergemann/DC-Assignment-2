using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BusinessTier.Models
{
    public class BankException : Exception
    {
        public BankException()
        {
        }
        public BankException(string message)
            : base(message)
        {
        }
    }
}