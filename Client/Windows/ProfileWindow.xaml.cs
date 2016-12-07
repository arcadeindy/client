using CoinPoker.Controls;
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
    public partial class ProfileWindow : StandardWindow
    {
        UserModel User { get; set; }

        /// <summary>
        /// Prywatny konstruktor
        /// </summary>
        public ProfileWindow(UserModel user)
        {
            this.User = user;
            InitializeComponent();

            this.Title = "Profil gracza " + user.Username;
            this.UsernameLabel.Text = user.Username;

            this.LoadProfile();
        }

        //Ładuje profil
        private void LoadProfile()
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    ProfileModel profile = Session.Data.Client.ServiceProxy.GetProfileUser(this.User);

                    ///PlayingOnTablesList.ItemsSource = profile.PlayingOnTables;
                   
                }));
            });

        }

        private void OnUnityTableListClickItem(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            TableModel game = button.DataContext as TableModel;
            
        }
    }
}
