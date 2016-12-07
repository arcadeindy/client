using CoinPoker.Controls;
using CoinPokerCommonLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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
    public class BooleanOrConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            foreach (object value in values)
            {
                if ((bool)value == true)
                {
                    return true;
                }
            }
            return false;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }


    /// <summary>
    /// Interaction logic for CashierWindow.xaml
    /// </summary>
    public partial class RegisterWindow : ModalWindow, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private static RegisterWindow instance;

        private UserModel user;
        public UserModel User
        {
            get
            {
                return user;
            }
            set
            {
                if (value.Username.Length < 5)
                {
                    throw new Exception("Nazwa użytkownika ma zbyt mało liter.");
                }
                user = value;
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public static RegisterWindow Instance
        {
            get
            {
                if (instance == null || instance.IsVisible == false)
                {
                    instance = new RegisterWindow();
                }
                return instance;
            }
        }

        /// <summary>
        /// Prywatny konstruktor
        /// </summary>
        private RegisterWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public bool IsValidEmail(string emailaddress)
        {
            if (emailaddress == "") return false;

            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static Regex sUserNameAllowedRegEx = new Regex(@"^[a-zA-Z]{1}[a-zA-Z0-9\._\-]{0,23}[^.-]$", RegexOptions.Compiled);
        
        public static bool IsUserNameAllowed(string userName)
        {
            if (string.IsNullOrEmpty(userName)
                || !sUserNameAllowedRegEx.IsMatch(userName)
                )
            {
                return false;
            }
            return true;
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {

            if (UsernameTxtBox.Text.Length < 4)
            {
                MessageBox.Show("Minimalna długość nazwy użytkownika wynosi 4 znaki.");
                return;
            }

            if (!IsUserNameAllowed(UsernameTxtBox.Text))
            {
                MessageBox.Show("Nieprawidłowa nazwa użytkownika, dozwolone są znaki alfanumeryczne, cyfry oraz znaki specjalne _ i myślniki. Maksymalna długość to 23 znaki.");
                return;
            }

            if (!IsValidEmail(EmailTxtBox.Text))
            {
                MessageBox.Show("Podano nieprawidłowy adres email");
                return;
            }

            if (PasswordTxtBox.Password != RepeatPasswordTxtBox.Password)
            {
                MessageBox.Show("Hasła różnią się");
                return;
            }


            var user = new UserModel()
            {
                Username = UsernameTxtBox.Text,
                Password = PasswordTxtBox.Password,
                Email = EmailTxtBox.Text
            };

            try
            {
                Session.Data.Client.ServiceProxy.DoRegister(user);
                MessageBox.Show("Zostałeś poprawnie zarejestrowany");
                this.Close();
            }catch(Exception ex)
            {
                MessageBox.Show("Wystąpił błąd podczas rejestracji: " + ex.InnerException.Message);
                return;
            }
        }

    }
}
