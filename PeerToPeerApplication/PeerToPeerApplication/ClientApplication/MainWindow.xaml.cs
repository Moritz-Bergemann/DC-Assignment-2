using System;
using System.Windows;

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

            ServerStatus.Text = "Starting...";

            //Generate remoting address
            Random random = new Random();
            uint port = Convert.ToUInt32(random.Next(49152, 65534)); //TODO potential double-up

            //Start server
            try
            {
                Server.Instance.Open("0.0.0.0", port);
                ServerStatus.Text = "Running";
            }
            catch (ArgumentException a)
            {
                ServerStatus.Text = $"Error encountered - {a.Message}";
            }

            //Start client looking for jobs in background
            Network.Instance.Run();
        }

        private void JobPostButton_Click(object sender, RoutedEventArgs e)
        {
            //Add new job based on job box content
            Server.Instance.AddNewJob(JobPostBox.Text); //Todo python validation?
        }
    }
}