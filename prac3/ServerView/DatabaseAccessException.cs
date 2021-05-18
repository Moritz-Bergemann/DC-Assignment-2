using System;

namespace ServerView
{
    [Serializable]
    class DatabaseAccessException : Exception
    {
        public DatabaseAccessException()
        {
        }

        public DatabaseAccessException(string name) : base(name)
        {
        }
    }
}