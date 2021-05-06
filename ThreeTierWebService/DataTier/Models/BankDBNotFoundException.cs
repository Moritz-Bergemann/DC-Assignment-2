using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataTier.Models
{
    public class BankDbNotFoundException : Exception
    {
        public BankDbNotFoundException()
        {
        }
        public BankDbNotFoundException(string message)
            : base(message)
        {
        }
    }
}