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
    public partial class SeatOfferActionView : Page
    {
        PokerGameWindow parent;

        public SeatOfferActionView(PokerGameWindow parent)
        {
            InitializeComponent();
            this.parent = parent;
        }

        private void JoinToTable_Click(object sender, RoutedEventArgs e)
        {
            //Modal window
            NormalModeStackWindow stackWindow = new NormalModeStackWindow(this.parent);
            stackWindow.ShowModal(this.parent);
            stackWindow.Owner = this.parent;

            //MainWindow.Session.Client.ServiceProxy.DoAction(ActionModel.GameActionType.JoinGame,this.parent.table);
            //this.parent.AfterAction();
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
