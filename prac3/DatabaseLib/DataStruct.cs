namespace DatabaseLib
{
    /// <summary>
    /// Struct representing a user in the database
    /// </summary>
    internal class DataStruct
    {
        public uint AcctNo;
        public uint Pin;
        public int Balance;
        public string FirstName;
        public string LastName;
        public int ImageNum;

        public DataStruct()
        {
            AcctNo = 0;
            Pin = 0;
            Balance = 0;
            FirstName = "";
            LastName = "";
            ImageNum = 0;
        }
    }
}