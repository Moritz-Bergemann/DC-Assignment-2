using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ClientApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //Generate remoting address
            Random random = new Random();
            uint port = Convert.ToUInt32(random.Next(50000, 60000)); //TODO potential double-up

            //Start server
            try
            {
                Server.Instance.Open("localhost", port);
            }
            catch (ArgumentException a)
            {
                MessageBoxResult result = MessageBox.Show($"Failed to open this client's server. Reason: '{a.Message}'", "Failed to open server", MessageBoxButton.OK, MessageBoxImage.Error);
            }

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
    }
}