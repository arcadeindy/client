using CoinPoker.Client.Helpers;
using CoinPoker.Games;
using CoinPoker.Views;
using CoinPokerCommonLib;
using CoinPokerCommonLib.Models;
using CoinPokerCommonLib.Models.Action;
using CoinPokerCommonLib.Models.Game.TournamentOption;
using CoinPokerCommonLib.Models.OfferAction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CoinPoker.Controllers
{
    class PokerClient : IPokerClient
    {
        public static PokerClient Instance { get; private set; }

        static PokerClient()
        {
            Instance = new PokerClient();
        }

        /// <summary>
        /// Poprawne zalogowanie w systemi
        /// </summary>
        /// <param name="SessionID"></param>
        public event Action<bool> OnLoginSuccessfullEvent;
        public void OnLoginSuccessfull(long client_id, UserModel user)
        {
            Session.Data.SessionID = client_id;
            Session.Data.User = user;

            //Przechodzi do głównego lobby
            if (OnLoginSuccessfullEvent != null)
                OnLoginSuccessfullEvent(true);
        }


        /// <summary>
        /// Wiadomość na stole
        /// </summary>
        /// <param name="table"></param>
        /// <param name="user"></param>
        /// <param name="message"></param>
        public event Action<TableModel, UserModel, string> OnGameTableUserMessageEvent;
        public void OnGameTableUserMessage(TableModel table, UserModel user, string message)
        {
            if (OnGameTableUserMessageEvent != null)
                OnGameTableUserMessageEvent(table, user, message);
        }

        /// <summary>
        /// Wstaje od stołu
        /// </summary>
        /// <param name="table"></param>
        /// <param name="placeID"></param>
        public event Action<TableModel, PlayerModel> OnPlayerStandupEvent;
        public void OnGamePlayerStandup(TableModel table, PlayerModel player)
        {
            if (OnPlayerStandupEvent != null)
                OnPlayerStandupEvent(table, player);
        }

        /// <summary>
        /// Siada przy stole
        /// </summary>
        /// <param name="table"></param>
        /// <param name="player"></param>
        /// <param name="placeID"></param>
        public event Action<TableModel, PlayerModel> OnPlayerSitdownEvent;
        public void OnGamePlayerSitdown(TableModel table, PlayerModel player)
        {
            if (OnPlayerSitdownEvent != null)
                OnPlayerSitdownEvent(table, player);
        }

        /// <summary>
        /// Nowe rozdanie
        /// </summary>
        /// <param name="table"></param>
        public event Action<TableModel> OnTableClearEvent;
        public void OnGameTableClear(TableModel table)
        {
            if (OnTableClearEvent != null)
                OnTableClearEvent(table);
        }

        /// <summary>
        /// Gracz otrzymuje czas na podjęcie decyzji w grze
        /// </summary>
        /// <param name="table"></param>
        /// <param name="action"></param>
        public event Action<TableModel, BaseOfferAction> OnGameActionOfferEvent;
        public void OnGameActionOffer(TableModel table, BaseOfferAction action)
        {
            if (OnGameActionOfferEvent != null)
                OnGameActionOfferEvent(table, action);
        }

        /// <summary>
        /// Gracz stawia pieniądze na stół
        /// </summary>
        /// <param name="table"></param>
        /// <param name="player"></param>
        /// <param name="money"></param>
        public event Action<TableModel, BaseAction> OnPlayerActionTriggerEvent;
        public void OnGameActionTrigger(TableModel table, BaseAction action)
        {
            if (OnPlayerActionTriggerEvent != null)
                OnPlayerActionTriggerEvent(table, action);
        }

        /// <summary>
        /// Pobiera najnowsze informacje
        /// ilość użytkowników etc
        /// </summary>
        /// <param name="data"></param>
        public event Action<StatsModel> OnGetStatsInfoEvent;
        public void OnGetStatsInfo(StatsModel data)
        {
            if (OnGetStatsInfoEvent != null)
                OnGetStatsInfoEvent(data);
        }

        /// <summary>
        /// Zmieniła się informacja gracza
        /// gotówka, status
        /// </summary>
        /// <param name="table"></param>
        /// <param name="player"></param>
        public event Action<TableModel, PlayerModel> OnGamePlayerUpdateEvent;
        public void OnGamePlayerUpdate(TableModel table, PlayerModel player)
        {
            if (OnGamePlayerUpdateEvent != null)
                OnGamePlayerUpdateEvent(table, player);
        }

        /// <summary>
        /// Aktualizacja stolu do normal game
        /// wykonuje sie gdy doszedl gracz, obserwator, skonczylo sie rozdanie (np: avgpot)
        /// </summary>
        /// <param name="table"></param>
        public event Action<NormalGameModel> OnNormalModeUpdateEvent;
        public void OnNormalGameModelUpdate(NormalGameModel table)
        {
            if (OnNormalModeUpdateEvent != null)
                OnNormalModeUpdateEvent(table);
        }

        /// <summary>
        /// Aktualizacja turnieju, ilość graczy etc.
        /// </summary>
        /// <param name="table"></param>
        public event Action<ITournamentGameModel> OnTournamentGameModelUpdateEvent;
        public void OnTournamentGameModelUpdate(ITournamentGameModel game)
        {
            if (OnTournamentGameModelUpdateEvent != null)
                OnTournamentGameModelUpdateEvent(game);
        }

        /// <summary>
        /// Przeniesienie gracza do innego stołu
        /// </summary>
        public event Action<UserModel, TableModel, TableModel> OnGameTableUserMoveEvent;
        public void OnGameTableUserMove(UserModel user, TableModel table, TableModel newTable)
        {
            if (OnGameTableUserMoveEvent != null)
                OnGameTableUserMoveEvent(user, table, newTable);
        }

        /// <summary>
        /// Pobiera nową reklamę
        /// </summary>
        /// <param name="url"></param>
        /// <param name="imageUrl"></param>
        public event Action<AdvertisingModel> OnGetAdvertisingEvent;
        public void OnGetAdvertising(AdvertisingModel adv)
        {
            if (OnGetAdvertisingEvent != null)
                OnGetAdvertisingEvent(adv);
        }

        /// <summary>
        /// Otrzymenie historii rozdania
        /// </summary>
        public event Action<TableModel, ObservableCollection<BaseAction>> OnTableHitoryEvent;
        public void OnGameTableHitory(TableModel table, ObservableCollection<BaseAction> tableHistory)
        {
            if (OnTableHitoryEvent != null)
                OnTableHitoryEvent(table, tableHistory);
        }

        /// <summary>
        /// Wiadomośc systemowa
        /// </summary>
        public event Action<TableModel, TableMessageModel.Status, TableMessageModel> OnGameTableSystemMessageEvent;
        public void OnGameTableSystemMessage(TableModel table, TableMessageModel.Status status, TableMessageModel messageModel)
        {
            if (OnGameTableSystemMessageEvent != null)
                OnGameTableSystemMessageEvent(table, status, messageModel);
        }

        /// <summary>
        /// Nowy transfer pieniędzy
        /// </summary>
        public event Action<TransferModel> OnDepositInfoEvent;
        public void OnDepositInfo(TransferModel transfer)
        {
            if (OnDepositInfoEvent != null)
                OnDepositInfoEvent(transfer);
        }

        /// <summary>
        /// Otwiera okno z grą (np turnieje, zerwane połączenie itp)
        /// </summary>
        public event Action<TableModel> OnTableOpenWindowEvent;
        public void OnTableOpenWindow(TableModel table)
        {
            if (OnTableOpenWindowEvent != null)
                OnTableOpenWindowEvent(table);
        }


        public event Action<UserModel> OnUserAvatarChangedEvent;
        public void OnUserAvatarChanged(UserModel user)
        {
            if (OnUserAvatarChangedEvent != null)
                OnUserAvatarChangedEvent(user);
        }

        /// <summary>
        /// Wiadomość systemowa
        /// </summary>
        /// <param name="message"></param>
        public void OnMessage(string message)
        {
           Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
           {
               var activeWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
               if (activeWindow == null)
                   activeWindow = Application.Current.MainWindow;

               MessageBoxHelper.PrepToCenterMessageBoxOnForm(activeWindow);
               MessageBox.Show(activeWindow, message);
           }));
        }

        /// <summary>
        /// Aktializacja listy sotlow turnieju
        /// </summary>
        public event Action<ITournamentGameModel, List<TableModel>> OnTournamentTableListUpdateEvent;
        public void OnTournamentTableListUpdate(ITournamentGameModel table, List<TableModel> tableList)
        {
            if (OnTournamentTableListUpdateEvent != null)
                OnTournamentTableListUpdateEvent(table, tableList);
        }
    }
}
