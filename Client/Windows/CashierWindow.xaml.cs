using CoinPoker.Controls;
using CoinPokerCommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CoinPoker
{
    /// <summary>
    /// Interaction logic for CashierWindow.xaml
    /// </summary>
    public partial class CashierWindow : ModalWindow
    {
        private static CashierWindow instance;

        public static CashierWindow Instance
        {
            get
            {
                if (instance == null || instance.IsVisible == false)
                {
                    instance = new CashierWindow();
                }
                return instance;
            }
        }

        /// <summary>
        /// Prywatny konstruktor
        /// </summary>
        private CashierWindow()
        {
            InitializeComponent();
            this.Loader.Visibility = Visibility.Visible;

            Update();

            if (CurrencyList.Items.Count>0)
                CurrencyList.SelectedItem = CurrencyList.Items.GetItemAt(0);

            this.Username.Text = Session.Data.User.Username;
        }

        private void Update()
        {
            //Pobieramy wallet
            List<WalletModel> walletList = Session.Data.Client.ServiceProxy.GetWalletList();
            this.CurrencyList.ItemsSource = walletList;
        }

        private void CurrencyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadNewWallet();

            if (((WalletModel)CurrencyList.SelectedItem).Type == Enums.CurrencyType.VIRTUAL)
            {
                this.PayOut.IsEnabled = false;
            }
            else
            {
                this.PayOut.IsEnabled = true;
            }
        }

        private void LoadNewWallet()
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    WalletModel wallet = CurrencyList.SelectedItem as WalletModel;
                    //Pobieramy zaktualizowany element
                    wallet = Session.Data.Client.ServiceProxy.GetWallet(wallet.Type);

                    this.Available.Text = CurrencyFormat.Get(wallet.Type, wallet.Available);
                    this.InGame.Text = CurrencyFormat.Get(wallet.Type, wallet.InGame);
                    this.Sum.Text = CurrencyFormat.Get(wallet.Type, wallet.Sum);
                    this.Sum2.Text = this.Sum.Text;
                    this.Currency.Text = wallet.Type.ToString();

                    //Ładujemy także historię
                    List<TransferModel> historyList = Session.Data.Client.ServiceProxy.GetTransferOperations(wallet);
                    this.TransferOperationList.ItemsSource = historyList;

                    ((Storyboard)FindResource("fadeOutAnimation")).Begin(Loader);
                }));
            });
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            ((Storyboard)FindResource("fadeInAnimation")).Begin(Loader);
            LoadNewWallet();
        }

        private void Pay_Click(object sender, RoutedEventArgs e)
        {
            WalletModel currentWallet = ((WalletModel)CurrencyList.SelectedItem);

            //Wysyłamy żądanie otwarcia nowej płatności
            switch(currentWallet.Type){
                case Enums.CurrencyType.VIRTUAL:
                    //Dla waluty wirtualnej nie potrzebne jest żadne okno wpłat więc od razu wysyłamy taką informację
                    Session.Data.Client.ServiceProxy.DoTransferRequest(currentWallet.Type);
                    MessageBox.Show("Żądanie wpłaty wirtualnych pieniędzy zostało wysłane.");
                    break;
            }

        }

    }
}
