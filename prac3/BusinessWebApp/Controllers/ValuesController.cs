using BusinessWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BusinessTier;

namespace BusinessWebApp.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public int Get()
        {
            return BusinessModel.Instance.GetNumEntries();
        }
    }
}