using APIClasses;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BusinessTier;

namespace BusinessWebApp.Controllers
{
    public class GetValuesController : ApiController
    {
        // GET api/values/5
        public ProfileData Get(int id)
        {
            try
            {
                return BusinessModel.Instance.GetProfileByIndex(id);
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