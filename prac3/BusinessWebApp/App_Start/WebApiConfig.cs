using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace BusinessWebApp.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // This configures the WebApi to work as we want
            // This binds custom HTTP Attribute paths in controllers to their functions. 
            //We're not going to need this now, but keep it in for later.
            config.MapHttpAttributeRoutes();
        //This binds API routes in the way we expect. You'll need to go to 
        http://localhost:xxxx/api/whatever to get to the api.
             //Just remember! that /api/ is critical!
            config.Routes.MapHttpRoute(
            name: "DefaultApi",
            routeTemplate: "api/{controller}/{id}",
            defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}

