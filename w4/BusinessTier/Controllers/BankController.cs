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
        [Route("api/Account")]
        [HttpPost]
        public AccountData CreateAccount(CreateAccountData createData)
        {
            return BusinessModel.Instance.CreateAccount(createData);
        }

        [Route("api/Account/{accountId}")]
        [HttpGet]
        public AccountData GetAccount(uint accountId)
        {
            return BusinessModel.Instance.GetAccount(accountId);
        }

        [Route("api/User/{userId}")]
        [HttpGet]
        public UserData GetUser(uint userId)
        {
            return BusinessModel.Instance.GetUser(userId);
        }

        [Route("api/User")]
        [HttpPost]
        public UserData CreateUser(CreateUserData createData)
        {
            return BusinessModel.Instance.CreateUser(createData);
        }

        [Route("api/Transaction")]
        [HttpPost]
        public TransactionData MakeTransaction(CreateTransactionData createData)
        {
            return BusinessModel.Instance.MakeTransaction(createData);
        }

        [Route("api/Account/{accountId}/transactions")]
        [HttpGet]
        public List<TransactionData> SeeSentTransactionsForAccount(uint accountId)
        {
            return BusinessModel.Instance.GetSentTransactions(accountId);
        }

        [Route("api/User/{userId}/accounts")]
        [HttpGet]
        public List<AccountData> GetAccountsForUser(uint userId)
        {
            return BusinessModel.Instance.GetAccountsByUser(userId);
        }

        [Route("api/Account/{accountId}/deposit")]
        [HttpPost]
        public AccountData MakeDeposit(uint accountId, MoneyData moneyData)
        {
            BusinessModel.Instance.Deposit(accountId, moneyData.amount);
            return BusinessModel.Instance.GetAccount(accountId);
        }

        [Route("api/Account/{accountId}/withdraw")]
        [HttpPost]
        public AccountData MakeWithdrawal(uint accountId, MoneyData moneyData)
        {
            BusinessModel.Instance.Withdraw(accountId, moneyData.amount);
            return BusinessModel.Instance.GetAccount(accountId);
        }
    }
}
