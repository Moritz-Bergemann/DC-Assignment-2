using System;

namespace APIClasses
{
    public class JobData
    {
        public uint Id;
        public string Python;
        public string Result;
        public DateTime Timeout;

        public JobData(uint id, string python)
        {
            Id = id;
            Python = python;
            Result = null;
            Timeout = DateTime.MinValue;
        }
    }
}