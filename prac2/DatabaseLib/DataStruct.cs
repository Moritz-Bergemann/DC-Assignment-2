using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLib
{
    internal class DataStruct
    {
        public uint acctNo;
        public uint pin;
        public int balance;
        public string firstName;
        public string lastName;
        public string imagePath;

        public DataStruct()
        {
            acctNo = 0;
            pin = 0;
            balance = 0;
            firstName = "";
            lastName = "";
            imagePath = null;
        }
    }
}
