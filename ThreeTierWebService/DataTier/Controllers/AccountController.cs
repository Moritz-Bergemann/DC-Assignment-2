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
            List<uint> accountIds = DataModel.Instance.GetAccountIdsByUser(userId);
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

            data.Id = accountId;
            data.UserId = DataModel.Instance.GetAccountOwner(accountId);
            data.Balance = DataModel.Instance.GetAccountBalance(accountId);

            return data;
        }

        [Route("api/Account")]
        [HttpPost]
        public uint CreateAccount(CreateAccountData createData)
        {
            return DataModel.Instance.CreateAccount(createData.UserId);
        }

        [Route("api/Account/{accountId}/deposit")]
        [HttpPost]
        public uint Deposit(uint accountId, MoneyData moneyData)
        {
            DataModel.Instance.DepositToAccount(accountId, moneyData.Amount);

            return DataModel.Instance.GetAccountBalance(accountId);
        }

        [Route("api/Account/{accountId}/withdraw")]
        [HttpPost]
        public uint Withdraw(uint accountId, MoneyData moneyData)
        {
            DataModel.Instance.WithdrawFromAccount(accountId, moneyData.Amount);

            return DataModel.Instance.GetAccountBalance(accountId);
        }
    }
}