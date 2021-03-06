using DatabaseLib;
using ServerInterfaceLib;
using System;
using System.Drawing;
using System.IO;
using System.ServiceModel;

namespace ServerProg
{
    /// <summary>
    /// Implementation of data tier server for profiles application. Supplies callers with profiles based on index.
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    internal class DataServer : DataServerInterface
    {
        private static readonly string LOGS_PATH = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent
            .Parent.Parent.FullName, "business-server-logs.log");

        private DatabaseClass database;
        public DataServer()
        {
            Console.WriteLine("Constructing new server...");
            database = new DatabaseClass(1000);
        }

        public int GetNumEntries()
        {
            return database.GetNumRecords();
        }

        /// <summary>
        /// Returns all values (excluding image) for the given profile index.
        /// </summary>
        /// <exception cref="FaultException">If index is not valid for database</exception>
        public void GetValuesForEntry(int index, out uint acctNo, out uint pin, out int bal, out string fName, out string lName, out int profileImageId)
        {
            try
            {
                acctNo = database.GetAcctNoByIndex(index);
                pin = database.GetPinByIndex(index);
                bal = database.GetBalanceByIndex(index);
                fName = database.GetFirstNameByIndex(index);
                lName = database.GetLastNameByIndex(index);
                profileImageId = database.GetImageIdByIndex(index);
            }
            catch (ArgumentOutOfRangeException a)
            {
                Log($"Attempted profile access by index '{index}', out of range");
                throw new FaultException<DatabaseAccessFault>(new DatabaseAccessFault("Profile access failed: " + a.Message));
            }
        }

        /// <summary>
        /// Returns the profile image with a given ID.
        /// </summary>
        /// <param name="id">ID of profile image</param>
        /// <returns>Stream representing user's profile</returns>
        /// <exception cref="FaultException">If ID is not valid for database</exception>
        public Stream GetImageById(int id)
        {
            //Get image from database
            Bitmap image;
            try
            {
                image = database.GetImageById(id);
            }
            catch (ArgumentOutOfRangeException a)
            {
                Log($"Attempted image access by id '{id}', out of range of possible IDs");
                throw new FaultException<DatabaseAccessFault>(new DatabaseAccessFault("Image access failed: " + a.Message));
            }

            //Create memory stream for givne bitmap image
            MemoryStream ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// Initiates server.
        /// </summary>
        static void Main(string[] args)
        {
            //This should *definitely* be more descriptive.
            Console.WriteLine("hey so like welcome to my server");

            //This is the actual host service system
            ServiceHost host;
            //This represents a tcp/ip binding in the Windows network stack
            NetTcpBinding tcp = new NetTcpBinding();
            //tcp.TransferMode = TransferMode.StreamedResponse;
            tcp.MaxReceivedMessageSize = 1024 * 1024 * 2; //NOTE: Why 2?

            //Bind server to the implementation of DataServer
            host = new ServiceHost(typeof(DataServer));
            //Present the publicly accessible interface to the client. 0.0.0.0 tells .net to accept on any interface. :8100 means this will use port 8100. DataService is a name for the actual service, this can be any string.
            host.AddServiceEndpoint(typeof(DataServerInterface), tcp,
            "net.tcp://0.0.0.0:8100/DataService");
            //And open the host for business!
            host.Open();
            Console.WriteLine("System Online");
            Console.ReadLine();
            //Don't forget to close the host after you're done!
            host.Close();
        }

        private void Log(string message)
        {
            //Add log number and increment log number
            StreamWriter logsFileWriter = File.AppendText(LOGS_PATH);
            logsFileWriter.WriteLine(message);
            logsFileWriter.Close();
        }
    }
}
