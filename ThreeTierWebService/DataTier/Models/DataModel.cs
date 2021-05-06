using System;
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
            try
            {
                this._userAccess.SelectUser(userId);
                this._userAccess.GetUserName(out fName, out lName);
            }
            catch (Exception e)
            {
                throw new BankDbNotFoundException("User ID not found");
            }
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
            try
            {
                this._accountAccess.SelectAccount(accountId);
            }
            catch (Exception)
            {
                throw new BankDbNotFoundException("Account ID not found");
            }

            try
            {
                this._accountAccess.Deposit(amount);
            }
            catch (Exception)
            {
                throw new BankDbInvalidException("Could not deposit to account");
            }
        }
        public void WithdrawFromAccount(uint accountId, uint amount)
        {
            try
            {
                this._accountAccess.SelectAccount(accountId);
            }
            catch (Exception)
            {
                throw new BankDbNotFoundException("Account ID not found");
            }

            try
            {
                this._accountAccess.Withdraw(amount);
            }
            catch (Exception)
            {
                throw new BankDbInvalidException("Invalid funds to withdraw");
            }
        }

        public uint GetAccountBalance(uint accountId)
        {
            try
            {
                this._accountAccess.SelectAccount(accountId);
                return this._accountAccess.GetBalance();
            }
            catch (Exception)
            {
                throw new BankDbNotFoundException("Account ID not found");
            }

        }

        public uint GetAccountOwner(uint accountId)
        {
            try
            {
                this._accountAccess.SelectAccount(accountId);
                return this._accountAccess.GetOwner();
            }
            catch (Exception)
            {
                throw new BankDbNotFoundException("Account ID not found");
            }
        }

        public List<uint> GetAccountIdsByUser(uint userId)
        {
            try
            {
                return this._accountAccess.GetAccountIDsByUser(userId);
            }
            catch (Exception)
            {
                throw new BankDbNotFoundException("User ID not found");
            }
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
            try
            {
                this._transactionAccess.SelectTransaction(transactionId);
            }
            catch (Exception)
            {
                throw new BankDbNotFoundException("Transaction ID not found");
            }
            return this._transactionAccess.GetSendrAcct();
        }

        public uint GetTransactionReceiver(uint transactionId)
        {
            try
            {
                this._transactionAccess.SelectTransaction(transactionId);
            }
            catch (Exception)
            {
                throw new BankDbNotFoundException("Transaction ID not found");
            }
            return this._transactionAccess.GetRecvrAcct();
        }

        public uint GetTransactionAmount(uint transactionId)
        {
            try
            {
                this._transactionAccess.SelectTransaction(transactionId);
            }
            catch (Exception)
            {
                throw new BankDbNotFoundException("Transaction ID not found");
            }
            return this._transactionAccess.GetAmount();
        }

        public List<uint> GetTransactions()
        {
            return this._transactionAccess.GetTransactions();
        }

        public void ProcessAllTransactions()
        {
            //TODO handle whatever this throws
            this._bankDb.ProcessAllTransactions();
        }

        public void Save()
        {
            this._bankDb.SaveToDisk();
        }
    }
}