using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServerInterfaceLib
{
    [DataContract]
    public class DatabaseAccessFault
    {
        private string report;
        public DatabaseAccessFault(string message)
        {
            this.report = message;
        }

        [DataMember]
        public string Message
        {
            get { return this.report; }
            set { this.report = value; }
        }
    }
}
