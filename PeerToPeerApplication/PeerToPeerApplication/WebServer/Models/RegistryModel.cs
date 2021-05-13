using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

        private List<ClientScoreData> _registry;

        private RegistryModel()
        {
            _registry = new List<ClientScoreData>();
        }

        public void Register(ClientData newData)
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

            _registry.Add(new ClientScoreData(newData));
        }

        public List<ClientData> GetRegistered()
        {
            List<ClientData> registeredNoScores = _registry.Select(clientScore => new ClientData(clientScore)).ToList();

            //Randomise registry elements so no one client is unfairly advantaged
            return registeredNoScores.OrderBy(a => Guid.NewGuid()).ToList();
        }

        public bool ReportDowned(ClientData downClient)
        {
            int numRemoved = _registry.RemoveAll(client => client.Address.Equals(downClient.Address) && client.Port == downClient.Port);

            return numRemoved > 1;
        }

        public IList<ClientScoreData> GetScoreBoard()
        {
            return _registry.AsReadOnly();
        }

        public bool AddToScore(ClientData clientWithPoint)
        {
            bool found = false;
            foreach (ClientScoreData client in _registry)
            {
                if (client.Address.Equals(clientWithPoint.Address) && (client.Port == clientWithPoint.Port))
                {
                    client.Score++;
                    found = true;
                }
            }

            return found;
        }
    }
}