using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            int port = random.Next(49152, 65534); //TODO potential double-up
            string url = $"net.tcp://0.0.0.0:{port}/PeerServer";

            //Start server
            Server.Instance.Open(url);

            //Start client looking for jobs in background
            Task.Run(Network.Instance.Run);
        }
    }
}
