using APIClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

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

        private static readonly string LOGS_PATH = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName, "registry-logs.log");

        private RegistryModel()
        {
            _registry = new List<ClientScoreData>();
        }

        public void Register(ClientData newData)
        {
            //Input validation
            bool validAddress = Uri.TryCreate($"net.tcp://{newData}", UriKind.Absolute, out Uri _);
            if (!validAddress)
            {
                Log($"Attempted to register client '{newData}' with invalid address");
                throw new ArgumentException("Invalid client address given");
            }

            if (newData.Port > 65353)
            {
                Log($"Attempted to register client '{newData}' with invalid port");
                throw new ArgumentException("Invalid client port given");
            }

            bool duplicate = _registry.Any(data => data.Address.Equals(newData.Address) && (data.Port == newData.Port));

            if (duplicate)
            {
                Log($"Attempted to register duplicate client '{newData}'");
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

            Log($"Client {downClient} reported as downed, removing from registry");

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

            if (!found)
            {
                Log($"Attempted to add to score of non-existent client '{clientWithPoint}'");
            }

            return found;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]

        private void Log(string message)
        {
            //Add log number and increment log number
            StreamWriter logsFileWriter = File.AppendText(LOGS_PATH);
            logsFileWriter.WriteLine(message);
            logsFileWriter.Close();
        }
    }
}