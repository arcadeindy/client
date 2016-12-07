using CoinPoker.Controls;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CoinPoker.Games.Poker
{
    /// <summary>
    /// Interaction logic for StackWindow.xaml
    /// </summary>
    public partial class NormalModeStackWindow : ModalWindow
    {
        NormalGameModel game;
        PokerGameWindow parent;
        WalletModel wallet;
        public GameControls.Seat Seat { get; set; }

        public NormalModeStackWindow(PokerGameWindow parent)
        {
            InitializeComponent();
            this.parent = parent;
            this.game = (NormalGameModel)Session.Data.Client.ServiceProxy.GetGameModelByTable(parent.TableModel);

            this.AvailableBallance.Visibility = Visibility.Hidden;

            this.JoinButton.IsEnabled = false;

            this.table_name.Text = this.game.Name;
            this.Blind.Text = this.game.Table.Stakes;

            this.StackSlider.TickFrequency = (double)this.game.Table.Blind;
            this.StackSlider.Minimum = (double)this.game.Minimum;
            this.StackSlider.Maximum = (double)this.game.Maximum;
            this.MinAmount.Text = CurrencyFormat.Get(
                                    this.game.Table.Currency, ((decimal)this.StackSlider.Minimum)
                                  );
            this.StackSlider.Value = this.StackSlider.Maximum;

            this.StackValue.Text = this.StackSlider.Value.ToString();

            //Pobieramy wallet
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    wallet = Session.Data.Client.ServiceProxy.GetWallet(this.game.Table.Currency);
                    if (wallet == null)
                    {
                        AvailableBallance.Text = "Brak konta z walutą";
                    }
                    else
                    {
                        AvailableBallance.Text = CurrencyFormat.Get(this.game.Table.Currency, wallet.Available);
                    }

                    if (wallet!=null && wallet.Available > this.game.Minimum)
                    {
                        if (wallet.Available < this.game.Maximum)
                        {
                            this.StackSlider.Maximum = (double)wallet.Available;
                        }
                        this.JoinButton.IsEnabled = true;

                        //Tworzymy rezerwacje
                        //Session.Data.Client.ServiceProxy.DoAction(ActionModel.GameActionType.Booking, this.game.Table);
                    }
                    else
                    {
                        this.JoinButton.Content = "Brak funduszy";
                    }

                    this.Loader.Visibility = Visibility.Hidden;
                    this.AvailableBallance.Visibility = Visibility.Visible;
                }));
            });
        }

        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            StackSlider.Value = StackSlider.Minimum;
        }

        private void MaxButton_Click(object sender, RoutedEventArgs e)
        {
            StackSlider.Value = StackSlider.Maximum;
        }

        private void StackSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.StackValue.Text = this.StackSlider.Value.ToString();
        }

        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int preferedPlaceID = PlayerModel.AUTO_SEAT;

                if (this.Seat != null)
                {
                    preferedPlaceID = this.parent.GameTable.SeatList.IndexOf(this.Seat); //Wybrane miejsce przez uzytkownika
                }

                try
                {
                    Task.Factory.StartNew(() => {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() => {
                            Session.Data.Client.ServiceProxy.DoJoinNormalMode(this.game, preferedPlaceID, (decimal)this.StackSlider.Value);
                            //Usuwamy panel akcji
                            this.parent.AfterAction();
                            //Zamkykamy okno
                            this.Close();
                        }));
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Wystąpił błąd podczas próby zajęcia miejsca:\n" + ex.Message.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void StackValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                this.StackValue.Text = double.Parse(this.StackValue.Text).ToString();
                this.StackSlider.Value = double.Parse(this.StackValue.Text);
            }
            catch (Exception ex)
            {
                this.StackSlider.Value = this.StackSlider.Maximum;
            }
        }
    }
}
