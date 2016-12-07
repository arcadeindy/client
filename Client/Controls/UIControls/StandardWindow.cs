using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace CoinPoker.Controls
{
    public class StandardWindow : Window
    {
        public void OnCloseClick(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        protected void OnMinimizeClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        protected void OnRestoreClick(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        }

        public override void OnApplyTemplate()
        {
            TextBlock closeButton = GetTemplateChild("WindowCloseButton") as TextBlock;
            if (closeButton != null)
                closeButton.MouseDown += OnCloseClick;

            base.OnApplyTemplate();
        }
    }
}
