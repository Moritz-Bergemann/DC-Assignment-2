﻿using ServerInterfaceLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BusinessTier
{
    class BusinessServer : BusinessServerInterface
    {
        //private static string LOGS_PATH = @"C:\Users\morit\Source\Repos\DC-Workshops\BusinessTier\business-server.log";
        private static string LOGS_PATH = "../../../business-server.log";

        private DataServerInterface m_dataServer;

        private int m_foundIndex;
        private int m_logNum = 0;
        public BusinessServer()
        {
            //Get the data server
            string url = "net.tcp://localhost:8100/DataService";
            NetTcpBinding tcp = new NetTcpBinding();
            //tcp.TransferMode = TransferMode.StreamedResponse; //FIXME for some reason this causes the constructor to be reloaded each time AND also causes fault contracts to not work jesus christ 🤔
            tcp.MaxReceivedMessageSize = 1024 * 1024 * 2; //NOTE: Why 2?

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
            } else
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
            message = m_logNum.ToString() + ": " + message;
            m_logNum++;

            StreamWriter logsFileWriter = File.AppendText(LOGS_PATH);
            logsFileWriter.WriteLine(message);
            logsFileWriter.Close();
        }
    }
}