using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIClasses
{
    public class AccountData
    {
        public uint id;
        public uint userId;
        public uint balance;
    }

    public class CreateAccountData
    {
        public uint userId;
    }

    public class UserData
    {
        public uint id;
        public string fName;
        public string lName;
    }

    public class CreateUserData
    {
        public string fName;
        public string lName;
    }

    public class TransactionData
    {
        public uint id;
        public uint senderAccountId;
        public uint receiverAccountId;
        public uint amount;
    }

    public class CreateTransactionData
    {
        public uint senderAccountId;
        public uint receiverAccountId;
        public uint amount;
    }

    public class MoneyData
    {
        public uint amount;
    }
}
