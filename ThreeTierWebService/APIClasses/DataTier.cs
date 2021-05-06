namespace APIClasses
{
    public class AccountData
    {
        public uint Id;
        public uint UserId;
        public uint Balance;
    }

    public class CreateAccountData
    {
        public uint UserId;
    }

    public class UserData
    {
        public uint Id;
        public string FName;
        public string LName;
    }

    public class CreateUserData
    {
        public string FName;
        public string LName;
    }

    public class TransactionData
    {
        public uint Id;
        public uint SenderAccountId;
        public uint ReceiverAccountId;
        public uint Amount;
    }

    public class CreateTransactionData
    {
        public uint SenderAccountId;
        public uint ReceiverAccountId;
        public uint Amount;
    }

    public class MoneyData
    {
        public uint Amount;
    }
}
