using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        private Block _latestBlock = null;

        public MainWindow()
        {
            InitializeComponent();

            //Generate remoting address
            Random random = new Random();
            uint port = Convert.ToUInt32(random.Next(50000, 60000));

            //Start server
            try
            {
                Server.Instance.Open("localhost", port);
                ClientEndpointLabel.Content = $"https://localhost:{port}";
            }
            catch (ArgumentException a)
            {
                MessageBoxResult result = MessageBox.Show($"Failed to open this client's server. Reason: '{a.Message}'", "Failed to open server", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            _registryServer = new RestClient("https://localhost:44392/");

            //Set up timer for updating UI every 0.1 seconds
            DispatcherTimer dTimer = new DispatcherTimer();
            dTimer.Tick += new EventHandler(UpdateGui);
            dTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dTimer.Start();

            //Create table for storing transactions
            //Prepare scoreboard table
            GridView gridView = new GridView();
            BlockchainListView.View = gridView;

            gridView.Columns.Add(new GridViewColumn
            {
                Header = "ID",
                DisplayMemberBinding = new Binding("Id")
            });
            gridView.Columns.Add(new GridViewColumn
            {
                Header = "From Wallet",
                DisplayMemberBinding = new Binding("WalletFrom")
            });
            gridView.Columns.Add(new GridViewColumn
            {
                Header = "To Wallet",
                DisplayMemberBinding = new Binding("WalletTo")
            });
            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Amount",
                DisplayMemberBinding = new Binding("Amount")
            });
            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Hash",
                DisplayMemberBinding = new Binding("Hash")
            });


        }
        private void UpdateGui(Object o, EventArgs args)
        {
            MinerStatusText.Text = Miner.Instance.Status;
            MinedBlocksText.Text = Miner.Instance.Status;

            ServerStatusText.Text = Server.Instance.Status;

            NumBlocksText.Text = Blockchain.Instance.Chain.Count.ToString();

            //Update blockchain display
            //If a new block has been added to the UI since the last update
            if (_latestBlock != null && !_latestBlock.Hash.SequenceEqual(Blockchain.Instance.LastBlock.Hash))
            {
                BlockchainListView.Items.Clear();

                List<Block> blockchain = Blockchain.Instance.Chain;

                foreach (Block block in blockchain)
                {
                    BlockchainListView.Items.Add(block);
                }
            }
        }

        private void SearchWalletButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uint id = UInt32.Parse(WalletIdBox.Text);

                BalanceText.Text = Blockchain.Instance.GetBalance(id).ToString();
            }
            catch (FormatException)
            {
                MessageBox.Show("Please input a non-negative integer as the user ID.", "Bad input",
                    MessageBoxButton.OK);
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Server.Instance.Close();
        }
    }
}