using CoinPoker.Controllers;
using CoinPoker.Controls;
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
    public partial class LoginWindow : ModalWindow
    {
        private static LoginWindow instance;

        public static LoginWindow Instance
        {
            get
            {
                if (instance == null || instance.IsVisible == false)
                {
                    instance = new LoginWindow();
                }
                return instance;
            }
        }

        /// <summary>
        /// Prywatny konstruktor
        /// </summary>
        private LoginWindow()
        {
            InitializeComponent();

            PokerClient.Instance.OnLoginSuccessfullEvent += (o) =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    RegisterWindow.Instance.Close();
                    this.Close();
                }));
            };

            RememberUsername.IsChecked = Properties.Settings.Default.RememberUsername;
            if (Properties.Settings.Default.Username != "")
            {
                UsernameTxtBox.Text = Properties.Settings.Default.Username;
            }

            Console.WriteLine(Properties.Settings.Default.Username);
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow.Instance.ShowModal(this);
        }

        private void LoginAction()
        {
            //Sprawdzamy wporwadzone dane
            if (UsernameTxtBox.Text == "")
            {
                MessageBox.Show("Wprowadź poprawną nazwę użytkownika");
                return;
            }

            if (PasswordTxtBox.Password == "")
            {
                MessageBox.Show("Wprowadź hasło");
                return;
            }

            Login.IsEnabled = false;

            if (RememberUsername.IsChecked == true)
            {
                Properties.Settings.Default.Username = UsernameTxtBox.Text;
            }
            else
            {
                Properties.Settings.Default.Username = "";
            }

            Properties.Settings.Default.RememberUsername = (bool)RememberUsername.IsChecked;
            Properties.Settings.Default.Save();

            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    try
                    {
                        Session.Data.Client.ServiceProxy.DoLogin(UsernameTxtBox.Text, PasswordTxtBox.Password);
                    }
                    catch (Exception ex)
                    {
                        Login.IsEnabled = true;
                        MessageBox.Show("Wystąpił błąd podczas logowania. " + ex.InnerException.Message);
                    }

                }));
            });
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            LoginAction();
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                LoginAction();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


    }
}
