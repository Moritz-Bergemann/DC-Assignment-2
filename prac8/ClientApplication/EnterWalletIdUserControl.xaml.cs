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
    /// Interaction logic for EnterWalletIdUserControl.xaml
    /// </summary>
    public partial class EnterWalletIdUserControl : UserControl
    {
        private Action<uint> _callback;

        public EnterWalletIdUserControl(Action<uint> callback)
        {
            _callback = callback;

            InitializeComponent();
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uint walletId = uint.Parse(WalletIdInputBox.Text);

                _callback(walletId);

                Window.GetWindow(this).DialogResult = true;
                Window.GetWindow(this).Close();
            }
            catch (FormatException)
            {
                MessageBox.Show("Please input a non-negative integer as the wallet ID.");
            }

        }
    }
}
