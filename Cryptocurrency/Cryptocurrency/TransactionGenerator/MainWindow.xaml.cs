using APIClasses;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Block = APIClasses.Block;

namespace TransactionGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RestClient _blockchainServer;
        private RestClient _minerServer;

        public MainWindow()
        {
            _blockchainServer = new RestClient("https://localhost:44367/");
            _minerServer = new RestClient("https://localhost:44373/");

            InitializeComponent();

            DispatcherTimer dTimer = new DispatcherTimer();
            dTimer.Tick += new EventHandler(UpdateGui);
            dTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dTimer.Start();
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            //Get the entire blockchain first
            RestRequest blockchainRequest = new RestRequest("api/blockchain");
            IRestResponse blockchainResponse = _blockchainServer.Get(blockchainRequest);

            if (!blockchainResponse.IsSuccessful)
            {
                MessageBox.Show($"An error occurred while attempting to access the blockchain - {blockchainResponse.Content}",
                    "Failed to get blockchain", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            IList<Block> blockchain = JsonConvert.DeserializeObject<IList<Block>>(blockchainResponse.Content);

            //Update blockchain length
            NumBlocksText.Text = blockchain.Count.ToString();

            //Update wallet balance
            try
            {
                uint walletId = uint.Parse(AccountIdBox.Text);

                RestRequest walletRequest = new RestRequest($"api/wallet/{walletId}");
                IRestResponse walletResponse = _blockchainServer.Get(walletRequest);

                if (!walletResponse.IsSuccessful)
                {
                    BalanceText.Text = $"Failed to get wallet - {walletResponse.Content}";
                }

                Wallet wallet =  JsonConvert.DeserializeObject<Wallet>(walletResponse.Content);
                BalanceText.Text = $"{wallet.Balance} coins";
            }
            catch (FormatException)
            {
                BalanceText.Text = "Please input an integer";
            }

            //Update blockchain list
            List<TextBlock> blockchainBlockTexts = new List<TextBlock>();

            foreach (Block block in blockchain)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = $"ID: {block.Id}\n" +
                              $"Amount: {block.Amount}\n" +
                              $"From Wallet: {block.WalletFrom}\n" +
                              $"To Wallet: {block.WalletTo}\n" +
                              $"Block Offset: {block.BlockOffset}";
                
                blockchainBlockTexts.Add(textBlock);
            }

            BlockchainContainer.ItemsSource = blockchainBlockTexts;
        }

        private void UpdateGui(object sender, EventArgs e)
        {
            //Update miner status
            RestRequest minerStatusRequest = new RestRequest("api/status");
            IRestResponse minerStatusResponse = _minerServer.Get(minerStatusRequest);

            MinerStatusText.Text = minerStatusResponse.IsSuccessful ? minerStatusResponse.Content : $"Error - {minerStatusResponse.Content}";
        }

        private void SubmitTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            Transaction transaction;

            //Create new transaction based on transaction details
            try
            {
                transaction = new Transaction()
                {
                    WalletFrom = uint.Parse(WalletFromBox.Text),
                    WalletTo = uint.Parse(WalletToBox.Text),
                    Amount = float.Parse(AmountBox.Text)

                };
            }
            catch (FormatException)
            {
                MessageBox.Show("Could not parse input values for transaction.", "Parse error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Submit transaction to miner
            RestRequest request = new RestRequest("api/add-transaction");
            request.AddJsonBody(transaction);
            IRestResponse response = _minerServer.Post(request);

            if (!response.IsSuccessful)
            {
                MessageBox.Show($"Error submitting transaction - {response.Content}.", "Submission error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Transaction submitted!", "Submitted", MessageBoxButton.OK);
        }
    }
}
