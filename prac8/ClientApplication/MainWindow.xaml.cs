using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
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

        private uint _clientWallet = 0;

        public MainWindow()
        {
            InitializeComponent();

            //Start server
            bool serverSuccess = false;

            while (!serverSuccess)
            {
                //Generate remoting address
                Random random = new Random();
                uint port = Convert.ToUInt32(random.Next(50000, 60000));

                try
                {
                    serverSuccess = true;
                    Server.Instance.Open("localhost", port);
                    ClientEndpointLabel.Content = $"https://localhost:{port}";

                    //Setup logger
                    Logger.Instance.SetClient(new ClientData("localhost", port));
                }
                catch (ArgumentException a)
                {
                    MessageBoxResult result =
                        MessageBox.Show($"Failed to open this client's server. Reason: '{a.Message}'",
                            "Failed to open server", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                }
                catch (ServerLaunchException s) //If server failed to open (likely due to port mismatch)
                {
                    Logger.Instance.Log($"Attempted opening client on https://localhost:{port}, but got error - {s}");

                    //Try again with a different port
                    serverSuccess = false;
                }
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
                Header = "Transactions",
                DisplayMemberBinding = new Binding("Transactions")
            });
            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Hash",
                DisplayMemberBinding = new Binding("Hash")
            });

            //Show login prompt
            Window loginWindow = new Window
            {
                Title = "Job Results",
                Content = new EnterWalletIdUserControl(SetWalletForClient), //Give control callback for setting wallet ID here
                SizeToContent = SizeToContent.WidthAndHeight
            };

            bool? accepted = loginWindow.ShowDialog();

            if (accepted == null || (bool) !accepted)
            {
                //Close the whole app
                Miner.Instance.Close();
                Server.Instance.Close();

                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Sets the wallet ID for this client.
        /// </summary>
        /// <param name="walletId">Wallet ID for this client</param>
        private void SetWalletForClient(uint walletId)
        {
            _clientWallet = walletId;

            //Show in GUI
            WalletLabel.Content = walletId.ToString();
        }

        private void UpdateGui(Object o, EventArgs args)
        {
            MinerStatusText.Text = Miner.Instance.Status;
            MinedBlocksText.Text = Miner.Instance.MinedBlocks.ToString();
            PendingTransactionsText.Text = Miner.Instance.PendingTransactions.ToString();

            ServerStatusText.Text = Server.Instance.Status;

            NumBlocksText.Text = Blockchain.Instance.Chain.Count.ToString();

            //Update blockchain display
            //If a new block has been added to the UI since the last update
            if (_latestBlock == null || !_latestBlock.Hash.SequenceEqual(Blockchain.Instance.LastBlock.Hash))
            {
                BlockchainListView.Items.Clear();

                List<Block> blockchain = Blockchain.Instance.Chain;

                foreach (Block block in blockchain)
                {
                    string transactionString = "";
                    foreach (Transaction transaction in block.Transactions)
                    {
                        transactionString += "|" + transaction.ToString();
                    }

                    BlockchainListView.Items.Add(new
                    {
                        Id = block.Id.ToString(),
                        Transactions = transactionString,
                        Hash = Convert.ToBase64String(block.Hash)
                    });
                }

                _latestBlock = Blockchain.Instance.LastBlock;
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
            catch (OverflowException)
            {
                MessageBox.Show("Please input a non-negative integer as the user ID.", "Bad input",
                    MessageBoxButton.OK);
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                Server.Instance.Close();
                Miner.Instance.Close();
            } catch (ArgumentException) { } //Do nothing, only catch to exit gracefully in case either already closed
            
        }

        private async void SubmitTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            //Create transaction from inputs
            Transaction transaction;
            try
            {
                transaction = new Transaction()
                {
                    WalletFrom = _clientWallet,
                    WalletTo = uint.Parse(WalletToBox.Text),
                    Amount = float.Parse(AmountBox.Text)

                };
            }
            catch (FormatException)
            {
                MessageBox.Show("Could not parse input values for transaction.", "Parse error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Broadcast transaction
            try
            {
                //Run in different thread (as this may take a very long time)
                await Task.Run(() => BroadcastTransaction(transaction));
            }
            catch (ArgumentException a)
            {
                MessageBox.Show($"Failed to broadcast transaction to other clients. Reason - {a.Message}", "Transaction broadcast failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BroadcastTransaction(Transaction transaction)
        {
            RestRequest request = new RestRequest("api/get-registered");
            IRestResponse response = _registryServer.Get(request);

            List<ClientData> clients = JsonConvert.DeserializeObject<List<ClientData>>(response.Content);

            if (clients.Count == 0)
            {
                throw new ArgumentException("No available clients in registry");
            }

            //Post transaction to each client
            foreach (ClientData clientData in clients)
            {
                string url = $"net.tcp://{clientData.Address}:{clientData.Port}/BlockchainServer";
                NetTcpBinding tcp = new NetTcpBinding();
                ChannelFactory<IServer> serverChannelFactory = new ChannelFactory<IServer>(tcp, url);
                IServer clientBlockchainServer =  serverChannelFactory.CreateChannel();

                clientBlockchainServer.PutTransaction(transaction);
            }
        }
    }
}