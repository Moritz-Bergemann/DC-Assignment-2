using APIClasses;
using Newtonsoft.Json;
using RestSharp;
using ServerInterfaceLib;
using System;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ServerView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RestClient _businessServer;

        public MainWindow()
        {
            InitializeComponent();

            //Set up web service connection
            string uri = "https://localhost:44376/"; //Base URI for connecting to server
            _businessServer = new RestClient(uri);

            //Also, tell me how many entries are in the DB.
            RestRequest numEntriesRequest = new RestRequest("api/values");
            IRestResponse numEntriesResponse = _businessServer.Get(numEntriesRequest);
            TotalEntriesBox.Text = numEntriesResponse.Content;
        }

        private async void NameSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string query;
            try
            {
                //Get the user query
                query = QueryEditBox.Text;

                //Build search data struct for query
                SearchData searchData = new SearchData();
                searchData.searchStr = query;

                //Make request on web service for search
                RestRequest searchRequest = new RestRequest("api/search");
                searchRequest.AddJsonBody(searchData);

                IRestResponse searchResponse = _businessServer.Post(searchRequest);

                bool found = true;

                if (!searchResponse.IsSuccessful)
                {
                    if (searchResponse.StatusCode == HttpStatusCode.NotFound) //If profile was not found
                    {
                        found = false;
                    }
                    else
                    {
                        throw new DatabaseAccessException($"Database access error - searchResponse.Content");
                    }
                }

                if (found)
                {
                    ProfileData profileData = JsonConvert.DeserializeObject<ProfileData>(searchResponse.Content);

                    FillGui(profileData);
                }
                else
                {
                    MessageBox.Show($"Could not find given profile with last name query \'{query}\'", "Not found", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unknown error has occurred - {ex.Message}.", "Unknown Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async void IndexSearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Get the index query
                int index = int.Parse(IndexEditBox.Text);

                //Make request on web service for search
                RestRequest searchRequest = new RestRequest($"api/GetValues/{index}");
                IRestResponse searchResponse = _businessServer.Get(searchRequest);

                if (!searchResponse.IsSuccessful)
                {
                    if (searchResponse.StatusCode == HttpStatusCode.BadGateway)
                    {
                        MessageBox.Show($"Bad request - {searchResponse.Content}", "Bad Request", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show($"Unexpected error - {searchResponse.Content}", "Unexpected Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return;
                }

                //Fill GUI given search was successful
                ProfileData profileData = JsonConvert.DeserializeObject<ProfileData>(searchResponse.Content);
                FillGui(profileData);
            }
            catch (FormatException)
            {
                MessageBox.Show("Please insert only numbers into the index box", "Bad Input", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillGui(ProfileData profileData)
        {
            //Set values in GUI
            FirstNameBox.Text = profileData.FName;
            LastNameBox.Text = profileData.LName;
            BalanceBox.Text = profileData.Bal.ToString();
            AccountNumBox.Text = profileData.Acct.ToString();
            PinBox.Text = profileData.Pin.ToString();

            //Decode image Base64 data and display it
            byte[] profileBinary = Convert.FromBase64String(profileData.ProfileImage);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = new MemoryStream(profileBinary);
            image.EndInit();

            ImageBox.Source = image;
        }
    }
}
