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

        private List<RegistryData> _registry;

        private RegistryModel()
        {
            _registry = new List<RegistryData>();
        }

        public void Register(RegistryData newData)
        {
            //Input validation
            bool validAddress = Uri.TryCreate($"net.tcp://{newData.Address}:{newData.Port}", UriKind.Absolute, out Uri _);
            if (!validAddress)
                throw new ArgumentException("Invalid client address given");

            if (newData.Port > 65353)
                throw new ArgumentException("Invalid client port given");

            bool duplicate = _registry.Any(data => data.Address.Equals(newData.Address) && (data.Port == newData.Port));

            if (duplicate)
            {
                throw new RegistryException("Cannot add - already in database");
            }

            _registry.Add(newData);
        }

        public List<RegistryData> GetRegistered()
        {
            //Randomise registry elements so no one client is unfairly advantaged
            return _registry.OrderBy(a => Guid.NewGuid()).ToList();
        }
    }
}