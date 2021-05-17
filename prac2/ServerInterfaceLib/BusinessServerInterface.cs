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
        bool SearchByLastName(string query);

        [OperationContract]
        void GetSearchedProfileDetails(out uint acctNo, out uint pin, out int bal, out string fName, out string lName);

        [OperationContract]
        [FaultContract(typeof(DatabaseAccessFault))]
        Stream GetSearchedProfileImage();

        [OperationContract]
        [FaultContract(typeof(DatabaseAccessFault))]
        int GetNumEntries();
    }
}
