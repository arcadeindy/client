using CoinPoker.Controllers;
using CoinPoker.Games;
using CoinPoker.Games.Poker;
using CoinPoker.Managers;
using CoinPokerCommonLib;
using CoinPokerCommonLib.Models.Action;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CoinPoker.GameControls
{
    public static class ExtensionMethods
    {
        private static Action EmptyDelegate = delegate() { };

        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
    }

    /// <summary>
    /// Interaction logic for Table.xaml
    /// </summary>
    public partial class Table : UserControl
    {
        //Lista miejsc
        public List<GameControls.Seat> SeatList = new List<GameControls.Seat>();

        /// <summary>
        /// Wartości stołu
        /// </summary>
        public TableModel GameTable { get; set; }
        public PokerGameWindow Window { get; set; }

        public Table()
        {
            //Dodajemy eventy
            this.Initialized += (ob, args) =>
            {
                //Zmiana ustawień wyglądu
                Preferences.OnPreferencesChangedEvent += Preferences_OnPreferencesChangedEvent;

                //Użytkownik wstaje ze stołu
                PokerClient.Instance.OnPlayerStandupEvent += Instance_OnPlayerStandupEvent;

                //Użytkownik dołącza do stołu
                PokerClient.Instance.OnPlayerSitdownEvent += Instance_OnPlayerSitdownEvent;

                //Czyszczenie stołu
                PokerClient.Instance.OnTableClearEvent += Instance_OnTableClearEvent;

                //Wiadomość systemowa
                PokerClient.Instance.OnGameTableSystemMessageEvent += Instance_OnGameTableSystemMessageEvent;

                //Gracz wykonał akcję
                PokerClient.Instance.OnPlayerActionTriggerEvent += Instance_OnPlayerActionTriggerEvent;

                //Pobiera historie całego rozdania od jego początku
                PokerClient.Instance.OnTableHitoryEvent += Instance_OnTableHitoryEvent;
            };

            this.Unloaded += (object sender, RoutedEventArgs e) =>
            {
                Preferences.OnPreferencesChangedEvent -= Preferences_OnPreferencesChangedEvent;
                PokerClient.Instance.OnPlayerStandupEvent -= Instance_OnPlayerStandupEvent;
                PokerClient.Instance.OnPlayerSitdownEvent -= Instance_OnPlayerSitdownEvent;
                PokerClient.Instance.OnTableClearEvent -= Instance_OnTableClearEvent;
                PokerClient.Instance.OnGameTableSystemMessageEvent -= Instance_OnGameTableSystemMessageEvent;
                PokerClient.Instance.OnPlayerActionTriggerEvent -= Instance_OnPlayerActionTriggerEvent;
                PokerClient.Instance.OnTableHitoryEvent -= Instance_OnTableHitoryEvent;
            };

            InitializeComponent();
        }

        void Instance_OnTableHitoryEvent(TableModel table, ObservableCollection<BaseAction> tableHistory)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (this.GameTable.ID == table.ID)
                {
                    this.ParseTable(table);

                    //Pobieramy akcje aktualnego stage
                    if (tableHistory != null)
                    {
                        //Pobieramy karty
                        List<BaseAction> actionListStage = tableHistory.ToList();

                        foreach (BaseAction action in actionListStage)
                        {
                            if (action is BetAction)
                            {
                                this.ParseBetAction(action, true);
                            }
                            else if (action is CardTableAction)
                            {
                                CardTableAction actionEntry = (CardTableAction)action;
                                AddTableCards(actionEntry.Cards, true);
                                CashierWorkspace.Children.Clear();
                            }
                        }
                    }

                    //Ustawiamy aktywnego uzytkownika jesli jest taki i jego timer
                    if (table.ActionPlayer != null)
                    {
                        var _seat = this.SeatList.
                            FirstOrDefault(p => p.player.User.ID == table.ActionPlayer.User.ID);

                        var lastTableHistoryEntry = tableHistory.LastOrDefault();
                        if (_seat != null && lastTableHistoryEntry != null)
                        {
                            TimeSpan difference = DateTime.Now.Subtract(lastTableHistoryEntry.CreatedAt);
                            _seat.RunTimer(table.ActionTimer, difference.TotalMilliseconds);
                        }
                    }
                }
            }));
        }

        void Instance_OnPlayerActionTriggerEvent(TableModel table, BaseAction action)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                Console.WriteLine("ON TABLE HISTORY EVENT");
                if (this.GameTable.ID == table.ID)
                {
                    if (action is BetAction)
                    {
                        this.ParseBetAction(action);
                    }
                    else if (action is CardTableAction)
                    {
                        CardTableAction actionEntry = (CardTableAction)action;
                        Console.WriteLine("CardTableAction <____");
                        OnCashierGroupMoney((o, e) =>
                        {
                            CashierWorkspace.Children.Clear();
                        });

                        AddTableCards(actionEntry.Cards, false);
                    }
                    else if (action is TablePotAction)
                    {
                        //Akcja otrzymania przez gracza funduszy
                        var actionEntry = action as TablePotAction;
                        this.OnCashierPotAction(actionEntry.Pot, actionEntry.Player);
                    }
                    else if (action is CardHighlightAction)
                    {
                        //Podświetlamy wygrane układy
                        //Bierzemy wszystkie kontrolki
                        /*CardHighlightAction actionEntry = (CardHighlightAction)action;

                        //Czyścimy wszystkie elementy = opacity 1.0

                        //Bierzemy seat playera
                        var playerCardList = this.SeatList.
                            FirstOrDefault(p => p.player != null && p.player.User.ID == actionEntry.Player.User.ID).
                            CardShowcase.Children.OfType<GameControls.Card>().
                            ToList();
                        
                        List<GameControls.Card> listOfCards = TableCards.Children.OfType<GameControls.Card>().ToList();
                        listOfCards = listOfCards.Concat(playerCardList).Distinct().ToList();*/
                    }

                    this.ParseTable(table);
                }
            }));
        }

        void Preferences_OnPreferencesChangedEvent()
        {
            LoadUserStyle();
        }

        void Instance_OnGameTableSystemMessageEvent(TableModel table, TableMessageModel.Status status, TableMessageModel message)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (this.GameTable.ID == table.ID)
                {
                    if (status == TableMessageModel.Status.SHOW)
                    {
                        Controls.Message messageControl = new Controls.Message(message);
                        messageControl.Visibility = Visibility.Hidden;
                        Messages.Children.Add(messageControl);

                        ((Storyboard)FindResource("fadeInAnimation")).Begin(messageControl);
                    }
                    else
                    {
                        Messages.Children.Clear();
                    }
                }
            }));
        }

        void Instance_OnTableClearEvent(TableModel table)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (this.GameTable.ID == table.ID)
                {
                    ClearTable();
                    this.ParseTable(table);
                    CashierWorkspace.Children.Clear();
                }
            }));
        }

        void Instance_OnPlayerStandupEvent(TableModel table, PlayerModel player)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (this.GameTable.ID == table.ID)
                {
                    SeatList.ElementAt(player.Seat).StandUp();
                }
            })); ;
        }

        void Instance_OnPlayerSitdownEvent(TableModel table, PlayerModel player)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (this.GameTable.ID == table.ID)
                {
                    //Dodajemyy element z listy do seats
                    Console.WriteLine("Usadzam gracza na " + player.Seat + "/" + (SeatList.Count() - 1));
                    SeatList.ElementAt(player.Seat).SitDownUser(player);
                }
            }));
        }

        private void ParseTable(TableModel table)
        {
            //Ustawienie stacka głównego w razie gydyby się zmienił za pośrednictwem gracza
            GameTable.Stage = table.Stage;
            GameTable.Dealer = table.Dealer;
            GameTable.Blind = table.Blind;
            GameTable.AvgPot = table.AvgPot;

            if (table.TablePot != 0)
            {
                this.GameTable.TablePot = table.TablePot;
                this.StackCounter.Visibility = Visibility.Visible;
                this.StackCounterLabel.Text = "Stawka: " + CurrencyFormat.Get(this.GameTable.Currency, this.GameTable.TablePot);
            }
        }

        /// <summary>
        /// Parsuje wykonanie akcji
        /// </summary>
        /// <param name="action"></param>
        private void ParseBetAction(IAction action, bool IsSilenceMode = false)
        {
            if (action is BetAction)
            {
                BetAction actionEntry = (BetAction)action;

                Seat playerSeat = SeatList.FirstOrDefault(s => s.player != null && s.player.User.ID == actionEntry.Player.User.ID);

                //Gracz wyszedł, nie ma sensu parsować jego przebić
                if (playerSeat == null) return;

                //Gracz wykonał akcję więc są zmiany
                if (!IsSilenceMode)
                {
                    playerSeat.UpdatePlayer(actionEntry.Player);
                }

                //Pasujemy akcje przebicia
                switch (actionEntry.Action)
                {
                    case Enums.ActionPokerType.Call:
                        //Sprawdzamy cze bet czy check
                        if (actionEntry.Bet == 0)
                        {
                            //check
                            if (!IsSilenceMode)
                                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                                {
                                    SoundManager.Play("Checkmark-4");
                                }));
                        }
                        else
                        {
                            //bid
                            if (!IsSilenceMode)
                                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                                {
                                    SoundManager.Play("Bet-4");
                                }));

                            //Dodajemy pieniądze na stół
                            OnPlayerBid(actionEntry.Player, actionEntry.Bet);
                        }
                        break;
                    case Enums.ActionPokerType.Raise:
                        //Wpłacamy pieniądze na stół
                        if (!IsSilenceMode)
                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                            {
                                SoundManager.Play("Bet-4");
                            }));

                        //Dodajemy pieniądze na stół
                        OnPlayerBid(actionEntry.Player, actionEntry.Bet);
                        break;
                    case Enums.ActionPokerType.Fold:

                        if (!IsSilenceMode)
                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                            {
                                SoundManager.Play("Fold-3");
                            }));

                        break;
                    case Enums.ActionPokerType.SmallBlind:
                    case Enums.ActionPokerType.BigBlind:
                        //Hajs na stół
                        OnPlayerBid(actionEntry.Player, actionEntry.Bet);
                        break;
                }
            }
        }

        /// <summary>
        /// Ustawienie miejsc dla graczy wraz z miejscami na stacki
        /// </summary>
        private void SeatsPlacer()
        {
            //Czyścymy elementy
            this.ellipseTableSeats.Children.Clear();
            this.SeatList.Clear();

            //Dodawanie miejsc / Pozycje etc / uzytkownicy
            for (int i = 0; i < GameTable.Seats; i++)
            {
                GameControls.Seat seat = new GameControls.Seat(this);
                if (this.GameTable.Type == Enums.TableType.Normal)
                {
                    seat.TakeSeat.Click += (o, e) =>
                    {
                        NormalModeStackWindow stackWindow = new NormalModeStackWindow(this.Window);
                        stackWindow.Seat = seat;
                        stackWindow.ShowModal(this.Window);
                        stackWindow.Owner = this.Window;
                    };
                }
                SeatList.Add(seat);
            }

            //Dodawanie iejsc do stolu
            foreach (GameControls.Seat seat in SeatList)
            {
                this.ellipseTableSeats.Children.Add(seat);
            }

            this.ellipseTableSeats.UpdateLayout();
        }

        /// <summary>
        /// Dodaje karty stołowe (+grupuje stacki w odpowiednie pule)
        /// </summary>
        /// <param name="cards"></param>
        public void AddTableCards(List<CardModel> cards, bool IsHistoryMode)
        {
            foreach (CardModel card in cards)
            {
                GameControls.Card cardControl = new GameControls.Card();
                cardControl.SetCard(card);

                if (!IsHistoryMode)
                    cardControl.Visibility = Visibility.Hidden;

                this.TableCards.Children.Add(cardControl);

                if (!IsHistoryMode)
                    ((Storyboard)FindResource("fadeInAnimation")).Begin(cardControl);
            }
        }

        private void LoadUserStyle()
        {
            var uri = @"pack://application:,,,/Unity.Assets;component/Assets/Game/Tables/" + Properties.Settings.Default.GameTableColor + ".png";
            this.TableVisual.Source = new BitmapImage(
                    new Uri(uri, UriKind.Absolute)
            );
        }

        public void InitializeTable()
        {
            this.Window = Helper.FindVisualParent<PokerGameWindow>(this);
            this.GameTable = this.Window.TableModel;

            //Styl gracza
            this.LoadUserStyle();

            //Usadzenie miejsc przy stole
            this.ClearTable();

            //Dodaje miejsca do gry
            this.SeatsPlacer();

            //Usadzenie graczy przy stolkach ktorzy sa juz przy nim
            foreach (PlayerModel player in GameTable.PlayersList)
            {
                var _seat = this.ellipseTableSeats.Children.
                    Cast<GameControls.Seat>().
                    ElementAt(player.Seat);

                _seat.SitDownUser(player);
            }

            this.ellipseTableSeats.Refresh();
            ellipseTableSeats.UpdateLayout();

            this.OnRenderSizeChanged(null);
            this.Refresh();
            this.UpdateLayout();
        }

        public void SetTable(TableModel table)
        {
            //Zapisywanie stołu
            this.GameTable = table;
            //Pobieranie listy graczy
            var tablePlayerList = Session.Data.Client.ServiceProxy.GetTablePlayers(table);
            this.GameTable.PlayersList = new ObservableCollection<PlayerModel>(tablePlayerList);
        }

        /// <summary>
        /// Dodaje kwotę na stół, gdy istnieje zmienia stan aktualnej
        /// </summary>
        /// <param name="player"></param>
        /// <param name="bid"></param>
        public void OnPlayerBid(PlayerModel player, decimal bid)
        {
            //Wyszukujemy kontrolkę odpowiedzialną za wyświetlanie naszego gracza
            GameControls.Seat seatObject = ellipseTableSeats.Children.
                OfType<GameControls.Seat>().
                FirstOrDefault(s=>s.player != null && s.player.User.ID == player.User.ID);

            if (seatObject == null) return;

            Vector playerCash = VisualTreeHelper.GetOffset(seatObject);
            Vector globalCash = VisualTreeHelper.GetOffset(TableStack);
            
            //Sprawdzamy czy już istnieje taka pozycja, jeśli tak zmieniamy ją na nową, jeśli nie dodajemy stack na planszę
            var playerChipsStack = this.CashierWorkspace.Children.
                OfType<GameControls.Chips>().
                FirstOrDefault(c => c.seat.player != null && c.seat.player.User.ID == player.User.ID);

            //Dodajemy stack na planszę
            if (playerChipsStack == null) { 
                //Gdy mamy gotową pozycję dodajemy element GameControls.Chips do CashierWorkspace
                var playerCashControl = new GameControls.Chips(this.SeatList.FirstOrDefault(s=>s.player != null && s.player.User.ID== player.User.ID));
                playerCashControl.SetChips(bid);

                double distance = 50;
                double scaleX = 1.0;
                double scaleY = 1.0;

                if (playerCash.Y < globalCash.Y) //górne pozycje
                {
                    distance = 35;
                    scaleX = 3.5;
                }
                else //dolne pozycje
                {
                    distance = 90;
                    scaleX = 1.55;
                }


                var stackPosition = Helper.MovePointTowards(
                    new Point(playerCash.X + seatObject.ActualWidth / 2, playerCash.Y + seatObject.ActualHeight / 2), 
                    new Point(globalCash.X, globalCash.Y),
                    distance,
                    scaleX,
                    scaleY
                );

                Canvas.SetLeft(playerCashControl, stackPosition.X);
                Canvas.SetTop(playerCashControl, stackPosition.Y);

                //Dodajemt element do naszej gry
                this.CashierWorkspace.Children.Add(playerCashControl);
             }
             else
             {
                playerChipsStack.AddChips(bid);
             }
        }

        /// <summary>
        /// Funkcja grupuje pieniądze graczy na jeden główny stack
        /// </summary>
        public void OnCashierGroupMoney(EventHandler OnAnimationCompleted)
        {
            Vector offsetTableStack = VisualTreeHelper.GetOffset(TableStack);
            Storyboard story = new Storyboard();

            foreach (GameControls.Chips playerCashControl in this.CashierWorkspace.Children)
            {
                TranslateTransform trans = new TranslateTransform();
                playerCashControl.RenderTransform = trans;
                DoubleAnimation animY = new DoubleAnimation(0, offsetTableStack.Y - Canvas.GetTop(playerCashControl), TimeSpan.FromSeconds(0.4));
                DoubleAnimation animX = new DoubleAnimation(0, offsetTableStack.X - Canvas.GetLeft(playerCashControl), TimeSpan.FromSeconds(0.4));

                Storyboard.SetTarget(animX, playerCashControl);
                Storyboard.SetTarget(animY, playerCashControl);
                Storyboard.SetTargetProperty(animX, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
                Storyboard.SetTargetProperty(animY, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));

                story.Children.Add(animY);
                story.Children.Add(animX);
            }

            story.Completed += (object sender, EventArgs e) =>
            {
                //Zamieniamy aktualny stack na jedna duza kupke

                //Liczymy pule
                this.CashierWorkspace.Children.Clear();

                var tablePotStack = new GameControls.Chips(null);
                tablePotStack.AddChips(this.GameTable.TablePot);

                Canvas.SetLeft(tablePotStack, offsetTableStack.X);
                Canvas.SetTop(tablePotStack, offsetTableStack.Y);

                this.CashierWorkspace.Children.Add(tablePotStack);
            };

            story.Completed += OnAnimationCompleted;
            story.Begin();
        }

        /// <summary>
        /// Funkcja rozdaje kwotę z puli do graczy
        /// </summary>
        public void OnCashierPotAction(decimal potValue, PlayerModel toPlayer)
        {
            /*GameControls.Seat seatObject = ellipseTableSeats.Children.
                OfType<GameControls.Seat>().
                FirstOrDefault(s => s.player != null && s.player.User.ID == toPlayer.User.ID);

            Vector offsetTableStack = VisualTreeHelper.GetOffset(TableStack);


            Storyboard story = new Storyboard();
            if (seatObject != null)
            {
                this.CashierWorkspace.Children.Clear();

                var tablePotStack = new GameControls.Chips(null);
                tablePotStack.AddChips(potValue);
                Canvas.SetLeft(tablePotStack, offsetTableStack.X);
                Canvas.SetTop(tablePotStack, offsetTableStack.Y);

                this.CashierWorkspace.Children.Add(tablePotStack);

                //Animujemy sume kasjera do gracza
                TranslateTransform trans = new TranslateTransform();
                tablePotStack.RenderTransform = trans;
                DoubleAnimation animY = new DoubleAnimation(0, offsetTableStack.Y - Canvas.GetTop(seatObject), TimeSpan.FromSeconds(0.4));
                DoubleAnimation animX = new DoubleAnimation(0, offsetTableStack.X - Canvas.GetLeft(seatObject), TimeSpan.FromSeconds(0.4));

                Storyboard.SetTarget(animX, seatObject);
                Storyboard.SetTarget(animY, seatObject);
                Storyboard.SetTargetProperty(animX, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
                Storyboard.SetTargetProperty(animY, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));

                story.Children.Add(animY);
                story.Children.Add(animX);
            }

            story.Completed += (object sender, EventArgs e) =>
            {
                this.CashierWorkspace.Children.Clear();
            };
            
            story.Begin();*/
        }

        /// <summary>
        /// Czyści stół
        /// </summary>
        public void ClearTable()
        {
            //Czyszczenie kart
            this.TableCards.Children.Clear();
            //Czyszczeni stacka
            this.CashierWorkspace.Children.Clear();

            this.StackCounter.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Zmiana rozmiaru okna
        /// </summary>
        /// <param name="sizeInfo"></param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            //Modyfikujemy stół w zaleznosci od okna
            this.ellipseTableSeats.EllipseCentreX = this.ActualWidth / 2 - 90;
            this.ellipseTableSeats.EllipseCentreY = this.ActualHeight / 2 - 33;
            this.ellipseTableSeats.EllipseHeight = (int)this.ActualHeight / 2.6;
            this.ellipseTableSeats.EllipseWidth = (int)this.ActualWidth / 2.6;

            this.ellipseTableSeats.Refresh();
        }
    }
}
