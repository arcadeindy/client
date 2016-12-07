using CoinPoker.Controllers;
using CoinPoker.Controls;
using CoinPoker.Games;
using CoinPoker.Managers;
using CoinPoker.Views;
using CoinPokerCommonLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Media;
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CoinPoker
{
    /// <summary>
    /// Okno główne aplikacji
    /// </summary>
    public partial class MainWindow : StandardWindow
    {
        private static MainWindow instance;

        public static MainWindow Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MainWindow();
                    instance.Initialize();
                }
                return instance;
            }
        }

        [DllImport("user32")]
        internal static extern bool EnableWindow(IntPtr hwnd, bool bEnable);

        /// <summary>
        /// Prywatny konstruktor
        /// Inicjalizacja połączenia do serwera
        /// Ustawienia czcionek/AA i wydarzenie MainWindows_Loaded
        /// </summary>
        private MainWindow()
        {
            Console.WriteLine("MainWindow()");
            InitializeComponent();
        }

        private void Initialize()
        {
            //Ustawienie event`u logowania
            PokerClient.Instance.OnLoginSuccessfullEvent += (o) =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    SoundManager.Play("login");
                }));
            };

            Session.Data.Client.Connected += (e, o) =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    MainWindow.Instance.NoConnection.Children.Clear();
                    MainWindowMainBorder.Effect = null;
                    IntPtr handle = (new WindowInteropHelper(this)).Handle;
                    EnableWindow(handle, true);

                    if (!Session.Data.IsLogged)
                    {
                        LoginWindow.Instance.ShowModal(this);
                    }
                }));
            };

            Session.Data.Client.Disconnected += (e, o) =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    MainWindow.Instance.NoConnection.Children.Add(new Controls.Disconnected());

                    BlurEffect objBlur = new BlurEffect();
                    objBlur.Radius = 3;
                    MainWindowMainBorder.Effect = objBlur;
                    IntPtr handle = (new WindowInteropHelper(this)).Handle;
                    EnableWindow(handle, false);


                    if (!Session.Data.IsLogged)
                    {
                        LoginWindow.Instance.Close();
                    }
                }));
            };

            this.Loaded += (elsed, o) =>
            {
                if (Session.IsConnected())
                {
                    LoginWindow.Instance.ShowModal(this);
                }
                else
                {
                    MainWindow.Instance.NoConnection.Children.Add(new Controls.Disconnected());

                    BlurEffect objBlur = new BlurEffect();
                    objBlur.Radius = 3;
                    MainWindowMainBorder.Effect = objBlur;
                    IntPtr handle = (new WindowInteropHelper(this)).Handle;
                    EnableWindow(handle, false);
                }
            };
        }

        /// <summary>
        /// onConfigurationClick
        /// </summary>
        private void OnConfigurationClick(object sender, MouseButtonEventArgs e)
        {
            ConfigurationWindow.Instance.ShowModal(MainWindow.Instance);
        }

        /// <summary>
        /// onCloseClick
        /// </summary>
        private void OnCloseClick(object sender, MouseButtonEventArgs e)
        {
            IEnumerable<PokerGameWindow> windows = Application.Current.Windows.OfType<PokerGameWindow>();

            if (windows.Count() > 0)
            {
                if (MessageBox.Show("W tym momencie masz otwarte okna z trwającymi rozgrywkami, czy jesteś pewien, że chcesz wyjść?",
                    "Potwierdzenie", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// onMinimizeClick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMinimizeClick(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// onMaximizeClick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMaximizeClick(object sender, MouseButtonEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else
                this.WindowState = WindowState.Normal;
        }

        /// <summary>
        /// onSupportClick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSupportClick(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.unitypoker.eu/");
        }
    }
}
