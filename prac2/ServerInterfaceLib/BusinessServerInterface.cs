using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ServerInterfaceLib
{
    [ServiceContract]
    public interface BusinessServerInterface
    {
        [OperationContract]
        [FaultContract(typeof(DatabaseAccessFault))]
        void SearchByLastName(string query, out uint acctNo, out uint pin, out int bal, out string fName, out string lName, out int profileImageId);

        [OperationContract]
        [FaultContract(typeof(DatabaseAccessFault))]
        Stream GetProfileImageById(int id);

        [OperationContract]
        [FaultContract(typeof(DatabaseAccessFault))]
        int GetNumEntries();
    }
}
