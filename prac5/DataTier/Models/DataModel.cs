using System;
using System.Collections.Generic;
using System.IO;
using BankDB;

namespace DataTier.Models
{
    /// <summary>
    /// Implements functions for interaction with BankDB database system.
    /// </summary>
    public class DataModel
    {
        private static readonly string LOGS_PATH = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName, "blockchain-logs.log");

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
                Log($"Attempted retrieve user '{userId}', not found");
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
                Log($"Attempted deposit account '{accountId}', not found");
                throw new BankDbNotFoundException("Account ID not found");
            }

            try
            {
                _accountAccess.Deposit(amount);
            }
            catch (Exception e)
            {
                Log($"Attempted deposit account '{accountId}', error - {e.Message}");
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
                Log($"Attempted withdraw account '{accountId}', not found");
                throw new BankDbNotFoundException("Account ID not found");
            }

            try
            {
                _accountAccess.Withdraw(amount);
            }
            catch (Exception e)
            {
                Log($"Attempted withdraw account '{accountId}', error - {e.Message}");
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
                Log($"Attempted get account balance '{accountId}', not found");
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
                Log($"Attempted get account owner '{accountId}', not found");
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
                Log($"Attempted get account IDs by user '{userId}', not found");
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
                Log($"Attempted get transaction sender for '{transactionId}', not found");
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
                Log($"Attempted get transaction receiver for '{transactionId}', not found");
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
                Log($"Attempted get transaction amount for '{transactionId}', not found");
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
            try
            {
                _bankDb.ProcessAllTransactions();
            }
            catch (Exception e)
            {
                Log($"Transaction failed - {e.Message}");
                throw new BankDbInvalidException($"Transaction failed - {e.Message}");
            }
        }

        public void Save()
        {
            //Exception NOT caught - error here should crash service
            _bankDb.SaveToDisk();
        }

        private void Log(string message)
        {
            //Add log number and increment log number
            StreamWriter logsFileWriter = File.AppendText(LOGS_PATH);
            logsFileWriter.WriteLine(message);
            logsFileWriter.Close();
        }
    }
}