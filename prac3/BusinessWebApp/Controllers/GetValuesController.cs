using APIClasses;
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
    public class GetValuesController : ApiController
    {
        // GET api/<controller>/5
        public ProfileData Get(int index)
        {
            try
            {
                return BusinessModel.Instance.GetProfileByIndex(index);
            }
            catch (ArgumentException a)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(a.Message)
                });
            }
        }
    }
}