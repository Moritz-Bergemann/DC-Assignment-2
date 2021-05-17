using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using DatabaseLib;
using ServerInterfaceLib;

namespace ServerProg
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    internal class DataServer : ServerInterfaceLib.DataServerInterface
    {
        private DatabaseLib.DatabaseClass database;

        public DataServer()
        {
            database = new DatabaseClass(100000);
        }
        public int GetNumEntries()
        {
            return database.GetNumRecords();
        }

        public void GetValuesForEntry(int index, out uint acctNo, out uint pin, out int bal, out string fName, out string lName)
        {
            try
            {
                acctNo = database.GetAcctNoByIndex(index);
                pin = database.GetPINByIndex(index);
                bal = database.GetBalanceByIndex(index);
                fName = database.GetFirstNameByIndex(index);
                lName = database.GetLastNameByIndex(index);
            }
            catch (ArgumentOutOfRangeException a)
            {
                throw new FaultException<DatabaseAccessFault>(new DatabaseAccessFault("Profile access failed: " + a.Message));
            }
        }
        public Stream GetImageForEntry(int index)
        {
            Bitmap image;
            try
            {
                string path = database.GetImagePathByIndex(index);
                image = new Bitmap(path, false);
            } catch (ArgumentOutOfRangeException a)
            {
                throw new FaultException<DatabaseAccessFault>(new DatabaseAccessFault("Profile access failed: " + a.Message));
            }
            MemoryStream ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            ms.Position = 0; //Figure out why this is so important - https://stackoverflow.com/questions/10584260/
            return ms;
        }
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
    }
}
