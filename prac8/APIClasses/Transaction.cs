namespace APIClasses
{
    public class Transaction
    {
        public uint WalletTo;
        public uint WalletFrom;
        public float Amount;

        public Transaction() { }

        public Transaction(uint walletTo, uint walletFrom, float amount)
        {
            WalletTo = walletTo;
            WalletFrom = walletFrom;
            Amount = amount;
        }
    }
}