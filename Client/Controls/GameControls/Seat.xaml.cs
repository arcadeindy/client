using CoinPoker.Controllers;
using CoinPoker.Managers;
using CoinPokerCommonLib;
using CoinPokerCommonLib.Models.Action;
using CoinPokerCommonLib.Models.OfferAction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Net.Cache;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CoinPoker.GameControls
{
    /// <summary>
    /// Interaction logic for Player.xaml
    /// </summary>
    public partial class Seat : UserControl
    {
        public PlayerModel player;

        public static readonly RoutedEvent TimerEvent = EventManager.RegisterRoutedEvent("Timer", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Seat));

        private bool isAnimating = false;
        private Storyboard blinkAnimation;

        public event RoutedEventHandler Timer
        {
            add
            {
                this.AddHandler(TimerEvent, value);
            }

            remove
            {
                this.RemoveHandler(TimerEvent, value);
            }
        }

        //Progress timer
        DateTime StartTime { get; set; }
        DateTime ActionTime;
        int TimerMS { get; set; }

        DispatcherTimer TimerCountdown;
        public GameControls.Table gameTable;

        public Seat(GameControls.Table gameTable)
        {
            this.gameTable = gameTable;

            this.Initialized += (obj, args) =>
            {
                //Aktywacja 'akcji' gracza
                PokerClient.Instance.OnGameActionOfferEvent += Instance_OnGameActionOfferEvent;

                //Historia rozdania
                PokerClient.Instance.OnTableHitoryEvent += Instance_OnTableHitoryEvent;

                //Aktualizacja gracza np zmiana statusu
                PokerClient.Instance.OnGamePlayerUpdateEvent += Instance_OnGamePlayerUpdateEvent;

                //Gracz wykonał akcję
                PokerClient.Instance.OnPlayerActionTriggerEvent += Instance_OnPlayerActionTriggerEvent;

                //Zmiana avatara gracza
                PokerClient.Instance.OnUserAvatarChangedEvent += Instance_OnUserAvatarChangedEvent;
            };

            this.Unloaded += (object sender, RoutedEventArgs e) =>
            {
                PokerClient.Instance.OnGameActionOfferEvent -= Instance_OnGameActionOfferEvent;
                PokerClient.Instance.OnTableHitoryEvent -= Instance_OnTableHitoryEvent;
                PokerClient.Instance.OnGamePlayerUpdateEvent -= Instance_OnGamePlayerUpdateEvent;
                PokerClient.Instance.OnPlayerActionTriggerEvent -= Instance_OnPlayerActionTriggerEvent;
            };

            InitializeComponent();

            SeatInfo.Visibility = Visibility.Visible;
            SeatInfoLabel.Text = "Wolne miejsce";

            UserInfo.Visibility = Visibility.Hidden;
            UserAvatarGrid.Visibility = UserInfo.Visibility;
            IsDealer.Visibility = Visibility.Hidden;

            blinkAnimation = ((Storyboard)FindResource("blinkAnimation"));

            DataContext = this;
        }

        void Instance_OnUserAvatarChangedEvent(UserModel user)
        {
            if (player != null && user.ID == player.User.ID)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    UserAvatar.Source = new BitmapImage(new Uri(user.Avatar, UriKind.Absolute));
                }));
            }
        }

        void Instance_OnPlayerActionTriggerEvent(TableModel table, BaseAction action)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (this.gameTable.GameTable.ID != table.ID || this.player == null) return;

                if (action is BetAction)
                {
                    BetAction actionEntry = (BetAction)action;

                    if (this.player.User.ID == actionEntry.Player.User.ID)
                    {
                        ParserPlayer(actionEntry.Player);
                        CloseTimer();

                        //Jeśli fold, ukrywamy karty
                        if (actionEntry.Action == Enums.ActionPokerType.Fold)
                            HideCards();
                    }
                }
                else if (action is CardBacksideAction)
                {
                    //Otrzymanie kart (tyły kart)
                    CardBacksideAction actionEntry = (CardBacksideAction)action;

                    if (this.player.User.ID == actionEntry.Player.User.ID)
                    {
                        ParserPlayer(actionEntry.Player);
                        ShowEmptyCards(actionEntry.Count);
                    }
                }
                else if (action is CardHideupAction)
                {
                    CardHideupAction actionEntry = (CardHideupAction)action;

                    if (this.player.User.ID == actionEntry.Player.User.ID)
                    {
                        ParserPlayer(actionEntry.Player);
                        HideCards();
                    }

                }
                else if (action is CardShowupAction)
                {
                    CardShowupAction actionEntry = (CardShowupAction)action;
                    if (this.player.User.ID == actionEntry.Player.User.ID)
                    {
                        ParserPlayer(actionEntry.Player);
                        ShowCards(actionEntry.Cards);
                    }
                }
                else if (action is TablePotAction)
                {
                    TablePotAction actionEntry = (TablePotAction)action;
                    if (this.player.User.ID == actionEntry.Player.User.ID)
                    {
                        ParserPlayer(actionEntry.Player);
                    }
                }
            }));
        }

        void Instance_OnGamePlayerUpdateEvent(TableModel table, PlayerModel player)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (this.gameTable.GameTable.ID != table.ID || this.player == null) return;

                if (this.player.User.ID == player.User.ID)
                    ParserPlayer(player);
            }));
        }

        void Instance_OnTableHitoryEvent(TableModel table, ObservableCollection<BaseAction> tableHistory)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (this.gameTable.GameTable.ID != table.ID || this.player == null) return;

                //Pobieramy informacje ile gracz kart ma na ręce
                foreach (IAction action in tableHistory)
                {
                    if (action is CardBacksideAction)
                    {
                        //Otrzymanie kart (tyły kart)
                        CardBacksideAction actionEntry = (CardBacksideAction)action;

                        if (this.player.User.ID == actionEntry.Player.User.ID)
                        {
                            ShowEmptyCards(actionEntry.Count);
                        }
                    }
                    else if (action is CardHideupAction)
                    {
                        CardHideupAction actionEntry = (CardHideupAction)action;

                        if (this.player.User.ID == actionEntry.Player.User.ID)
                        {
                            HideCards();
                        }
                    }
                    else if (action is CardShowupAction)
                    {
                        CardShowupAction actionEntry = (CardShowupAction)action;

                        if (this.player.User.ID == actionEntry.Player.User.ID)
                        {
                            ShowCards(actionEntry.Cards);
                        }
                    }
                }
            }));
        }

        void Instance_OnGameActionOfferEvent(TableModel table, BaseOfferAction action)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (this.gameTable.GameTable.ID == table.ID)
                {
                    if (action is BetOfferAction)
                    {
                        var actionModel = (BetOfferAction)action;

                        if (this.player != null && actionModel.Player.User.ID == this.player.User.ID)
                        {
                            ParserPlayer(actionModel.Player);
                            RunTimer(actionModel.Time);
                        }
                    }
                }
            }));
        }

        public void BackgroundBlink()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(TimerEvent);
            RaiseEvent(newEventArgs);
        }

        public void SetDealer(bool is_dealer)
        {
            if (is_dealer)
            {
                IsDealer.Visibility = Visibility.Visible;
            }
            else
            {
                IsDealer.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Uruchamia czas na zdarzenie
        /// </summary>
        /// <param name="ms"></param>
        public void RunTimer(int ms, double actionStartFrom = 0)
        {
            TimerMS = ms;
            StartTime = DateTime.Now;

            if (actionStartFrom != 0)
                ActionTime = DateTime.Now.AddMilliseconds(-actionStartFrom);
            else
                ActionTime = DateTime.Now;

            GameTimer.Visibility = Visibility.Visible;

            TimerCountdown = new DispatcherTimer();

            TimerCountdown.Tick += new System.EventHandler(Timer_Tick);
            TimerCountdown.Interval = new TimeSpan(0, 0, 0, 0, 0);

            TimerCountdown.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            //Różnica między startem a aktualnym czasem
            TimeSpan difference = DateTime.Now.Subtract(StartTime);

            //W razie gdyby zaczeto akcje od okreslonego czasu
            TimeSpan startDifference = StartTime.Subtract(ActionTime);

            //Ustawienie aktualnej pozycji procentowej
            GameTimerProgress.Percentage = (double)((difference.TotalMilliseconds + startDifference.TotalMilliseconds) / (TimerMS)) * 100;


            if (GameTimerProgress.Percentage > 75)
            {
                if (!isAnimating)
                {
                    isAnimating = true;
                }
            }

            if (GameTimerProgress.Percentage >= 100)
            {
                this.CloseTimer();
                this.GameTimerProgress.Percentage = 0;

                if (isAnimating)
                {
                    blinkAnimation.Stop(this.GameTimerProgress);
                    isAnimating = false;
                }
                return;
            }
        }

        public void CloseTimer()
        {
            if (TimerCountdown != null)
                TimerCountdown.Stop();
            GameTimer.Visibility = Visibility.Hidden;
        }

        public bool IsEmpty()
        {
            return (this.player == null) ? true : false;
        }

        public void SitDownUser(PlayerModel player)
        {
            SeatInfo.Visibility = Visibility.Hidden;
            UserAvatarGrid.Visibility = Visibility.Visible;

            this.UpdatePlayer(player);
            this.GetAvatar();
            this.Update();

            this.HideCards();

            UserInfo.Visibility = Visibility.Visible;
        }

        public void GetAvatar()
        {
            //Pobieramy avatar użytkownika
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    UserAvatar.Source = new BitmapImage(new Uri(player.User.Avatar, UriKind.Absolute));
                }));
            });
        }

        public void ParserPlayer(PlayerModel player)
        {
            UpdatePlayer(player);
            Update();
        }

        public void UpdatePlayer(PlayerModel player)
        {
            this.player = player;
        }

        public void Update()
        {
            //W zaleznosci od statusu
            if (player.Status.HasFlag(PlayerModel.PlayerStatus.DONTPLAY) || player.Status.HasFlag(PlayerModel.PlayerStatus.LEAVED))
            {
                SeatInfo.Visibility = Visibility.Hidden;
                UserInfo.Visibility = Visibility.Visible;
                UsernameLbl.Text = player.User.Username;
                StatusLabel.Text = "Nie gra";
                ColorBrush.Opacity = 0.3;
            }
            else if (player.Status.HasFlag(PlayerModel.PlayerStatus.FOLDED) || player.Status.HasFlag(PlayerModel.PlayerStatus.WAITING))
            {
                SeatInfo.Visibility = Visibility.Hidden;
                UserInfo.Visibility = Visibility.Visible;
                UsernameLbl.Text = player.User.Username;
                StatusLabel.Text = "Saldo: " + CurrencyFormat.Get(this.gameTable.GameTable.Currency, player.Stack);
                ColorBrush.Opacity = 0.3;
            }
            else if (player.Status.HasFlag(PlayerModel.PlayerStatus.INGAME))
            {
                SeatInfo.Visibility = Visibility.Hidden;
                UserInfo.Visibility = Visibility.Visible;
                UsernameLbl.Text = player.User.Username;
                StatusLabel.Text = "Saldo: " + CurrencyFormat.Get(this.gameTable.GameTable.Currency, player.Stack);
                ColorBrush.Opacity = 0.7;

                if (player.Stack == 0.0m)
                {
                    StatusLabel.Text = "ALL IN";
                }
            }

            if (gameTable.GameTable.Dealer!=null && gameTable.GameTable.Dealer.User.ID == player.User.ID)
            {
                this.IsDealer.Visibility = Visibility.Visible;
            }
            else
            {
                this.IsDealer.Visibility = Visibility.Hidden;
            }
        }

        public void StandUp()
        {
            SeatInfo.Visibility = Visibility.Visible;
            SeatInfoLabel.Text = "Wolne miejsce";
            ColorBrush.Opacity = 0.3;

            UserAvatarGrid.Visibility = Visibility.Hidden;
            this.player = null;

            UsernameLbl.Text = "";
            StatusLabel.Text = "";

            HideCards();

            UserInfo.Visibility = Visibility.Hidden;
        }

        public void ShowCards(List<CardModel> cards)
        {
            this.CardShowcase.Children.Clear();
            CardShowcase.Visibility = Visibility.Visible;

            int marginLeft = 0;
            int marginTop = -20;
            foreach (CardModel card in cards)
            {
                GameControls.Card cardUI = new GameControls.Card();
                cardUI.SetCard(card);
                cardUI.Margin = new Thickness(marginLeft, marginTop, 0, 0);
                this.CardShowcase.Children.Add(cardUI);
                marginLeft += 35;
                marginTop += 5;
            }
        }

        public void ShowEmptyCards(int count)
        {
            this.CardShowcase.Children.Clear();
            CardShowcase.Visibility = Visibility.Visible;

            int marginLeft = 0;
            int marginTop = -20;
            for (int i = 0; i < count; i++)
            {
                GameControls.Card cardUI = new GameControls.Card();
                cardUI.SetCard(null);
                cardUI.Margin = new Thickness(marginLeft, marginTop, 0, 0);
                this.CardShowcase.Children.Add(cardUI);
                marginLeft += 35;
                marginTop += 5;
            }
        }

        public void HideCards()
        {
            CardShowcase.Visibility = Visibility.Hidden;

            //TODO nadal widoczne po najechaniu myszka
        }

        private void SeatInfo_MouseEnter(object sender, MouseEventArgs e)
        {
            //jeśli użytkownik siedzi przy stole nie wyświetlamy mu przycisku do siadania na miejscach
            if (
                gameTable.GameTable.PlayersList.Where(p => p.User.ID == Session.Data.User.ID).Select(c => c).FirstOrDefault() == null &&
                gameTable.GameTable.Type == Enums.TableType.Normal
                )
                FreeSeat.Visibility = Visibility.Visible;
        }

        private void SeatInfo_MouseLeave(object sender, MouseEventArgs e)
        {
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            FreeSeat.Visibility = Visibility.Hidden;
        }

        private void UserAvatarHover_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ProfileWindow profile = new ProfileWindow(this.player.User);
            profile.Owner = gameTable.Window;
            profile.Show();
        }

        private void UserAvatar_MouseEnter(object sender, MouseEventArgs e)
        {
            //UserAvatarHover.Visibility = Visibility.Visible;
            //if (this.Cursor != Cursors.Wait)
            //    Mouse.OverrideCursor = Cursors.Hand;
        }

        private void UserAvatarHover_MouseLeave(object sender, MouseEventArgs e)
        {
            //UserAvatarHover.Visibility = Visibility.Hidden;
            //if (this.Cursor != Cursors.Wait)
            //    Mouse.OverrideCursor = Cursors.Arrow;
        }
    }
}
