using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

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
