using System;

namespace APIClasses
{
    /// <summary>
    /// Represents a cryptocurrency transaction
    /// </summary>
    public class Transaction
    {
        public uint WalletTo;
        public uint WalletFrom;
        public float Amount;
        public DateTime Timestamp;

        public Transaction() { }

        public Transaction(uint walletTo, uint walletFrom, float amount, DateTime timestamp)
        {
            WalletTo = walletTo;
            WalletFrom = walletFrom;
            Amount = amount;
            Timestamp = timestamp;
        }

        public Transaction(Transaction transaction)
        {
            WalletTo = transaction.WalletTo;
            WalletFrom = transaction.WalletFrom;
            Amount = transaction.Amount;
            Timestamp = transaction.Timestamp;
        }

        public override string ToString()
        {
            return $"[{WalletFrom}]-({Amount})-[{WalletTo}]";
        }
    }
}