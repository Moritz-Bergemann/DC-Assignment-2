using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ServerInterfaceLib
{
    //Make this a service contract as it is a service interface
    [ServiceContract]
    public interface DataServerInterface
    {
        //Each of these are service function contracts. They need to be tagged as OperationContracts.
        
         [OperationContract]
        int GetNumEntries();
    
        [OperationContract]
        [FaultContract(typeof(DatabaseAccessFault))]
        void GetValuesForEntry(int index, out uint acctNo, out uint pin, out int bal, out string fName, out string lName);

        [OperationContract]
        [FaultContract(typeof(DatabaseAccessFault))]
        Stream GetImageForEntry(int index);
    }
}
