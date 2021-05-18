using APIClasses;
using BusinessWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BusinessWebApp.Controllers
{
    public class GetValuesController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public ProfileData Get(int index)
        {
            ProfileData result = new ProfileData();

            DataModel.Instance.GetValuesForEntry(index, out result.Acct, out result.Pin, out result.Bal, out result.FName, out result.LName);

            return result;
        }
    }
}