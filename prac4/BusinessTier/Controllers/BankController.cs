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

            try
            {
                return BankModel.Instance.CreateAccount(createData);
            } catch (BankException b)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent($"Could not create account - {b.Message}")
                });
            }
        }

        [Route("api/account/{accountId}")]
        [HttpGet]
        public AccountData GetAccount(uint accountId)
        {
            try
            {
                return BankModel.Instance.GetAccount(accountId);
            }
            catch (BankException b)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent($"Could not get account - {b.Message}")
                });
            }
        }

        [Route("api/user/{userId}")]
        [HttpGet]
        public UserData GetUser(uint userId)
        {
            try
            {
                return BankModel.Instance.GetUser(userId);

            }
            catch (BankException b)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent($"Could not get user - {b.Message}")
                });
            }
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

            try
            {
                return BankModel.Instance.CreateUser(createData);
            }
            catch (BankException b)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent($"Could not create user - {b.Message}")
                });
            }
        }

        [Route("api/transaction")]
        [HttpPost]
        public TransactionData MakeTransaction(CreateTransactionData createData)
        {
            if (createData == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Create transaction data required")
                });
            }

            try
            {
                return BankModel.Instance.MakeTransaction(createData);
            }
            catch (BankException b)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent($"Could not make transaction - {b.Message}")
                });
            }
        }

        [Route("api/user/{userId}/accounts")]
        [HttpGet]
        public List<AccountData> GetAccountsForUser(uint userId)
        {
            try
            {
                return BankModel.Instance.GetAccountsByUser(userId);
            }
            catch (BankException b)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent($"Could not get accounts for user - {b.Message}")
                });
            }
        }

        [Route("api/account/{accountId}/deposit")]
        [HttpPost]
        public AccountData MakeDeposit(uint accountId, MoneyData moneyData)
        {
            if (moneyData == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Money data required")
                });
            }

            try
            {
                BankModel.Instance.Deposit(accountId, moneyData);
                return BankModel.Instance.GetAccount(accountId);

            }
            catch (BankException b)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent($"Could not make deposit - {b.Message}")
                });
            }
        }

        [Route("api/account/{accountId}/withdraw")]
        [HttpPost]
        public AccountData MakeWithdrawal(uint accountId, [FromBody] MoneyData moneyData)
        {
            if (moneyData == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Money data required")
                });
            }

            try
            {
                BankModel.Instance.Withdraw(accountId, moneyData);
                return BankModel.Instance.GetAccount(accountId);
            }
            catch (BankException b)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent($"Could not make withdrawal - {b.Message}")
                });
            }
        }
    }
}
