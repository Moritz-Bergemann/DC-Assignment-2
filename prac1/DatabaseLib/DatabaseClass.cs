using System;
using System.Collections.Generic;
using System.Drawing;

namespace DatabaseLib
{
    /// <summary>
    /// Class representing a database of users. Can be accessed to retrieve individual user details
    /// </summary>
    public class DatabaseClass
    {
        List<DataStruct> dataStructs;
        private DatabaseGenerator _generator;
        public DatabaseClass(int numUsers)
        {
            dataStructs = new List<DatabaseLib.DataStruct>();

            _generator = new DatabaseGenerator();

            //Generate users using generator
            for (int ii = 0; ii < numUsers; ii++)
            {
                DatabaseLib.DataStruct curDataStruct = new DataStruct();
                _generator.GetNextAccount(out curDataStruct.Pin, out curDataStruct.AcctNo, 
                    out curDataStruct.FirstName, out curDataStruct.LastName, out curDataStruct.Balance, out curDataStruct.ImageNum);

                dataStructs.Add(curDataStruct);
            }
        }

        public uint GetAcctNoByIndex(int index)
        {
            return dataStructs[index].AcctNo;
        }
        public uint GetPinByIndex(int index)
        {
            return dataStructs[index].Pin;
        }
        public string GetFirstNameByIndex(int index)
        {
            return dataStructs[index].FirstName;
        }
        public string GetLastNameByIndex(int index)
        {
            return dataStructs[index].LastName;
        }
        public int GetBalanceByIndex(int index)
        {
            return dataStructs[index].Balance;
        }
        public int GetNumRecords()
        {
            return dataStructs.Count;
        }
        public Bitmap GetImageByIndex(int index)
        {
            return _generator.GetImageByNum(dataStructs[index].ImageNum);
        }
    }
}
