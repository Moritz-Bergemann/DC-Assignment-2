namespace APIClasses
{
    public class JobData
    {
        public uint Id;
        public string Python;
        public string Result;

        public JobData(uint id, string python)
        {
            Id = id;
            Python = python;
            Result = null;
        }
    }
}