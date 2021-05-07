using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using APIClasses;
using WebServer.Models;

namespace WebServer.Controllers
{
    public class RegistryController : ApiController
    {
        [System.Web.Http.Route("api/register")]
        [System.Web.Http.HttpPost]
        public string Register(RegistryData data)
        {
            try
            {
                RegistryModel.Instance.Register(data);
            }
            catch (ArgumentException a)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "Invalid data",
                    Content = new StringContent(a.Message)
                });
            }
            catch (RegistryException r)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "Service already in registry",
                    Content = new StringContent(r.Message)
                });
            }

            return "Added";
        }

        [System.Web.Http.Route("api/get-registered")]
        [System.Web.Http.HttpGet]
        public List<RegistryData> getRegistered()
        {
            return RegistryModel.Instance.GetRegistered();
        }
    }
}