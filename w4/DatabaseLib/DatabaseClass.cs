using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLib
{
    public class DatabaseClass
    {
        List<DataStruct> dataStructs;
        public DatabaseClass()
        {
            Console.WriteLine("Making new database object...");

            dataStructs = new List<DatabaseLib.DataStruct>();

            DatabaseLib.DatabaseGenerator generator = new DatabaseGenerator();

            for (int ii = 0; ii < 1000; ii++)
            {
                DatabaseLib.DataStruct curDataStruct = new DataStruct();
                generator.GetNextAccount(out curDataStruct.pin, out curDataStruct.acctNo, 
                    out curDataStruct.firstName, out curDataStruct.lastName, out curDataStruct.balance, out curDataStruct.imagePath);

                dataStructs.Add(curDataStruct);
            }
        }

        public uint GetAcctNoByIndex(int index)
        {
            return dataStructs[index].acctNo;
        }
        public uint GetPINByIndex(int index)
        {
            return dataStructs[index].pin;
        }
        public string GetFirstNameByIndex(int index)
        {
            return dataStructs[index].firstName;
        }
        public string GetLastNameByIndex(int index)
        {
            return dataStructs[index].lastName;
        }
        public int GetBalanceByIndex(int index)
        {
            return dataStructs[index].balance;
        }
        public string GetImagePathByIndex(int index)
        {
            return dataStructs[index].imagePath;
        }
        public int GetNumRecords()
        {
            return dataStructs.Count;
        }
    }
}
