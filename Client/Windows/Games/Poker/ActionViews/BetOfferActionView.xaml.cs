using CoinPoker.Games;
using CoinPokerCommonLib;
using CoinPokerCommonLib.Models.OfferAction;
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
    public partial class BetOfferActionView : Page
    {
        PokerGameWindow parent;
        BetOfferAction action;

        public BetOfferActionView(PokerGameWindow parent, BetOfferAction action)
        {
            InitializeComponent();
            this.parent = parent;
            this.action = action;

            //Pobieramy naszego gracza
            var _player = (from p in parent.TableModel.PlayersList
                           where p.User.ID == Session.Data.User.ID
                           select p).FirstOrDefault();

            this.BidSlider.Maximum = (double)this.action.MaxBet;
            this.BidSlider.TickFrequency = (double)this.action.BetTick;
            this.BidSlider.Minimum = (double)action.MinBet;

            this.BidSlider.Value = this.BidSlider.Minimum;

            //Jesli za malo pieniedzy ukrywamy elementy przebiajajce
            if (this.BidSlider.Value > (double)_player.Stack)
            {
                RiseBetButton.Visibility = Visibility.Hidden;
                SliderPanel.Visibility = Visibility.Hidden;
            }

            this.UpdateRiseButton();
            this.UpdateCheckCallButton();
        }

        private void UpdateCheckCallButton()
        {
            var gameTable = this.parent.GameTable.GameTable;

            if (this.action.LastBet != 0)
                this.CheckCallButton.Content = "Sprawdzam " + this.action.LastBet.ToString("c");
            else
                this.CheckCallButton.Content = "Czekam";
        }

        private void UpdateRiseButton()
        {
            var gameTable = this.parent.GameTable.GameTable;

            if (this.action.LastBet != 0)
                this.RiseBetButton.Content = "Przebijam " + ((decimal)(this.BidSlider.Value)).ToString("c");
            else
                this.RiseBetButton.Content = "Stawiam " + ((decimal)(this.BidSlider.Value)).ToString("c");
        }

        private void Fold_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var table = this.parent.TableModel;
                var bid = (decimal)this.BidSlider.Value;

                Task.Factory.StartNew(() =>
                {
                    Session.Data.Client.ServiceProxy.DoGameAction(table, Enums.ActionPokerType.Fold, 0);
                });

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
            this.parent.AfterAction();
        }

        private void RiseBetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var table = this.parent.TableModel;
                var bid = (decimal)this.BidSlider.Value;
                Task.Factory.StartNew(() =>
                {
                    Session.Data.Client.ServiceProxy.DoGameAction(table, Enums.ActionPokerType.Raise, bid);
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
            this.parent.AfterAction();
        }

        private void CheckCall_Click(object sender, RoutedEventArgs e)
        {
            try{
                var table = this.parent.TableModel;
                var bid = (decimal)this.BidSlider.Value;

                Task.Factory.StartNew(() =>
                {
                    Session.Data.Client.ServiceProxy.DoGameAction(table, Enums.ActionPokerType.Call, 0);
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
            this.parent.AfterAction();
        }

        private void BidSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //Aktualizacja buttonu
            this.UpdateRiseButton();
            this.BidValue.Text = BidSlider.Value.ToString();
        }

        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            BidSlider.Value = BidSlider.Minimum;
        }

        private void MaxButton_Click(object sender, RoutedEventArgs e)
        {
            BidSlider.Value = BidSlider.Maximum;
        }

        private void BidValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                this.BidValue.Text = double.Parse(this.BidValue.Text).ToString();
                this.BidSlider.Value = double.Parse(this.BidValue.Text);
            }
            catch (Exception ex)
            {
                this.BidSlider.Value = this.BidSlider.Maximum;
            }
        }
    }
}
