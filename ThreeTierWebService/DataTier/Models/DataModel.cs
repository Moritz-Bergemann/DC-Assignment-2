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
            _bankDb = new BankDB.BankDB();
            _userAccess = _bankDb.GetUserAccess();
            _accountAccess = _bankDb.GetAccountInterface();
            _transactionAccess = _bankDb.GetTransactionInterface();
        }

        // USER MANAGEMENT
        public uint CreateUser(string fName, string lName)
        {
            //Create new user
            uint newUserId = _userAccess.CreateUser();

            //Set new user details
            _userAccess.SelectUser(newUserId);
            _userAccess.SetUserName(fName, lName);

            return newUserId;
        }

        public void GetUserName(uint userId, out string fName, out string lName)
        {
            try
            {
                _userAccess.SelectUser(userId);
                _userAccess.GetUserName(out fName, out lName);
            }
            catch (Exception e)
            {
                throw new BankDbNotFoundException("User ID not found");
            }
        }

        public List<uint> GetUserIds()
        {
            return _userAccess.GetUsers();
        }

        // ACCOUNT MANAGEMENT
        public uint CreateAccount(uint userId)
        {
            return _accountAccess.CreateAccount(userId);
        }

        public void DepositToAccount(uint accountId, uint amount)
        {
            try
            {
                _accountAccess.SelectAccount(accountId);
            }
            catch (Exception)
            {
                throw new BankDbNotFoundException("Account ID not found");
            }

            try
            {
                _accountAccess.Deposit(amount);
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
                _accountAccess.SelectAccount(accountId);
            }
            catch (Exception)
            {
                throw new BankDbNotFoundException("Account ID not found");
            }

            try
            {
                _accountAccess.Withdraw(amount);
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
                _accountAccess.SelectAccount(accountId);
                return _accountAccess.GetBalance();
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
                _accountAccess.SelectAccount(accountId);
                return _accountAccess.GetOwner();
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
                return _accountAccess.GetAccountIDsByUser(userId);
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
            uint newTransactionId = _transactionAccess.CreateTransaction();

            //Set transaction details
            _transactionAccess.SelectTransaction(newTransactionId);
            _transactionAccess.SetSendr(senderAccountID);
            _transactionAccess.SetRecvr(recvrAccountId);
            _transactionAccess.SetAmount(amount);

            return newTransactionId;
        }

        public uint GetTransactionSender(uint transactionId)
        {
            try
            {
                _transactionAccess.SelectTransaction(transactionId);
            }
            catch (Exception)
            {
                throw new BankDbNotFoundException("Transaction ID not found");
            }
            return _transactionAccess.GetSendrAcct();
        }

        public uint GetTransactionReceiver(uint transactionId)
        {
            try
            {
                _transactionAccess.SelectTransaction(transactionId);
            }
            catch (Exception)
            {
                throw new BankDbNotFoundException("Transaction ID not found");
            }
            return _transactionAccess.GetRecvrAcct();
        }

        public uint GetTransactionAmount(uint transactionId)
        {
            try
            {
                _transactionAccess.SelectTransaction(transactionId);
            }
            catch (Exception)
            {
                throw new BankDbNotFoundException("Transaction ID not found");
            }
            return _transactionAccess.GetAmount();
        }

        public List<uint> GetTransactions()
        {
            return _transactionAccess.GetTransactions();
        }

        public void ProcessAllTransactions()
        {
            //TODO handle whatever this throws
            try
            {
                _bankDb.ProcessAllTransactions();
            }
            catch (Exception e)
            {
                throw new BankDbInvalidException($"Transaction failed - {e.Message}");
            }
        }

        public void Save()
        {
            //Exception NOT caught - error here should crash service
            _bankDb.SaveToDisk(); 
        }
    }
}