using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using APIClasses;

namespace WebServer.Models
{
    public class RegistryModel
    {
        //Singleton management
        public static RegistryModel Instance
        {
            get;
        } = new RegistryModel();

        private List<RegistryData> registry;

        private RegistryModel()
        {
            registry = new List<RegistryData>();
        }

        public void Register(RegistryData newData)
        {
            //Input validation
            Uri uriResult;
            bool validAddress = Uri.TryCreate(newData.Address, UriKind.Absolute, out uriResult)
                          && uriResult.Scheme == Uri.UriSchemeHttp;
            if (!validAddress)
                throw new ArgumentException("Invalid client address given");

            if (newData.Port > 65353)
                throw new ArgumentException("Invalid client port given");

            bool duplicate = registry.Any(data => data.Address.Equals(newData.Address) && (data.Port == newData.Port));

            if (duplicate)
            {
                throw new RegistryException("Cannot add - already in database");
            }

            registry.Add(newData);
        }

        public List<RegistryData> GetRegistered()
        {
            return registry;
        }
    }
}