using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using APIClasses;
using BusinessTier.Models;

namespace BusinessTier.Controllers
{
    public class BankController : ApiController
    {
        [Route("api/account")]
        [HttpPost]
        public AccountData CreateAccount(CreateAccountData createData)
        {
            return BusinessModel.Instance.CreateAccount(createData);
        }

        [Route("api/account/{accountId}")]
        [HttpGet]
        public AccountData GetAccount(uint accountId)
        {
            return BusinessModel.Instance.GetAccount(accountId);
        }

        [Route("api/user/{userId}")]
        [HttpGet]
        public UserData GetUser(uint userId)
        {
            return BusinessModel.Instance.GetUser(userId);
        }

        [Route("api/user")]
        [HttpPost]
        public UserData CreateUser(CreateUserData createData)
        {
            return BusinessModel.Instance.CreateUser(createData);
        }

        [Route("api/transaction")]
        [HttpPost]
        public TransactionData MakeTransaction(CreateTransactionData createData)
        {
            return BusinessModel.Instance.MakeTransaction(createData);
        }

        [Route("api/account/{accountId}/transactions")]
        [HttpGet]
        public List<TransactionData> SeeSentTransactionsForAccount(uint accountId)
        {
            return BusinessModel.Instance.GetSentTransactions(accountId);
        }

        [Route("api/user/{userId}/accounts")]
        [HttpGet]
        public List<AccountData> GetAccountsForUser(uint userId)
        {
            return BusinessModel.Instance.GetAccountsByUser(userId);
        }

        [Route("api/account/{accountId}/deposit")]
        [HttpPost]
        public AccountData MakeDeposit(uint accountId, MoneyData moneyData)
        {
            BusinessModel.Instance.Deposit(accountId, moneyData.Amount);
            return BusinessModel.Instance.GetAccount(accountId);
        }

        [Route("api/account/{accountId}/withdraw")]
        [HttpPost]
        public AccountData MakeWithdrawal(uint accountId, MoneyData moneyData)
        {
            BusinessModel.Instance.Withdraw(accountId, moneyData.Amount);
            return BusinessModel.Instance.GetAccount(accountId);
        }
    }
}
