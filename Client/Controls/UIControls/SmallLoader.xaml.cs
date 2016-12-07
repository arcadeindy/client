using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace CoinPoker.Controls
{
    /// <summary>
    /// Interaction logic for SmallLoader.xaml
    /// </summary>
    public partial class SmallLoader : UserControl
    {
        public SmallLoader()
        {
            InitializeComponent();
            Visibility = Visibility.Hidden;
        }

        public void Show()
        {
            //this.Visibility = Visibility.Visible;
            ((Storyboard)FindResource("fadeInAnimation")).Begin(this);
        }

        public void Hide()
        {
            //this.Visibility = Visibility.Hidden;
            ((Storyboard)FindResource("fadeOutAnimation")).Begin(this);
        }
    }
}
