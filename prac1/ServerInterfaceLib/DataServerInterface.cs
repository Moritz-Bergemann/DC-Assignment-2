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
        /// <summary>
        /// Retrieves the number of entries in the database.
        /// </summary>
        /// <returns>Number of entries</returns>
        [OperationContract]
        int GetNumEntries();
    
        /// <summary>
        /// Retrieves all details (excluding profile image) for a entry in the database.
        /// </summary>
        /// <param name="index">Index of profile</param>
        [OperationContract]
        [FaultContract(typeof(DatabaseAccessFault))]
        void GetValuesForEntry(int index, out uint acctNo, out uint pin, out int bal, out string fName, out string lName);

        /// <summary>
        /// Retrieves the profile image for a given user.
        /// </summary>
        /// <param name="index">Index of profile</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(DatabaseAccessFault))]
        Stream GetImageForEntry(int index);
    }
}
