using System.Windows;
using System.ServiceModel;
using System;
using ServerInterfaceLib;
using System.Windows.Media.Imaging;

namespace ServerView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataServerInterface m_server;

        public MainWindow()
        {
            InitializeComponent();

            Console.WriteLine("Making new TCP request...");

            //Set server URL 
            string url = "net.tcp://localhost:8100/DataService";
            NetTcpBinding tcp = new NetTcpBinding();
            tcp.MaxReceivedMessageSize = 1024 * 1024 * 2;

            //Create connection factory for connection to server
            ChannelFactory<ServerInterfaceLib.DataServerInterface> serverChannelFactory = new ChannelFactory<DataServerInterface>(tcp, url);
            m_server = serverChannelFactory.CreateChannel();

            //Also, tell me how many entries are in the DB.
            TotalEntriesBox.Text = m_server.GetNumEntries().ToString();
        }

        /// <summary>
        /// Called when selecting "get user" button. Retrieves user data from database server.
        /// </summary>
        private void GetUserButton_Click(object sender, RoutedEventArgs e)
        {
            int index = 0;
            string fName = "", lName = "";
            int bal = 0;
            uint acct = 0, pin = 0;

            try
            {
                //On click, Get the index....
                index = Int32.Parse(IndexEditBox.Text);

                //Then, run our RPC function, using the out mode parameters...
                m_server.GetValuesForEntry(index, out acct, out pin, out bal, out fName, out lName);

                //And now, set the values in the GUI!
                FirstNameBox.Text = fName;
                LastNameBox.Text = lName;
                BalanceBox.Text = bal.ToString("C");
                AccountNumBox.Text = acct.ToString();
                PinBox.Text = pin.ToString("D4");

                //Display the profile image, getting it from the image stream
                ImageBox.Source = BitmapFrame.Create(m_server.GetImageForEntry(index));
            } catch (FormatException)
            {
                MessageBox.Show("Please insert only numbers into the index box", "Bad Input", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (FaultException<DatabaseAccessFault> d)
            {
                MessageBox.Show(d.Detail.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            } catch (FaultException f)
            {
                MessageBox.Show("An unknown server error has occured.", "Unknown Server Error", MessageBoxButton.OK, MessageBoxImage.Error);
            } catch (Exception ex)
            {
                MessageBox.Show($"Unknown Error - {ex.Message}.", "Unknown Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
