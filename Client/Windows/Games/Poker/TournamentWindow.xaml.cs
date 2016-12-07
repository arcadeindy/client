using CoinPoker.Controllers;
using CoinPoker.Controls;
using CoinPokerCommonLib;
using CoinPokerCommonLib.Models.Game.Tournament.WinningPotModel;
using CoinPokerCommonLib.Models.Game.TournamentOption;
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
    /// Interaction logic for TournamentWindow.xaml
    /// </summary>
    public partial class TournamentWindow : StandardWindow
    {
        public ITournamentGameModel GameModel { get; set; }

        public TournamentWindow(ITournamentGameModel tournamentModel)
        {
            GameModel = tournamentModel;

            PokerClient.Instance.OnTournamentGameModelUpdateEvent += Instance_OnTournamentGameModelUpdateEvent;
            PokerClient.Instance.OnTournamentTableListUpdateEvent += Instance_OnTournamentTableListUpdateEvent;

            InitializeComponent();

            this.WindowTitle.Text = "Lobby turniejowe - " + tournamentModel.TournamentModel.Name;
            this.Title = WindowTitle.Text;

            UpdateTournamentModel(tournamentModel);
            UpdateRegisterStatus();
            UpdateTournamentTableList(null);
        }

        void Instance_OnTournamentTableListUpdateEvent(ITournamentGameModel game, List<TableModel> tableList)
        {
            if (game.TournamentModel.ID == GameModel.TournamentModel.ID)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    UpdateTournamentTableList(tableList);
                }));
            }
        }

        private void UpdateTournamentTableList(List<TableModel> tableList)
        {
            if (tableList != null)
            {
                TableList.ItemsSource = tableList;
                NoTableToShow.Visibility = Visibility.Hidden;
            }
            else
            {
                TableList.ItemsSource = null;
                NoTableToShow.Visibility = Visibility.Visible;
            }
        }

        void Instance_OnTournamentGameModelUpdateEvent(ITournamentGameModel game)
        {
            if (game.TournamentModel.ID == GameModel.TournamentModel.ID)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    GameModel = game;
                    UpdateTournamentModel(game);
                }));
            }
        }

        private void UpdateTournamentModel(ITournamentGameModel tournamentModel)
        {
            this.Entries.Text = GameModel.TournamentModel.Registered + "/" + GameModel.TournamentModel.MaxPlayers;
            this.WinPot.Text = GameModel.TournamentModel.WinPotCurrency;
            this.WinPotSummary.Text = this.WinPot.Text;
            this.Status.Text = GameModel.TournamentModel.State.ToString();

            if (tournamentModel is SitAndGoTournamentGameModel)
            {
                this.WhenStart.Text = "gdy uzbiera się " + tournamentModel.TournamentModel.MaxPlayers + " graczy";
            }
            else
            {
                this.WhenStart.Text = tournamentModel.StartString;
            }

            SetTournamentDescription();

            RegisterInTournament.Visibility = Visibility.Collapsed;
            UnRegister.Visibility = Visibility.Collapsed;
            GetSeatTournament.Visibility = Visibility.Collapsed;

            //Miejsca platne
            UpdatePrizeList();
            UpdatePlayerList();

            this.DataContext = GameModel;
        }

        private void SetTournamentDescription()
        {
            
        }

        private void UpdateRegisterStatus()
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    var tournamentList = Session.Data.Client.ServiceProxy.RegisteredTournamentList();
                    var registeredInTournament = tournamentList.Any(c => c.TournamentModel.ID == GameModel.TournamentModel.ID);
                    var registerAvailable = (GameModel.TournamentModel.State == Enums.TournamentState.Registration ||
                        GameModel.TournamentModel.State == Enums.TournamentState.LateRegistration);

                    if (registeredInTournament && GameModel.IsStarted())
                    {
                        GetSeatTournament.Visibility = Visibility.Visible;
                        RegisterInTournament.Visibility = Visibility.Collapsed;
                        UnRegister.Visibility = Visibility.Collapsed;
                    }else if (registeredInTournament && registerAvailable)
                    {
                        GetSeatTournament.Visibility = Visibility.Collapsed;
                        RegisterInTournament.Visibility = Visibility.Collapsed;
                        UnRegister.Visibility = Visibility.Visible;
                    }
                    else if (!registeredInTournament && registerAvailable)
                    {
                        GetSeatTournament.Visibility = Visibility.Collapsed;
                        RegisterInTournament.Visibility = Visibility.Visible;
                        UnRegister.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        GetSeatTournament.Visibility = Visibility.Collapsed;
                        RegisterInTournament.Visibility = Visibility.Collapsed;
                        UnRegister.Visibility = Visibility.Collapsed;
                    }
                }));
            });

        }

        private void UpdatePlayerList()
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    var playerList = Session.Data.Client.ServiceProxy.GetTournamentPlayers(this.GameModel);
                    PlayerList.ItemsSource = playerList;
                }));
            });
        }

        private void UpdatePrizeList()
        {
            IWinningPotModel winningPotModel;

            switch (GameModel.TournamentModel.PrizeType)
            {
                case Enums.TournamentPrizeType.STATIC:
                    winningPotModel = new StaticWinningPotModel(GameModel.TournamentModel);
                    break;
                default:
                    return;
            }

            var elementList = winningPotModel.PrizeCalc();
            this.PayPlaces.Text = elementList.Count() + " miejsc płatnych";
            PrizeList.ItemsSource = null;
            PrizeList.ItemsSource = elementList;
        }

        private void CloseTournamentLobby_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void RegisterInTournament_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    try
                    {
                        Session.Data.Client.ServiceProxy.DoJoinTournamentMode(GameModel);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.InnerException.Message.ToString());
                    }
                    UpdateRegisterStatus();
                }));
            });
        }

        private void UnRegister_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    try
                    {
                        Session.Data.Client.ServiceProxy.DoLeftTournamentMode(GameModel);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.InnerException.Message.ToString());
                    }
                    UpdateRegisterStatus();
                }));
            });
        }

        /// <summary>
        /// Zajęcie miejsca 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetSeatTournament_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    try
                    {
                        var table = Session.Data.Client.ServiceProxy.GetTournametTable(GameModel);

                        if (table == null)
                        {
                            MessageBox.Show("Nie znaleziono gracza o Twoim identyfikatorze w turnieju. Sprawdź stan rejestracji turnieju.");
                            return;
                        }

                        if (Application.Current.Windows.OfType<PokerGameWindow>().Any(w => w.TableModel.ID == table.ID))
                        {
                            return;
                        }

                        PokerGameWindow gameWindow = new PokerGameWindow(table);
                        gameWindow.Show();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.InnerException.Message.ToString());
                    }
                }));
            });
        }

        private void ObserveTournamentTable_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
