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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoinPoker.Controls
{
    /// <summary>
    /// Interaction logic for MenuPopup.xaml
    /// </summary>
    public partial class MenuPopup : UserControl
    {
        public static readonly DependencyProperty ArrowAlignmentProperty = DependencyProperty.Register("ArrowAlignment", typeof(string), typeof(MenuPopup));
        public static readonly DependencyProperty ArrowMarginProperty = DependencyProperty.Register("ArrowMargin", typeof(string), typeof(MenuPopup));

        /// <summary>
        /// Gets or sets additional content for the UserControl
        /// </summary>
        public object AdditionalContent
        {
            get { return (object)GetValue(AdditionalContentProperty); }
            set { SetValue(AdditionalContentProperty, value); }
        }
        public static readonly DependencyProperty AdditionalContentProperty =
            DependencyProperty.Register("AdditionalContent", typeof(object), typeof(MenuPopup),
              new PropertyMetadata(null));


        public string ArrowAlignment
        {
            get { return (string)GetValue(ArrowAlignmentProperty); }
            set { SetValue(ArrowAlignmentProperty, value); }
        }

        public string ArrowMargin
        {
            get { return (string)GetValue(ArrowMarginProperty); }
            set { SetValue(ArrowMarginProperty, value); }
        }

        public MenuPopup()
        {
            InitializeComponent();
            this.DataContext = this;
        }
    }
}
