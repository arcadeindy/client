using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace CoinPoker.Controls
{
    public class ModalWindow : StandardWindow
    {
        [DllImport("user32")]
        internal static extern bool EnableWindow(IntPtr hwnd, bool bEnable);

        public void ShowModal(Window window = null)
        {
            if (window == null)
                window = Application.Current.MainWindow;

            IntPtr handle = (new WindowInteropHelper(window)).Handle;
            EnableWindow(handle, false);

            this.Closed += delegate
            {
                EnableWindow(handle, true);
                if (window is MainWindow)
                {
                    ((Storyboard)FindResource("fadeOutAnimation")).Begin(MainWindow.Instance.ModalBackground);
                    ClearEffect(MainWindow.Instance);
                }

                this.Owner.Activate();
            };

            if (window is MainWindow)
            {
                ((Storyboard)FindResource("fadeInAnimation")).Begin(MainWindow.Instance.ModalBackground);
                ApplyEffect(MainWindow.Instance);
            }

            this.Owner = window;
            Show();
        }

        /// <summary>
        /// Dodaje efekt blur
        /// </summary>
        /// <param name="win"></param>
        private void ApplyEffect(Window win)
        {
            System.Windows.Media.Effects.BlurEffect objBlur = new System.Windows.Media.Effects.BlurEffect();
            objBlur.Radius = 3;
            win.Effect = objBlur;
        }

        /// <summary>
        /// Usuwa efekt blur
        /// </summary>
        /// <param name=”win”></param>
        private void ClearEffect(Window win)
        {
            win.Effect = null;
        }
    }
}
