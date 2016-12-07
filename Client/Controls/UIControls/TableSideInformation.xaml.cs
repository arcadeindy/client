using CoinPoker.Views;
using CoinPokerCommonLib;
using CoinPokerCommonLib.Models.Game.TournamentOption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for TableSideInformation.xaml
    /// </summary>
    public partial class TableSideInformation : UserControl
    {
        public class TableSideElement
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Value { get; set; }
        }

        public TableSideInformation()
        {
            InitializeComponent();
            Loader.Visibility = Visibility.Visible;
            NoPlayers.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Informacje o stole
        /// </summary>
        /// <param name="table"></param>
        public void SetNormalModeNote(NormalGameModel normalMode)
        {
            if (normalMode == null) return;
            TablePlayerList.Visibility = Visibility.Visible;

            //Ukrywamy loader
            Loader.Visibility = Visibility.Hidden;

            //Wypisujemy informacje o stole
            EventName.Text = normalMode.Name;
            EventStatus.Text = "Gra stołowa";
            EventNameID.Text = "ID: #" + (normalMode.ID).ToString().PadLeft(8, '0');

            //PlayerList.ItemsSource = normalMode.Table.PlayersList;
            PlayerList.ItemsSource = null;
            PlayerList.ItemsSource = Session.Data.Client.ServiceProxy.GetTablePlayers(normalMode.Table);

            //Usuwamy click handler
            var routedEventHandlers = Helper.GetRoutedEventHandlers(JoinButton, ButtonBase.ClickEvent);
            if (routedEventHandlers != null)
                foreach (var routedEventHandler in routedEventHandlers)
                    JoinButton.Click -= (RoutedEventHandler)routedEventHandler.Handler;

            //Dajemy zmieniony click handler
            JoinButton.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) => {
                Lobby.OpenGameWindow(normalMode);
            });

            //Parsujemy informacje na temat stolu do listy elementow
            TableSideElementList.ItemsSource = null;
            List<TableSideElement> tableSideElementList = new List<TableSideElement>();
            tableSideElementList.Add(
                new TableSideElement()
                {
                    Title = "Ilość graczy:",
                    Description = "Zajęte miejsca/Maksymalna ilość miejsc",
                    Value = normalMode.Table.Players + "/" + normalMode.Table.Seats
                }
            );
            tableSideElementList.Add(
                new TableSideElement()
                {
                    Title = "Średni stack:",
                    Description = "Ostatnie 15 rozdań",
                    Value = normalMode.Table.AvgPotCurrency
                }
            );
            tableSideElementList.Add(
                new TableSideElement()
                {
                    Title = "Minimalna kwota:",
                    Description = "Minimalna kwota wejścia do gry",
                    Value = CurrencyFormat.Get(normalMode.Currency, normalMode.Minimum)
                }
            );

            TableSideElementList.ItemsSource = tableSideElementList;

            //Jeśli nie ma graczy ukrywamy panel
            if (normalMode.Table.Players == 0)
            {
                NoPlayers.Visibility = Visibility.Visible;
            }
            else
            {
                NoPlayers.Visibility = Visibility.Hidden;
            }
        }

        public void SetTournamentModeNote(ITournamentGameModel tournamentMode)
        {
            //Wypisujemy informacje o turnieju
            TablePlayerList.Visibility = Visibility.Collapsed;
            PlayerList.ItemsSource = null;

            EventName.Text = tournamentMode.TournamentModel.Name;
            EventStatus.Text = "Gra turniejowa";
            EventNameID.Text = "ID: #" + (tournamentMode.TournamentModel.ID).ToString().PadLeft(8, '0');

            //Parsujemy informacje na temat stolu do listy elementow
            TableSideElementList.ItemsSource = null;
            List<TableSideElement> tableSideElementList = new List<TableSideElement>();

            tableSideElementList.Add(
                new TableSideElement()
                {
                    Title = "Kwota rejestracji:",
                    Description = "Kwota do zapłaty przy rejestracji",
                    Value = tournamentMode.TournamentModel.EntryPaymentCurrency
                }
            );

            if (tournamentMode.TournamentModel.State != Enums.TournamentState.Running && tournamentMode.TournamentModel.State != Enums.TournamentState.LateRegistration && tournamentMode.TournamentModel.State != Enums.TournamentState.Completed)
            {
                tableSideElementList.Add(
                    new TableSideElement()
                    {
                        Title = "Start turnieju:",
                        Description = "Kiedy turniej się zaczyna",
                        Value = tournamentMode.StartString
                    }
                );
            }

            tableSideElementList.Add(
                new TableSideElement()
                {
                    Title = "Ilość graczy:",
                    Description = "Zarejestrowani gracze",
                    Value = tournamentMode.TournamentModel.Registered.ToString()
                }
            );
            tableSideElementList.Add(
                new TableSideElement()
                {
                    Title = "Status turnieju:",
                    Description = "Aktualny status",
                    Value = tournamentMode.TournamentModel.State.ToString()
                }
            );

            TableSideElementList.ItemsSource = tableSideElementList;

            if (tournamentMode == null) return;
        }

        private void LikeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }
    }
}
