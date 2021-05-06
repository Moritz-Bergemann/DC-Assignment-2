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
            if (createData == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "Account creation data required"
                });
            }

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
            if (createData == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "Create user data required"
                });
            }

            return BusinessModel.Instance.CreateUser(createData);
        }

        [Route("api/transaction")]
        [HttpPost]
        public TransactionData MakeTransaction(CreateTransactionData createData)
        {
            if (createData == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "Create transaction data required"
                });
            }

            return BusinessModel.Instance.MakeTransaction(createData);
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
            if (moneyData == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "Money data required"
                });
            }

            BusinessModel.Instance.Deposit(accountId, moneyData);
            return BusinessModel.Instance.GetAccount(accountId);
        }

        [Route("api/account/{accountId}/withdraw")]
        [HttpPost]
        public AccountData MakeWithdrawal(uint accountId, [FromBody] MoneyData moneyData)
        {
            if (moneyData == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "Money data required"
                });
            }

            BusinessModel.Instance.Withdraw(accountId, moneyData);
            return BusinessModel.Instance.GetAccount(accountId);
        }
    }
}
