using ServerInterfaceLib;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.ServiceModel;

namespace BusinessTier
{
    class BusinessServer : BusinessServerInterface
    {
        //private static string LOGS_PATH = @"C:\Users\morit\Source\Repos\DC-Workshops\BusinessTier\business-server.log";
        private static readonly string LOGS_PATH = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent
            .Parent.Parent.FullName, "business-server-logs.log");

        private DataServerInterface m_dataServer;

        private int m_logNum = 0;
        public BusinessServer()
        {
            //Get the data server
            string url = "net.tcp://localhost:8100/DataService";
            NetTcpBinding tcp = new NetTcpBinding();
            tcp.MaxReceivedMessageSize = 1024 * 1024 * 2;

            //Create connection factory for connection to server
            ChannelFactory<ServerInterfaceLib.DataServerInterface> serverChannelFactory = new ChannelFactory<DataServerInterface>(tcp, url);
            m_dataServer = serverChannelFactory.CreateChannel();

            //DEBUG: Test connection actually works
            Console.WriteLine(m_dataServer.GetNumEntries());
        }

        /// <summary>
        /// Gets the number of entries in the database
        /// </summary>
        /// <returns></returns>
        public int GetNumEntries()
        {
            int numEntries = m_dataServer.GetNumEntries();
            Log($"Retrieve Number of Entries - Result {numEntries}");

            return numEntries;
        }

        /// <summary>
        /// Searches the database for the first profile with the given last name. If found, it is made the found index.
        /// </summary>
        /// <param name="query"></param> True if matching profile was found, false if not
        /// <returns></returns>
        public void SearchByLastName(string query, out uint acctNo, out uint pin, out int bal, out string fName, out string lName, out int profileImageId)
        {
            int foundIndex = -1;

            //Assign default values to search results (in case value not found)
            acctNo = pin = 0;
            bal = profileImageId = -1;
            fName = lName = null;


            //Find the first profile matching the query
            for (int ii = 0; ii < m_dataServer.GetNumEntries(); ii++)
            {
                lName = null;
                m_dataServer.GetValuesForEntry(ii, out acctNo, out pin, out bal, out fName, out lName, out profileImageId);

                //If query contained in lastname, the match has been found
                if (lName.ToLower().Contains(query.ToLower()))
                {
                    foundIndex = ii;
                    break;
                }
            }

            //Do Logging
            string logMessage = $"Profile Search - query \'{query}\' - ";
            logMessage += (foundIndex == -1 ? "No Results" : $"Match - index {foundIndex}: \'#{acctNo}\', \'PIN {pin}\', \'${bal}\', \'{fName} {lName}\', \'IMG{profileImageId}\'");
            Log(logMessage);

            if (foundIndex == -1)
            {
                throw new FaultException<DatabaseAccessFault>(
                    new DatabaseAccessFault($"Profile matching last name query \'{query}\' could not be found"));
            }
        }

        /// <summary>
        /// Finds a given profile image by ID integer, retrieved by searching for a given profile.
        /// </summary>
        /// <param name="id">ID integer of image to return</param>
        /// <returns></returns>
        public Stream GetProfileImageById(int id)
        {
            string logString = $"Image search By ID \'{id}\' - ";

            Stream stream = null;
            try
            {
                stream = m_dataServer.GetImageById(id);
                logString += "found";
            }
            catch (FaultException<DatabaseAccessFault> d)
            {
                logString += $"not found (\'{d.Message}\')";
            }
            
            Log(logString);

            //Throw exception if profile image was not found
            if (stream == null)
            {
                throw new FaultException<DatabaseAccessFault>(new DatabaseAccessFault($"Image with ID \'{id}\' could not be found"));
            }

            return stream;
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Starting business server...");

            //Create host service
            ServiceHost host;
            NetTcpBinding tcp = new NetTcpBinding();
            tcp.MaxReceivedMessageSize = 1024 * 1024 * 2; //NOTE: Why 2?
            string url = "net.tcp://0.0.0.0:8101/BusinessService";

            //Bind service to server
            host = new ServiceHost(typeof(BusinessServer));

            //Define publicly available interface for server
            host.AddServiceEndpoint(typeof(BusinessServerInterface), tcp, url);
            host.Open();

            Console.WriteLine("Business server online.");

            Console.ReadLine();
            //Don't forget to close the host after you're done!
            host.Close();
        }

        /// <summary>
        /// Logs a message
        /// </summary>
        /// <param name="message"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void Log(string message)
        {
            //Add log number and increment log number
            message = m_logNum + ": " + message;
            m_logNum++;

            StreamWriter logsFileWriter = File.AppendText(LOGS_PATH);
            logsFileWriter.WriteLine(message);
            logsFileWriter.Close();
        }
    }
}