using System;
using System.Collections.Generic;
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
    /// Interaction logic for CashierWindow.xaml
    /// </summary>
    public partial class CashierWindow : Window
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
        /// onCloseClick
        /// </summary>
        private void onCloseClick(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Prywatny konstruktor
        /// </summary>
        private CashierWindow()
        {
            InitializeComponent();
        }

    }
}
