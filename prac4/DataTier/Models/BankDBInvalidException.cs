using System;

namespace DataTier.Models
{
    public class BankDbInvalidException : Exception
    {
        public BankDbInvalidException()
        {
        }
        public BankDbInvalidException(string message)
            : base(message)
        {
        }
    }
}