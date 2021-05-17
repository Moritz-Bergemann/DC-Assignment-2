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

            PrepareServer();

            //Also, tell me how many entries are in the DB.
            TotalEntriesBox.Text = m_server.GetNumEntries().ToString();
        }

        private void PrepareServer()
        {
            //Set server URL 
            string url = "net.tcp://localhost:8101/BusinessService";
            NetTcpBinding tcp = new NetTcpBinding();
            tcp.MaxReceivedMessageSize = 1024 * 1024 * 2; //NOTE: Why 2?

            //Create connection factory for connection to server
            ChannelFactory<BusinessServerInterface> serverChannelFactory =
                new ChannelFactory<BusinessServerInterface>(tcp, url);
            m_server = serverChannelFactory.CreateChannel();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Get the user query
                var query = QueryEditBox.Text;

                //Then, run our RPC function, using the out mode parameters
                Task<bool> searchTask = SearchByLastNameAsync(query);

                //Show loading bar
                SearchingProgressBar.Visibility = Visibility.Visible;
                SearchingLabel.Visibility = Visibility.Visible;

                bool found = await searchTask;

                if (found)
                {
                    Task<Tuple<uint, uint, int, string, string>> getProfileTask = GetProfileDetailsAsync();
                    Task<Stream> getProfileImageTask = GetProfileImageAsync();

                    //await search complete
                    Tuple<uint, uint, int, string, string> profileResult = await getProfileTask;
                    Stream profileStream = await getProfileImageTask;

                    //Set values in GUI
                    FirstNameBox.Text = profileResult.Item4;
                    LastNameBox.Text = profileResult.Item5;
                    BalanceBox.Text = profileResult.Item3.ToString();
                    AccountNumBox.Text = profileResult.Item1.ToString();
                    PinBox.Text = profileResult.Item2.ToString();

                    //Display the profile image, getting it from the image stream
                    ImageBox.Source = BitmapFrame.Create(profileStream);
                }
                else
                {
                    MessageBox.Show("No profiles containing the specified query were found.", "No results",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Please insert only numbers into the index box", "Bad Input", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (FaultException<DatabaseAccessFault> d)
            {
                MessageBox.Show(d.Detail.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (FaultException)
            {
                MessageBox.Show("An unknown error has occured.", "Unknown Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (TimeoutException t)
            {
                MessageBox.Show("The requested operation has timed out. This may be due to an internal server error or as the request is taking too long to process.", "Timeout Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);

                //Rebuild server
                PrepareServer();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unknown Error - {ex.Message}", "Unknown Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);

                //Rebuild server (just in case)
                PrepareServer();
            }
            finally
            {
                //Hide loading bar
                SearchingProgressBar.Visibility = Visibility.Hidden;
                SearchingLabel.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>Tuple of elements (in order) account number, PIN, balance, first name, last name</returns>
        private async Task<Tuple<uint, uint, int, string, string>> GetProfileDetailsAsync()
        {
            return await Task.Run(() =>
            {
                m_server.GetSearchedProfileDetails(out uint acct, out uint pin, out int bal, out string fName,
                        out string lName);

                return new Tuple<uint, uint, int, string, string>(acct, pin, bal, fName, lName);
            });
        }

        private async Task<Stream> GetProfileImageAsync()
        {
            return await Task.Run(() => m_server.GetSearchedProfileImage());
        }

        private async Task<bool> SearchByLastNameAsync(string query)
        {
            return await Task.Run(() => m_server.SearchByLastName(query));
        }
    }
}
