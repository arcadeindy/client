using CoinPoker.Controls;
using CoinPoker.Managers;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace CoinPoker
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : ModalWindow
    {
        private static ConfigurationWindow instance;

        public static ConfigurationWindow Instance
        {
            get
            {
                if (instance == null || instance.IsVisible == false)
                {
                    instance = new ConfigurationWindow();
                }
                return instance;
            }
        }

        public List<CoinPoker.Preferences.ComboItem> GameTableBackgroundList;
        public List<CoinPoker.Preferences.ComboItem> GameBackgroundList;

        /// <summary>
        /// Prywatny konstruktor
        /// </summary>
        private ConfigurationWindow()
        {
            InitializeComponent();

            GameTableBackgroundList = Preferences.GetTableList();
            GameBackgroundList = Preferences.GetBackgroundList();

            this.GameTableBackground.ItemsSource = GameTableBackgroundList;
            this.GameBackground.ItemsSource = GameBackgroundList;

            this.GameTableBackground.SelectedIndex = Properties.Settings.Default.GameTableColor;
            this.GameBackground.SelectedIndex = Properties.Settings.Default.GameBackground;

            this.DataContext = this;
        }

        private void SaveConfiguration_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Preferences.OnPreferencesChanged();
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void Cancel_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            SoundManager.Play("Alert-5");
        }

        private void GameTableBackground_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.GameTableColor = GameTableBackground.SelectedIndex;
            Preferences.OnPreferencesChanged();
        }

        private void GameBackground_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.GameBackground = GameBackground.SelectedIndex;
            Preferences.OnPreferencesChanged();
        }
    }
}
