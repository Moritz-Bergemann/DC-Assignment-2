using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using APIClasses;
using DataTier.Models;

namespace DataTier.Controllers
{
    public class AccountController : ApiController
    {
        [Route("api/Account/User/{userId}")]
        [HttpGet]
        public IEnumerable<AccountData> GetAccountsByUser(uint userId)
        {
            List<uint> accountIds;
            try
            {
                accountIds = DataModel.Instance.GetAccountIdsByUser(userId);
            }
            catch (BankDbNotFoundException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "User with ID not found"
                });
            }

            List<AccountData> accountList = new List<AccountData>();

            //Add each of the user's accounts to the list
            foreach (uint accountId in accountIds)
            {
                AccountData data = new AccountData
                {
                    Id = accountId,
                    UserId = DataModel.Instance.GetAccountOwner(accountId),
                    Balance = DataModel.Instance.GetAccountBalance(accountId)
                };

                accountList.Add(data);
            }

            return accountList;
        }

        [Route("api/Account/{accountId}")]
        [HttpGet]
        public AccountData GetAccount(uint accountId)
        {
            AccountData data = new AccountData();

            try
            {
                data.Id = accountId;
                data.UserId = DataModel.Instance.GetAccountOwner(accountId);
                data.Balance = DataModel.Instance.GetAccountBalance(accountId);
            }
            catch (BankDbNotFoundException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "Account with ID not found"
                });
            }

            return data;
        }

        [Route("api/Account")]
        [HttpPost]
        public uint CreateAccount(CreateAccountData createData)
        {
            if (createData == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "Create account data required"
                });
            }

            return DataModel.Instance.CreateAccount(createData.UserId);
        }

        [Route("api/Account/{accountId}/deposit")]
        [HttpPost]
        public uint Deposit(uint accountId, MoneyData moneyData)
        {
            try
            {
                DataModel.Instance.DepositToAccount(accountId, moneyData.Amount);
            }
            catch (BankDbNotFoundException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "Account with ID not found"
                });
            }
            catch (BankDbInvalidException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "Could not deposit to account"
                });
            }

            return DataModel.Instance.GetAccountBalance(accountId);
        }

        [Route("api/Account/{accountId}/withdraw")]
        [HttpPost]
        public uint Withdraw(uint accountId, MoneyData moneyData)
        {
            try
            {
                DataModel.Instance.WithdrawFromAccount(accountId, moneyData.Amount);

            }
            catch (BankDbNotFoundException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "Account with ID not found"
                });
            }
            catch (BankDbInvalidException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "Could not withdraw - insufficient funds"
                });
            }

            return DataModel.Instance.GetAccountBalance(accountId);
        }
    }
}