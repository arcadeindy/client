using CoinPoker.Controllers;
using CoinPoker.Controls;
using CoinPokerCommonLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CoinPoker
{
    /// <summary>
    /// Interaction logic for ProfileEditWindow.xaml
    /// </summary>
    public partial class ProfileEditWindow : ModalWindow
    {
        private static ProfileEditWindow instance;

        public static ProfileEditWindow Instance
        {
            get
            {
                if (instance == null || instance.IsVisible == false)
                {
                    instance = new ProfileEditWindow();
                    instance.Initialize();
                }
                return instance;
            }
        }

        /// <summary>
        /// Prywatny konstruktor
        /// </summary>
        private ProfileEditWindow()
        {
            InitializeComponent();

            PokerClient.Instance.OnUserAvatarChangedEvent += Instance_OnUserAvatarChangedEvent;
        }

        void Instance_OnUserAvatarChangedEvent(UserModel user)
        {
            if (user.ID == Session.Data.User.ID)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    Session.Data.User.Avatar = user.Avatar;
                    AvatarImage.Source = new BitmapImage(new Uri(user.Avatar, UriKind.Absolute));
                }));
            }
        }

        public void Initialize()
        {
            Username.Text = Session.Data.User.Username;
            AvatarImage.Source = new BitmapImage(new Uri(Session.Data.User.Avatar, UriKind.Absolute));
            Update();
        }

        private void Update()
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    ProfileModel profile = Session.Data.Client.ServiceProxy.GetProfileUser(Session.Data.User);
                    InGameCounter.Text = "Liczba gier w których uczestniczysz: " + profile.PlayingOnTables.Count();
                }));
            });
        }

        private BitmapImage DrawingImageToMedia(Image drawingImage)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();

            MemoryStream ms = new MemoryStream();
            drawingImage.Save(ms, ImageFormat.Jpeg);
            ms.Seek(0, SeekOrigin.Begin);
            bi.StreamSource = ms;
            bi.EndInit();

            return bi;
        }

        public static byte[] ImagenToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }

        private void ChangeAvatar_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "JPG Files (*.jpg)|*.jpg|JPEG Files (*.jpeg)|*.jpeg";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;

                if (File.Exists(dlg.FileName))
                {
                    ChangeAvatar.IsEnabled = false;
                    Image UserImage = Image.FromFile(dlg.FileName);

                    byte[] avatarBuffor = ImagenToByteArray(UserImage);


                    //Wysyłanie nowego avatara
                    Task.Factory.StartNew(() =>
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {
                            var resultMessage = Session.Data.Client.ServiceProxy.DoChangeUserAvatar(avatarBuffor);
                            ChangeAvatar.IsEnabled = true;
                            MessageBox.Show("Zmiana avatara: " + resultMessage);
                        }));
                    });

                }
            }
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            ChangePasswordWindow.Instance.ShowModal(this);
        }

    }
}
