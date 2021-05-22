using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using APIClasses;
using WebServer.Models;

namespace WebServer.Controllers
{
    public class RegistryController : ApiController
    {
        [Route("api/register")]
        [HttpPost]
        public string Register(ClientData data)
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

            return "OK";
        }

        [Route("api/get-registered")]
        [HttpGet]
        public List<ClientData> GetRegistered()
        {
            return RegistryModel.Instance.GetRegistered();
        }

        [Route("api/report-downed")]
        [HttpPost]
        public string ReportDowned(ClientData downClient)
        {
            bool anyRemoved = RegistryModel.Instance.ReportDowned(downClient);

            return anyRemoved ? "OK" : "None removed";
        }
    }
}