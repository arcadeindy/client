using CoinPoker.Controls;
using CoinPoker.Views.Game;
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
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;
using CoinPoker.Controllers;
using CoinPokerCommonLib.Models.Action;
using System.Text.RegularExpressions;
using System.Windows.Media.Effects;
using CoinPokerCommonLib.Models.OfferAction;

namespace CoinPoker.Games
{

    /// <summary>
    /// Stół do gry w pokera (obsługuje TableModel)
    /// </summary>
    public partial class PokerGameWindow : Window
    {
        //Przechowujemy typ gry
        public TableModel TableModel { get; set; }

        //Cenzor
        string[] BadWords = new[] {
            "kurwa",
            "chuj",
            "jebany",
            "jebać",
            "dupa",
            "pierdole",
            "pierdolony",
            "szmata",
        };

        const string CensoredText = "***";
        const string PatternTemplate = @"\b({0})(s?)\b";
        const RegexOptions Options = RegexOptions.IgnoreCase;

        //Historia rozdania
        public ObservableCollection<BaseAction> GameHistory { get; set; }

        /// <summary>
        /// Stół do gry normalnej
        /// </summary>
        /// <param name="gameModel"></param>
        public PokerGameWindow(TableModel tableModel)
        {
            this.TableModel = tableModel;

            this.Initialized += (o, args) =>
            {
                //Zmiana ustawień wyglądu
                Preferences.OnPreferencesChangedEvent += Preferences_OnPreferencesChangedEvent;

                //Inicjalizacja dla normalnego stołu gry
                PokerClient.Instance.OnTableHitoryEvent += Instance_OnTableHitoryEvent;

                //Gracz wykonał akcję
                PokerClient.Instance.OnPlayerActionTriggerEvent += Instance_OnPlayerActionTriggerEvent;

                //Czyszczenie stołu
                PokerClient.Instance.OnTableClearEvent += Instance_OnTableClearEvent;

                //Zmiana stołu
                PokerClient.Instance.OnGameTableUserMoveEvent += Instance_OnGameTablePlayerMoveEvent;

                //Obsługa zerwania połączenia
                Session.Data.Client.Disconnected += Client_Disconnected;
                Session.Data.Client.Connected += Client_Connected;
            };

            Closing += OnWindowClosing;
            Loaded += new RoutedEventHandler(OnWindowLoaded);
            InitializeComponent();

            //Uchwyty gry

            //Ustawienie eventów
            PokerClient.Instance.OnGameTableUserMessageEvent += Instance_OnGameTableUserMessageEvent;

            //Użytkownik odchodzi od stołu
            PokerClient.Instance.OnPlayerStandupEvent += Instance_OnPlayerStandupEvent;

            //Gracz dołącza do stołu
            PokerClient.Instance.OnPlayerSitdownEvent += Instance_OnPlayerSitdownEvent;

            //Gracz ma nowe zadanie
            PokerClient.Instance.OnGameActionOfferEvent += Instance_OnGameActionOfferEvent;

            //Aktualizacja stołu gry (zewnetrzna)
            PokerClient.Instance.OnNormalModeUpdateEvent += Instance_OnNormalModeUpdateEvent;

            //Inicjalizacja stołu do gry i samej rozgrywki
            InitializeGameTable();
        }

