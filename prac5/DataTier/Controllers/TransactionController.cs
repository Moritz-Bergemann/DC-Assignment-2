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
    public class TransactionController : ApiController
    {
        [Route("api/Transaction/")]
        [HttpGet]
        public List<TransactionData> GetTransactions()
        {
            List<uint> transactionIds = DataModel.Instance.GetTransactions();

            List<TransactionData> transactionsList = new List<TransactionData>();

            foreach (uint transactionId in transactionIds)
            {
                TransactionData data = new TransactionData
                {
                    Id = transactionId,
                    Amount = DataModel.Instance.GetTransactionAmount(transactionId),
                    SenderAccountId = DataModel.Instance.GetTransactionSender(transactionId),
                    ReceiverAccountId = DataModel.Instance.GetTransactionReceiver(transactionId)
                };

                transactionsList.Add(data);    
            }

            return transactionsList;
        }

        [Route("api/Transaction/{transactionId}")]
        public TransactionData GetTransaction(uint transactionId)
        {
            TransactionData data;

            try
            {
                data = new TransactionData
                {
                    Id = transactionId,
                    Amount = DataModel.Instance.GetTransactionAmount(transactionId),
                    SenderAccountId = DataModel.Instance.GetTransactionSender(transactionId),
                    ReceiverAccountId = DataModel.Instance.GetTransactionReceiver(transactionId)
                };
            }
            catch (BankDbNotFoundException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent($"Transaction with ID {transactionId} not found")
                });
            }

            return data;
        }

        [Route("api/Transaction/")]
        [HttpPost]
        public uint MakeTransaction(CreateTransactionData createData)
        {
            if (createData == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Create transaction data required")
                });
            }

            //Run the transaction (this will attempt to perform it and fail if unsuccessful)
            return DataModel.Instance.MakeTransaction(createData.SenderAccountId, createData.ReceiverAccountId,
                createData.Amount);
        }
    }
}