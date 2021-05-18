using APIClasses;
using BusinessWebApp.Models;
using System.Web.Http;

namespace BusinessWebApp.Controllers
{
    public class SearchController : ApiController
    {
        // POST api/<controller>
        public ProfileData Post([FromBody] SearchData searchData)
        {
            DataModel.Instance.SearchByLastName(searchData.searchStr);

            ProfileData result = new ProfileData();
            DataModel.Instance.GetSearchedProfileDetails(out result.Acct, out result.Pin, out result.Bal, out result.FName, out result.LName);
            
            return result;
        }
    }
}