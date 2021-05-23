using System.Net;
using System.Net.Http;
using APIClasses;
using BusinessWebApp.Models;
using System.Web.Http;
using BusinessTier;

namespace BusinessWebApp.Controllers
{
    public class SearchController : ApiController
    {
        // POST api/<controller>
        public ProfileData Post([FromBody] SearchData searchData)
        {
            try
            {
                return BusinessModel.Instance.SearchByLastName(searchData.searchStr);
            }
            catch (NotFoundException nf)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(nf.Message)
                });
            }
            catch (InternalErrorException ie)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ie.Message)
                });
            }
        }
    }
}