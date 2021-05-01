using System.Collections.Generic;
using BankDB;

namespace DataTier.Models
{
    /// <summary>
    /// Implements functions for interaction with BankDB database system.
    /// </summary>
    public class DataModel
    {
        public static DataModel Instance
        {
            get;
        } = new DataModel();

        private readonly BankDB.BankDB _bankDb;
        private UserAccessInterface _userAccess;
        private AccountAccessInterface _accountAccess;
        private TransactionAccessInterface _transactionAccess;

        private DataModel()
        {
            this._bankDb = new BankDB.BankDB();
            this._userAccess = _bankDb.GetUserAccess();
            this._accountAccess = _bankDb.GetAccountInterface();
            this._transactionAccess = _bankDb.GetTransactionInterface();
        }

        // USER MANAGEMENT
        public uint CreateUser(string fName, string lName)
        {
            //Create new user
            uint newUserId = this._userAccess.CreateUser();

            //Set new user details
            this._userAccess.SelectUser(newUserId);
            this._userAccess.SetUserName(fName, lName);

            return newUserId;
        }

        public void GetUserName(uint userId, out string fName, out string lName)
        {
            this._userAccess.SelectUser(userId);
            this._userAccess.GetUserName(out fName, out lName);
        }

        public List<uint> GetUserIds()
        {
            return this._userAccess.GetUsers();
        }

        // ACCOUNT MANAGEMENT
        public uint CreateAccount(uint userId)
        {
            return this._accountAccess.CreateAccount(userId);
        }

        public void DepositToAccount(uint accountId, uint amount)
        {
            this._accountAccess.SelectAccount(accountId);
            this._accountAccess.Deposit(amount);
        }
        public void WithdrawFromAccount(uint accountId, uint amount)
        {
            this._accountAccess.SelectAccount(accountId);
            this._accountAccess.Withdraw(amount);
        }

        public uint GetAccountBalance(uint accountId)
        {
            this._accountAccess.SelectAccount(accountId);
            return this._accountAccess.GetBalance();
        }

        public uint GetAccountOwner(uint accountId)
        {
            this._accountAccess.SelectAccount(accountId);
            return this._accountAccess.GetOwner();
        }

        public List<uint> GetAccountIdsByUser(uint userId)
        {
            return this._accountAccess.GetAccountIDsByUser(userId);
        }

        // TRANSACTION MANAGEMENT
        public uint MakeTransaction(uint senderAccountID, uint recvrAccountId, uint amount)
        {
            //Create the transaction
            uint newTransactionId = this._transactionAccess.CreateTransaction();

            //Set transaction details
            this._transactionAccess.SelectTransaction(newTransactionId);
            this._transactionAccess.SetSendr(senderAccountID);
            this._transactionAccess.SetRecvr(recvrAccountId);
            this._transactionAccess.SetAmount(amount);

            return newTransactionId;
        }

        public uint GetTransactionSender(uint transactionId)
        {
            this._transactionAccess.SelectTransaction(transactionId);
            return this._transactionAccess.GetSendrAcct();
        }

        public uint GetTransactionReceiver(uint transactionId)
        {
            this._transactionAccess.SelectTransaction(transactionId);
            return this._transactionAccess.GetRecvrAcct();
        }

        public uint GetTransactionAmount(uint transactionId)
        {
            this._transactionAccess.SelectTransaction(transactionId);
            return this._transactionAccess.GetAmount();
        }

        public List<uint> GetTransactions()
        {
            return this._transactionAccess.GetTransactions();
        }

        public void ProcessAllTransactions()
        {
            //Try to complete the transaction (FIXME - how does error handling work)
            this._bankDb.ProcessAllTransactions();
        }

        public void Save()
        {
            this._bankDb.SaveToDisk();
        }
    }
}