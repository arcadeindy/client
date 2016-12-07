using CoinPoker.Controls;
using CoinPoker.Managers;
using CoinPokerCommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CoinPoker
{
    /// <summary>
    /// Interaction logic for CashierWindow.xaml
    /// </summary>
    public partial class DepositInfoWindow : ModalWindow
    {
        /// <summary>
        /// Prywatny konstruktor
        /// </summary>
        public DepositInfoWindow(TransferModel transfer)
        {
            InitializeComponent();

            this.TransferComment.Text = transfer.Comment;
            this.TransferCurrencyName.Text = CurrencyFormat.GetName(transfer.Currency);
            this.TransferValue.Text = CurrencyFormat.Get(transfer.Currency, transfer.Amount);
            this.TransferBalanceCalculation.Text = CurrencyFormat.Get(transfer.Currency, transfer.WalletAmountBefore) + " + " + CurrencyFormat.Get(transfer.Currency, transfer.Amount) + " = " + CurrencyFormat.Get(transfer.Currency, transfer.WalletAmountBefore + transfer.Amount);

            SoundManager.Play("notification");
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void GoToCashier_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            CashierWindow.Instance.ShowModal(MainWindow.Instance);
        }

    }
}