        void Instance_OnGameTablePlayerMoveEvent(UserModel user, TableModel oldTable, TableModel newTable)
        {
            //Jeśli zmiana uzytkownika dotyczy zalogowanego uzytkownika
            if (user.ID == Session.Data.User.ID)
            {
                //Jeśli zmiana stołu dotyczy tego okna
                if (oldTable.ID == TableModel.ID)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        AddMessageLine(null, "Przeniesiono Cię do stołu: " + newTable.Name);

                        //Odłączamy stare eventy od stołu gry i czyscimy okno stolu do gry
                        this.GameTable.Clear();

                        //Rejestrujemy nowy stół
                        this.TableModel = newTable;

                        //Inicjalizujemy nowy stół do gry
                        this.InitializeGameTable();
                    }));
                }
            }
        }


        void Client_Connected(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                GameWindowRootNode.Effect = null;
                NoConnection.Children.Clear();
            }));
        }

        void Client_Disconnected(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                BlurEffect objBlur = new BlurEffect();
                objBlur.Radius = 3;
                GameWindowRootNode.Effect = objBlur;
                NoConnection.Children.Add(new Controls.Disconnected());
            }));
        }

        void Instance_OnTableClearEvent(TableModel table)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (this.GameTable.GameTable.ID == table.ID)
                {
                    this.GameHistory.Clear();
                }
            }));
        }

        void Instance_OnPlayerActionTriggerEvent(TableModel table, BaseAction action)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (this.GameTable.GameTable.ID == table.ID)
                {
                    this.GameHistory.Add(action);
                }
            }));
        }

        void Instance_OnTableHitoryEvent(TableModel table, ObservableCollection<BaseAction> tableHistory)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (this.GameTable.GameTable.ID == table.ID)
                {
                    this.GameHistory = tableHistory;
                }
            }));
        }

        void Preferences_OnPreferencesChangedEvent()
        {
            LoadUserStyle();
        }

        private void LoadUserStyle()
        {
            var uri = @"pack://application:,,,/Unity.Assets;component/Assets/Game/Backgrounds/" + Properties.Settings.Default.GameBackground + ".png";
            this.Background.ImageSource = new BitmapImage(
                    new Uri(uri, UriKind.Absolute)
            );
        }

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="table"></param>
        public void InitializeGameTable()
        {
            this.GameHistory = new ObservableCollection<BaseAction>();
            this.TableModel.ActionHistory = this.GameHistory;

            this.Title = TableModel.Name;

            //Pobieranie listy graczy
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    var tablePlayerList = Session.Data.Client.ServiceProxy.GetTablePlayers(TableModel);
                    this.TableModel.PlayersList = new ObservableCollection<PlayerModel>(tablePlayerList);

                    this.GameTable.InitializeTable();

                    //Pokazujemy stół
                    ((Storyboard)FindResource("fadeInAnimation")).Begin(GameTable);
                }));
            });

            //Dodajemy obserwatora gry
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    Session.Data.Client.ServiceProxy.DoAction(Enums.GameActionType.Watch, TableModel);
                }));
            });

            UpdateInfoLabels(this.TableModel);
        }

        void Instance_OnNormalModeUpdateEvent(NormalGameModel game)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (game.Table.ID == this.TableModel.ID)
                    UpdateInfoLabels(game.Table);
            }));
        }

        void Instance_OnGameActionOfferEvent(TableModel table, BaseOfferAction action)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (this.TableModel.ID == table.ID)
                {
                    if (action is BetOfferAction)
                    {
                        var actionModel = (BetOfferAction)action;

                        //Bet Action powinno iść tylko do gracza który je otrzymuje, jako jedyny jest rozsyłany do wszystkich
                        //musimy wiec sprawdzić czy na pewno jest to nasz bet action
                        if (actionModel.Player.User.ID == Session.Data.User.ID)
                        {
                            UserAction.Content = new Views.Game.BetOfferActionView(this, actionModel);
                        }
                    }
                    else if (action is FindAnotherTableOfferAction)
                    {
                        var actionModel = (FindAnotherTableOfferAction)action;
                        UserAction.Content = new Views.Game.FindAnotherTableOfferActionView(this);
                    }
                    else if (action is SeatOfferAction)
                    {
                        var actionModel = (SeatOfferAction)action;
                        UserAction.Content = new Views.Game.SeatOfferActionView(this);
                    }
                    else if (action is HideOfferAction)
                    {
                        var actionModel = (HideOfferAction)action;
                        UserAction.Content = null;
                    }
                    else if (action is BackOfferAction)
                    {
                        var actionModel = (BackOfferAction)action;
                        UserAction.Content = new Views.Game.BackOfferActionView(this);
                    }
                }
            }));
        }

        void Instance_OnPlayerSitdownEvent(TableModel table, PlayerModel player)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (this.TableModel.ID == table.ID)
                {
                    this.TableModel.PlayersList.Add(player);
                }
            }));
        }

        void Instance_OnPlayerStandupEvent(TableModel table, PlayerModel player)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (this.TableModel.ID == table.ID && this.TableModel.PlayersList.ElementAtOrDefault(player.Seat) != null)
                {
                    this.TableModel.PlayersList.RemoveAt(player.Seat);
                }
            }));
        }

        private void AddMessageLine(UserModel user, string message)
        {
            TextBlock tb = new TextBlock();

            if (user != null)
            {
                //Kolor nicka
                var nickRun = new Run();
                nickRun.Foreground = new SolidColorBrush(Colors.LightGray);
                nickRun.Text = user.Username;
                nickRun.FontWeight = FontWeights.Bold;

                //Cenzor
                if (Properties.Settings.Default.Censor)
                {
                    IEnumerable<Regex> badWordMatchers = this.BadWords.
                        Select(x => new Regex(string.Format(PatternTemplate, x), Options));

                    message = badWordMatchers.
                       Aggregate(message, (current, matcher) => matcher.Replace(current, CensoredText));
                }

                //Tekst
                var messageRun = new Run();
                messageRun.Foreground = new SolidColorBrush(Colors.White);
                messageRun.Text = ": " + message;
                tb.Inlines.Add(nickRun);

                //Dodajemy do textblocka
                tb.Inlines.Add(messageRun);
            }
            else
            {
                tb.Text = "Dealer: " + message;
                tb.Foreground = new SolidColorBrush(Colors.Gray);
            }

            tb.TextWrapping = TextWrapping.Wrap;

            Chat.Children.Add(tb);
            ChatScroll.ScrollToEnd();
        }

        void Instance_OnGameTableUserMessageEvent(TableModel table, UserModel user, string message)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (this.GameTable.GameTable.ID == table.ID)
                {
                    AddMessageLine(user, message);
                }
            }));
        }

        private void UpdateInfoLabels(TableModel table)
        {
            this.AvgPotLabel.Text = "Średnia pula: " + table.AvgPotCurrency;

            if (table.ActionHistoryID != null)
                this.HandIDLabel.Text = "Rozdanie: " + table.ActionHistoryID;
            else
                this.HandIDLabel.Text = "Żadna gra aktualnie się nie toczy";
        }

        /// <summary>
        /// Załadowanie okna gry
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Ładuje style gracza
            this.LoadUserStyle();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        /// <summary>
        /// Wysyłanie wiadomości czatu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChatMessageEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string message = this.ChatInput.Text;
                this.ChatInput.Clear();

                Task.Factory.StartNew(() =>
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        Session.Data.Client.ServiceProxy.SendMessageToTable(TableModel, message);
                    }));
                });
            }
        }

        /// <summary>
        /// Zamkyanie okna
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var isPlaying = this.TableModel.PlayersList.Where(c => c.User.ID == Session.Data.User.ID).Select(c => c).FirstOrDefault();
            if (isPlaying != null)
            {
                    MessageBoxResult messageBoxResult;

                    if (TableModel.Type == Enums.TableType.Normal)
                    {
                        if (isPlaying.Status.HasFlag(PlayerModel.PlayerStatus.FOLDED))
                            return;

                        messageBoxResult = System.Windows.MessageBox.Show("Wychodząc podczas rozgrywki twoja ręka zostanie spasowana oraz zostaniesz usunięty ze stołu. Czy na pewno chcesz wyjść?", "Czy na pewno chcesz wyjść?", System.Windows.MessageBoxButton.YesNo);
                    }
                    else
                    {
                        messageBoxResult = System.Windows.MessageBox.Show("Gra turniejowa będzie dalej toczyć się z Twoim udziałem nawet jeśli wyjdziesz. Czy chcesz na pewno opuścić turniej?", "Czy na pewno chcesz wyjść?", System.Windows.MessageBoxButton.YesNo);
                    } 
                    
                    if (messageBoxResult == MessageBoxResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }
            }

            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    //Wstajemy od stolu jesli siedziemy przy nim
                    if (isPlaying != null)
                        Session.Data.Client.ServiceProxy.DoAction(Enums.GameActionType.LeaveGame, TableModel);

                    //Przestajemy obserwowac stol
                    Session.Data.Client.ServiceProxy.DoAction(Enums.GameActionType.Unwatch, TableModel);
                }));
            });
        }

        /// <summary>
        /// Czyści możliwość akcji użytkownika
        /// </summary>
        public void AfterAction()
        {
            UserAction.Content = null;
        }

        private void StyleSettingButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ConfigurationWindow.Instance.ShowModal(MainWindow.Instance);
        }
    }
}
