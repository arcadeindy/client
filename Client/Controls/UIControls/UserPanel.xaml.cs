using CoinPoker.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Cache;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CoinPoker.Controls
{
    /// <summary>
    /// Interaction logic for UserPanel.xaml
    /// </summary>
    public partial class UserPanel : UserControl
    {
        public UserPanel()
        {
            InitializeComponent();
            this.UserPanelInner.Visibility = Visibility.Hidden;

            this.AvatarLoader.Visibility = Visibility.Visible;

            this.DataContext = this;

            PokerClient.Instance.OnLoginSuccessfullEvent += (o) =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    this.UserPanelInner.Visibility = Visibility.Visible;

                    this.Update();
                    this.GetAvatar();
                }));
            };

            PokerClient.Instance.OnUserAvatarChangedEvent += Instance_OnUserAvatarChangedEvent;
        }

        void Instance_OnUserAvatarChangedEvent(CoinPokerCommonLib.UserModel user)
        {
            if (user.ID == Session.Data.User.ID)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    AvatarImage.Source = new BitmapImage(new Uri(user.Avatar, UriKind.Absolute));
                }));
            }
        }

        public void GetAvatar()
        {
            AvatarImage.Source = new BitmapImage(new Uri(Session.Data.User.Avatar, UriKind.Absolute));
        }

        public void Update()
        {
            this.UsernameLbl.Text = Session.Data.User.Username;
        }

        private void OnUserButtonClick(object sender, MouseButtonEventArgs e)
        {
            //userPopup.IsOpen = !userPopup.IsOpen;
        }

        private void OnCashierButtonClick(object sender, MouseButtonEventArgs e)
        {
            CashierWindow.Instance.ShowModal(MainWindow.Instance);
        }

        private void OnFeedButtonClick(object sender, MouseButtonEventArgs e)
        {
            //feedPopup.IsOpen = !feedPopup.IsOpen;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow.Instance.ShowModal(MainWindow.Instance);
        }

        private void userButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Otwiera okno profilowe (zmiana profilu/avatara + lista powiadomien)
            ProfileEditWindow.Instance.ShowModal(MainWindow.Instance);
        }
    }
}
