using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace CoinPoker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IDisposable
    {
        Mutex appMutex;

        public App()
        {
            Startup += new StartupEventHandler(App_Startup);
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            bool aIsNewInstance = false;
            appMutex = new Mutex(true, "UnityPokerEUApplication", out aIsNewInstance);  
            if (!aIsNewInstance)
            {
              MessageBox.Show("Aplikacja jest już uruchomiona.");
              App.Current.Shutdown();  
            }

            (new Thread(() => {
                Session.NetworkConfiguration();
            })).Start();

            (new Thread(() => {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    LoadingWindow.Instance.Show();
                    LoadingWindow.Instance.ShowInTaskbar = false;
                    LoadingWindow.Instance.Topmost = true;
                    LoadingWindow.Instance.Activate();

                    (new Thread(() =>
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {
                            RenderOptions.ClearTypeHintProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata { DefaultValue = ClearTypeHint.Auto });
                            TextOptions.TextFormattingModeProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata { DefaultValue = TextFormattingMode.Display });
                        }));
                    })).Start();

                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Interval = new TimeSpan(0, 0, 3);
                    timer.Tick += (s, args) =>
                    {
                        CoinPoker.MainWindow.Instance.Show();

                        DispatcherTimer timer2 = new DispatcherTimer();
                        timer2.Interval = new TimeSpan(0, 0, 1);
                        timer2.Tick += (s2, args2) =>
                        {
                            LoadingWindow.Instance.Visibility = Visibility.Hidden;
                            LoadingWindow.Instance.Close();
                            timer2.Stop();
                        };
                        timer2.Start();

                        timer.Stop();
                    };
                    timer.Start();
                }));
            })).Start();

        }
    
        public void Dispose()
        {
        }

    }
}
