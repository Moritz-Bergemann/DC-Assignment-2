using System.Windows;
using System.Windows.Controls;
using System.ServiceModel;
using ServerProg;
using System;
using ServerInterfaceLib;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;

namespace ServerView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BusinessServerInterface m_server;

        public MainWindow()
        {
            InitializeComponent();

            Console.WriteLine("Making new TCP request...");

            //Set server URL 
            string url = "net.tcp://localhost:8101/BusinessService";
            NetTcpBinding tcp = new NetTcpBinding();
            //tcp.TransferMode = TransferMode.StreamedResponse; //FIXME for some reason this causes the constructor to be reloaded each time AND also causes fault contracts to not work jesus christ 🤔
            tcp.MaxReceivedMessageSize = 1024 * 1024 * 2; //NOTE: Why 2?

            //Create connection factory for connection to server
            ChannelFactory<BusinessServerInterface> serverChannelFactory = new ChannelFactory<BusinessServerInterface>(tcp, url);
            m_server = serverChannelFactory.CreateChannel();

            //Also, tell me how many entries are in the DB.
            TotalEntriesBox.Text = m_server.GetNumEntries().ToString();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string query;
            try
            {
                //Get the user query
                query = QueryEditBox.Text;

                //Then, run our RPC function, using the out mode parameters
                bool found = m_server.SearchByLastName(query);

                string fName = "", lName = "";
                int bal = 0;
                uint acct = 0, pin = 0;
                Stream profileStream;

                if (found)
                {
                    m_server.GetSearchedProfileDetails(out acct, out pin, out bal, out fName, out lName);

                    //And now, set the values in the GUI!
                    FirstNameBox.Text = fName;
                    LastNameBox.Text = lName;
                    BalanceBox.Text = bal.ToString("C");
                    AccountNumBox.Text = acct.ToString();
                    PinBox.Text = pin.ToString("D4");

                    profileStream = m_server.GetSearchedProfileImage();

                    //Display the profile image, getting it from the image stream
                    ImageBox.Source = BitmapFrame.Create(profileStream);
                }
                else
                {
                    MessageBox.Show("No profiles containing the specified query were found.", "No results", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Please insert only numbers into the index box", "Bad Input", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (FaultException<DatabaseAccessFault> d)
            {
                MessageBox.Show(d.Detail.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            } 
            catch (FaultException)
            {
                MessageBox.Show("An unknown error has occured.", "Unknown Error", MessageBoxButton.OK, MessageBoxImage.Error);
            } 
            catch (Exception ex)
            {
                MessageBox.Show($"Unknown Error - {ex.Message}", "Unknown Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<Tuple<uint, uint, int, string, string> GetUser()
        {

        }

        private async Task<Stream> GetImageAsync(int index)
    }
}
