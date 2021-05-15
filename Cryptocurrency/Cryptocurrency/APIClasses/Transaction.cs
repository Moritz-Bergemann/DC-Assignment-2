using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIClasses
{
    public class Transaction
    {
        public uint WalletTo;
        public uint WalletFrom;
        public float Amount;

        public Transaction(uint walletTo, uint walletFrom, float amount)
        {
            WalletTo = walletTo;
            WalletFrom = walletFrom;
            Amount = amount;
        }
    }
}
