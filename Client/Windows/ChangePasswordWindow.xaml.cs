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
    public partial class ChangePasswordWindow : ModalWindow
    {
        private static ChangePasswordWindow instance;

        public static ChangePasswordWindow Instance
        {
            get
            {
                if (instance == null || instance.IsVisible == false)
                {
                    instance = new ChangePasswordWindow();
                }
                return instance;
            }
        }

        /// <summary>
        /// Prywatny konstruktor
        /// </summary>
        private ChangePasswordWindow()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordTxtBox.Password != PasswordTxtBox2.Password)
            {
                MessageBox.Show("Podane hasła muszą się zgadzać");
            }

            if (Session.Data.Client.ServiceProxy.DoChangePassword(OldPasswordTxtBox.Password, PasswordTxtBox.Password))
            {
                MessageBox.Show("Poprawnie zmieniono hasło");
                this.Close();
            }
            else
            {
                MessageBox.Show("Podane przez Ciebie hasło nie jest poprawne");
            }
        }

    }
}
