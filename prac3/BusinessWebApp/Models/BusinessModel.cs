using ServerInterfaceLib;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using APIClasses;
using BusinessWebApp.Models;

namespace BusinessTier
{
    class BusinessModel
    {

        private int m_foundIndex;
        //private int m_logNum = 0;

        public static BusinessModel Instance
        {
            get;
        } = new BusinessModel();

        private DataServerInterface _dataServer;

        private BusinessModel()
        {
            //Get the data server
            string url = "net.tcp://localhost:8100/DataService";
            NetTcpBinding tcp = new NetTcpBinding();
            tcp.MaxReceivedMessageSize = 1024 * 1024 * 2;

            //Create connection factory for connection to server
            ChannelFactory<ServerInterfaceLib.DataServerInterface> serverChannelFactory = new ChannelFactory<DataServerInterface>(tcp, url);
            _dataServer = serverChannelFactory.CreateChannel();

            //DEBUG: Test connection actually works
            Console.WriteLine(_dataServer.GetNumEntries());
        }

        /// <summary>
        /// Gets the number of entries in the database
        /// </summary>
        /// <returns></returns>
        public int GetNumEntries()
        {
            int numEntries = _dataServer.GetNumEntries();

            return numEntries;
        }

        /// <summary>
        /// Searches the database for the first profile with the given last name. If found, it is made the found index.
        /// </summary>
        /// <param name="query"></param> True if matching profile was found, false if not
        /// <returns></returns>
        public ProfileData SearchByLastName(string query)
        {
            int foundIndex = -1;

            //Find the first profile matching the query
            for (int ii = 0; ii < _dataServer.GetNumEntries(); ii++)
            {
                string searchLName = null;
                _dataServer.GetValuesForEntry(ii, out _, out _, out _, out _, out searchLName, out _);

                //If query contained in lastname, the match has been found
                if (searchLName.ToLower().Contains(query.ToLower()))
                {
                    foundIndex = ii;
                    break;
                }
            }

            //Throw exception if profile not found
            if (foundIndex == -1)
            {
                throw new NotFoundException("Item with given query not found");
            }

            return GetProfileByIndex(foundIndex);
        }

        public ProfileData GetProfileByIndex(int index)
        {
            ProfileData profileData = new ProfileData();
            int profileImageId;

            try
            {
                _dataServer.GetValuesForEntry(index, out profileData.Acct, out profileData.Pin, out profileData.Bal,
                    out profileData.FName, out profileData.LName, out profileImageId);
            }
            catch (FaultException<DatabaseAccessFault>)
            {
                throw new ArgumentException("Index it out of range for database");
            }

            //Get profile image as stream
            Stream profileStream = null;
            try
            {
                profileStream = _dataServer.GetImageById(profileImageId);
            }
            catch (FaultException<DatabaseAccessFault> d)
            {
                throw new InternalErrorException($"Failed to retrieve image - {d.Detail.Message}");
            }

            //Get profile image stream stream and convert to Base64 string
            byte[] profileBytes;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                profileStream.CopyTo(memoryStream);
                profileBytes = memoryStream.ToArray();

                profileData.ProfileImage = Convert.ToBase64String(profileBytes);
            }

            return profileData;
        }
    }
}