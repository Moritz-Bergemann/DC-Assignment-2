using ServerInterfaceLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Web;

namespace BusinessWebApp.Models
{
    /// <summary>
    /// Connects to the data tier to supply business tier services
    /// </summary>
    public class DataModel
    {
        private DataServerInterface m_dataServer;

        private int m_foundIndex;
        //private int m_logNum = 0;

        public static DataModel Instance
        {
            get;
        } = new DataModel();

        protected DataModel()
        {
            //Get the data server
            string url = "net.tcp://localhost:8100/DataService";
            NetTcpBinding tcp = new NetTcpBinding();
            tcp.MaxReceivedMessageSize = 1024 * 1024 * 2;

            //Create connection factory for connection to server
            ChannelFactory<DataServerInterface> serverChannelFactory = new ChannelFactory<DataServerInterface>(tcp, url);
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
        public bool SearchByLastName(string query)
        {
            m_foundIndex = -1;

            //Find the first profile matching the query
            for (int ii = 0; ii < m_dataServer.GetNumEntries(); ii++)
            {
                string curLName = null;
                m_dataServer.GetValuesForEntry(ii, out _, out _, out _, out _, out curLName);

                //If query contained in lastname, the match has been found
                if (curLName.ToLower().Contains(query.ToLower()))
                {
                    m_foundIndex = ii;
                    break;
                }
            }

            //Do Logging
            string logMessage = $"Profile Search - query \'{query}\' - ";
            logMessage += (m_foundIndex == -1 ? "No Results" : $"First Match Index {m_foundIndex}");
            Log(logMessage);

            return (m_foundIndex != -1);
        }

        public void GetValuesForEntry(int index, out uint acctNo, out uint pin, out int bal, out string fName, out string lName)
        {
            m_dataServer.GetValuesForEntry(index, out acctNo, out pin, out bal, out fName, out lName);
        }

        /// <summary>
        /// Get the profile details of the current found profile.
        /// </summary>
        public void GetSearchedProfileDetails(out uint acctNo, out uint pin, out int bal, out string fName, out string lName)
        {
            string logMessage = "Retrieve Searched Profile Details - ";

            if (m_foundIndex != -1)
            {
                m_dataServer.GetValuesForEntry(m_foundIndex, out acctNo, out pin, out bal, out fName, out lName);
                Log(logMessage + $"Matched Index {m_foundIndex}: \'{acctNo}\', \'{pin}\', \'{bal}\', \'{fName} {lName}\'");
            }
            else
            {
                Log(logMessage + "No latest profile");
                throw new FaultException<DatabaseAccessFault>(new DatabaseAccessFault("Last searched profile was not found"));
            }
        }

        /// <summary>
        /// Get the profile image for the current found profile
        /// </summary>
        /// <returns></returns> stream containing the image of the current found profile.
        public Stream GetSearchedProfileImage()
        {
            string logMessage = "Retrieve Searched Profile Details - ";

            if (m_foundIndex != -1)
            {
                Log(logMessage + $"Matched Index {m_foundIndex}");
                return m_dataServer.GetImageForEntry(m_foundIndex);
            }
            else
            {
                Log(logMessage + "No latest profile");
                throw new FaultException<DatabaseAccessFault>(new DatabaseAccessFault("Last searched profile was not found"));
            }
        }

        /// <summary>
        /// Logs a message
        /// </summary>
        /// <param name="message"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void Log(string message)
        {
            // Do nothing because I'm not sure how file IO will work with this

            ////Add log number and increment log number
            //message = m_logNum.ToString() + ": " + message;
            //m_logNum++;

            //StreamWriter logsFileWriter = File.AppendText(LOGS_PATH);
            //logsFileWriter.WriteLine(message);
            //logsFileWriter.Close();
        }
    }
}