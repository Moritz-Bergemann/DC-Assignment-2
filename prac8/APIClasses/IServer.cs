using System.Collections.Generic;
using System.ServiceModel;

namespace APIClasses
{
    [ServiceContract]
    public interface IServer
    {
        [OperationContract]
        Block GetLastBlock();

        [OperationContract]
        List<Block> GetBlockchain();

        [OperationContract]
        void PutTransaction(Transaction transaction);
    }
}
