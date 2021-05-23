using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Miner.Models
{
    [Serializable]
    class MinerException : Exception
    {
        public MinerException()
        {
        }

        public MinerException(string name) : base(name)
        {
        }
    }
}