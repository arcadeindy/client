using CoinPoker.Games;
using CoinPoker.Games.Poker;
using CoinPokerCommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CoinPoker.Views.Game
{
    /// <summary>
    /// Interaction logic for JoinAvailable.xaml
    /// </summary>
    public partial class BackOfferActionView : Page
    {
        PokerGameWindow parent;

        public BackOfferActionView(PokerGameWindow parent)
        {
            InitializeComponent();
            this.parent = parent;

            var _player = this.parent.GameTable.GameTable.PlayersList.
                Where(c=>c.User.ID == Session.Data.User.ID).
                Select(c=>c).FirstOrDefault();

            this.Sit.Content = "Usiądź";
        }

        private void Sit_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                Session.Data.Client.ServiceProxy.DoAction(Enums.GameActionType.PlayerActivation, parent.GameTable.GameTable);
            });

            this.parent.AfterAction();
        }
    }
}
