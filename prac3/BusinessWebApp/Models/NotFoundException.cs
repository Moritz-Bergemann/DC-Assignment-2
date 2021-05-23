using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BusinessWebApp.Models
{
    [Serializable]
    class NotFoundException : Exception
    {
        public NotFoundException()
        {
        }

        public NotFoundException(string name) : base(name)
        {
        }
    }
}