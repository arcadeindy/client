using CoinPoker.Games;
using CoinPoker.Games.Poker;
using CoinPokerCommonLib;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoinPoker.Views.Game
{
    /// <summary>
    /// Interaction logic for JoinAvailable.xaml
    /// </summary>
    public partial class FindAnotherTableOfferActionView : Page
    {
        PokerGameWindow parent;

        public FindAnotherTableOfferActionView(PokerGameWindow parent)
        {
            InitializeComponent();
            this.parent = parent;
        }

        private void FindAnotherTable_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                Session.Data.Client.ServiceProxy.DoAction(Enums.GameActionType.FindAnotherTable, this.parent.TableModel);
            });
        }

    }
}
