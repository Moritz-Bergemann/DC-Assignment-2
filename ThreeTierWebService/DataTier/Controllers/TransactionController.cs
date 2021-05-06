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
            TransactionData data = new TransactionData
            {
                Id = transactionId,
                Amount = DataModel.Instance.GetTransactionAmount(transactionId),
                SenderAccountId = DataModel.Instance.GetTransactionSender(transactionId),
                ReceiverAccountId = DataModel.Instance.GetTransactionReceiver(transactionId)
            };


            return data;
        }

        [Route("api/Transaction/")]
        [HttpPost]
        public uint MakeTransaction(CreateTransactionData createData)
        {
            //Run the transaction (this will attempt to perform it and fail if unsuccessful)
            return DataModel.Instance.MakeTransaction(createData.SenderAccountId, createData.ReceiverAccountId,
                createData.Amount);
        }
    }
}