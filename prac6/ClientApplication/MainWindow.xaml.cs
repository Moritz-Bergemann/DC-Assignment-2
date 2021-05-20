using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using APIClasses;
using Newtonsoft.Json;

namespace ClientApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RestClient _registryServer;

        public MainWindow()
        {
            InitializeComponent();

            //Generate remoting address
            Random random = new Random();
            uint port = Convert.ToUInt32(random.Next(50000, 60000)); //TODO potential double-up and picking of bad ports

            //Start server
            try
            {
                Server.Instance.Open("localhost", port);
            }
            catch (ArgumentException a)
            {
                MessageBoxResult result = MessageBox.Show($"Failed to open this client's server. Reason: '{a.Message}'", "Failed to open server", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            _registryServer = new RestClient("https://localhost:44392/");

            //Prepare scoreboard table
            GridView gridView = new GridView();
            ScoreboardListView.View = gridView;

            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Endpoint",
                DisplayMemberBinding = new Binding("Endpoint")
            });
            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Score",
                DisplayMemberBinding = new Binding("Score")
            });

            //Start client looking for jobs in background
            Task.Run(Worker.Instance.Run);

            //Set up timer for updating UI every 0.5 seconds
            DispatcherTimer dTimer = new DispatcherTimer();
            dTimer.Tick += new EventHandler(UpdateGui);
            dTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dTimer.Start();
        }

        private void JobPostButton_Click(object sender, RoutedEventArgs e)
        {
            //Add new job based on job box content
            Server.Instance.AddNewJob(JobPostBox.Text); //Todo python validation?

            JobPostBox.Text = string.Empty;

            MessageBox.Show("Job Posted!", "Posted", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateGui(object sender, EventArgs e)
        {
            //Server Stuff
            ServerStatus.Text = Server.Instance.Status;
            PostedJobs.Text = Server.Instance.NumJobs.ToString();
            PostedJobsCompleted.Text = Server.Instance.CompletedJobs.Count.ToString();

            //Client Stuff
            WorkerStatus.Text = Worker.Instance.Status;
            CompletedJobs.Text = Worker.Instance.NumJobsDone.ToString();
        }

        private void SeeResultsButton_Click(object sender, RoutedEventArgs e)
        {
            Window resultsWindow = new Window
            {
                Title = "Job Results",
                Content = new JobResultsUserControl(Server.Instance.CompletedJobs),
                SizeToContent = SizeToContent.WidthAndHeight
            };

            resultsWindow.Show();
        }

        private async void UpdateScoreboardButton_Click(object sender, RoutedEventArgs e)
        {
            //Run request for scoreboard in different thread
            RestRequest request = new RestRequest("api/scoreboard");
            IRestResponse response = await Task.Run(() => _registryServer.Get(request));

            if (!response.IsSuccessful)
            {
                MessageBox.Show($"Could not retrieve scoreboard data - {response.Content}");
            }

            List<ClientScoreData> scores = JsonConvert.DeserializeObject<List<ClientScoreData>>(response.Content);

            //Clear current scoreboard
            ScoreboardListView.Items.Clear();

            //Display the table
            foreach (ClientScoreData scoreData in scores)
            {
                ScoreboardListView.Items.Add(new
                {
                    Endpoint = $"https://{scoreData.Address}:{scoreData.Port}",
                    Score = scoreData.Score
                });
            }
        }
    }
}