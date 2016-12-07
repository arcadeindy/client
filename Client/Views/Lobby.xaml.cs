using CoinPoker.Controllers;
using CoinPoker.Controls;
using CoinPoker.Games;
using CoinPoker.Games.Poker;
using CoinPokerCommonLib;
using CoinPokerCommonLib.Models;
using CoinPokerCommonLib.Models.Game.TournamentOption;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CoinPoker.Views
{
    /// <summary>
    /// Interaction logic for Lobby.xaml
    /// </summary>
    public partial class Lobby : UserControl
    {
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        public ObservableCollection<NormalGameModel> NormalModelList { get; set; }
        public ObservableCollection<ITournamentGameModel> TournamentModelList { get; set; }

        public Lobby()
        {
            Console.WriteLine("Lobby constructor");

            this.Initialized += (obj, args) =>
            {
                Console.WriteLine("Lobby.Initialized");

                //Zaznaczam ostatnio aktywna zakladke
                LobbyTab.SelectedIndex = Properties.Settings.Default.LastLobbyTab;

                //Aktualizacja statystyk
                PokerClient.Instance.OnGetStatsInfoEvent += Instance_OnGetStatsInfoEvent;

                //Rekalma
                PokerClient.Instance.OnGetAdvertisingEvent += (adv) =>
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        SetAdvertisingLobby(adv);
                    }));
                };

                //Informacja o nowym transferze
                PokerClient.Instance.OnDepositInfoEvent += (transfer) =>
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        if (!Properties.Settings.Default.DontShowDepositWindow)
                        {
                            DepositInfoWindow deposit = new DepositInfoWindow(transfer);
                            deposit.ShowModal(MainWindow.Instance);
                        }
                    }));
                };

                PokerClient.Instance.OnNormalModeUpdateEvent += (game) =>
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        if (NormalModelList == null) return;
                        //Aktualizacja elementów listy
                        NormalGameModel item = NormalModelList.FirstOrDefault(t => t.Table.ID == game.Table.ID);

                        if (item != null)
                        {
                            NormalModelList[NormalModelList.IndexOf(item)] = game;
                            NormalGameModeList.Items.Refresh();
                        }
                    }));
                };

                PokerClient.Instance.OnTournamentGameModelUpdateEvent += (game) =>
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        if (NormalModelList == null) return;
                        //Aktualizacja elementów listy
                        ITournamentGameModel item = TournamentModelList.FirstOrDefault(t => t.TournamentModel.ID == game.TournamentModel.ID);

                        if (item != null)
                        {
                            TournamentModelList[TournamentModelList.IndexOf(item)] = game;
                            TournamentGameModeList.Items.Refresh();
                        }
                    }));
                };

                PokerClient.Instance.OnTableOpenWindowEvent += (table) =>
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {

                        if (Application.Current.Windows.OfType<PokerGameWindow>().Any(w => w.TableModel.ID == table.ID))
                        {
                            return;
                        }

                        PokerGameWindow gameWindow = new PokerGameWindow(table);
                        gameWindow.Show();
                    }));
                };

                Session.Data.Client.Connected += (e, o) =>
                {
                    Console.WriteLine("OnConnect");
                    RefreshNormalMode();
                    RefreshTournamentMode();
                };

                Session.Data.Client.Disconnected += (e, o) =>
                {
                    Console.WriteLine("OnDisconnect");
                };

                InitializeLobby();
            };

            InitializeComponent();
        }

        void Instance_OnGetStatsInfoEvent(StatsModel data)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                ServerTimeLbl.Text = data.ServerTime.ToShortTimeString();
                StatOnlineCount.Text = data.UsersOnline.ToString();
                StatTablesCount.Text = data.TablesToPlay.ToString();
            }));
        }

        private void InitializeLobby()
        {
            GetAdvertisement();
        }

        private void GetAdvertisement()
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    try
                    {
                        Session.Proxy().GetAdsLobby();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Błąd pobierania reklamy serwisu");
                    }
                }));
            });
        }

        public void SetAdvertisingLobby(AdvertisingModel adv)
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    AdsMiniRight.Tag = adv;

                    this.AdsLoader.Visibility = Visibility.Visible;

                    var image = new Image();
                    var fullFilePath = @adv.Image;

                    BitmapImage imgBitmap = new BitmapImage();
                    imgBitmap.BeginInit();
                    imgBitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
                    imgBitmap.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.Default);
                    imgBitmap.CacheOption = BitmapCacheOption.OnLoad;
                    imgBitmap.DownloadCompleted += (o, e) =>
                    {
                        ImageBrush b = new ImageBrush(imgBitmap);
                        b.AlignmentX = AlignmentX.Center;
                        b.AlignmentY = AlignmentY.Center;
                        b.Stretch = Stretch.UniformToFill;
                        this.AdsMiniRight.Background = b;
                        this.AdsLoader.Visibility = Visibility.Hidden;
                    };
                    imgBitmap.DownloadFailed += (o, e) =>
                    {
                        SetAdvertisingLobby(adv);
                    };
                    imgBitmap.EndInit();

                    //Z cache
                    if (imgBitmap.IsDownloading == false)
                    {
                        ImageBrush b = new ImageBrush(imgBitmap);
                        b.AlignmentX = AlignmentX.Center;
                        b.AlignmentY = AlignmentY.Center;
                        b.Stretch = Stretch.UniformToFill;
                        this.AdsMiniRight.Background = b;
                        this.AdsLoader.Visibility = Visibility.Hidden;
                    }
                }));
            });
        }

        private void RefreshNormalMode()
        {
            Console.WriteLine("RefreshNormalMode()");
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    try
                    {
                        if (Session.IsConnected())
                        {
                            NormalModelList = new ObservableCollection<NormalGameModel>(
                                Session.Proxy().GetNormalModeList()
                            );
                            NormalGameModeList.ItemsSource = NormalModelList;
                            NormalGameModeList.Focus();
                            NormalGameModeList.SelectedIndex = 0;

                            NormalFilterChange();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Błąd pobierania listy stołów");
                    }
                }));
            });
        }

        private void RefreshTournamentMode()
        {
            Console.WriteLine("RefreshTournamentMode()");
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    try
                    {
                        if (Session.IsConnected())
                        {
                            TournamentModelList = new ObservableCollection<ITournamentGameModel>(
                                Session.Proxy().GetTournamentModeList()
                            );
                            TournamentGameModeList.ItemsSource = TournamentModelList;
                            TournamentGameModeList.Focus();
                            TournamentGameModeList.SelectedIndex = 0;

                            TournamentFilterChange();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Błąd pobierania listy stołów");
                    }
                }));
            });
        }

        /// <summary>
        /// Kliknięcie kolumny headera - sortowanie
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridViewColumnSort(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                NormalGameModeList.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);

            var item = ((FrameworkElement)e.OriginalSource).DataContext;

            switch (LobbyTab.SelectedIndex)
            {
                case 0:
                    NormalGameModeList.Items.SortDescriptions.Clear();
                    NormalGameModeList.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
                    break;
                case 1:
                    TournamentGameModeList.Items.SortDescriptions.Clear();
                    TournamentGameModeList.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
                    break;
            }
        }

        public static void OpenTournamentWindow(ITournamentGameModel game)
        {
            /**
             * Sprawdz czy stol uruchomiony
             * jesli tak podswietl go jesli nie - utworz nowe okno stolu
             * */
            if (Application.Current.Windows.OfType<TournamentWindow>().Any(t => t.GameModel.TournamentModel.ID == game.TournamentModel.ID))
            {
                return;
            }

            TournamentWindow gameWindow = new TournamentWindow(game);
            gameWindow.Owner = MainWindow.Instance;
            gameWindow.Show();
            gameWindow.Owner = null;
        }

        public static void OpenGameWindow(NormalGameModel game)
        {
            /**
             * Sprawdz czy stol uruchomiony
             * jesli tak podswietl go jesli nie - utworz nowe okno stolu
             * */
            if (Application.Current.Windows.OfType<PokerGameWindow>().Any(w => w.TableModel.ID == game.Table.ID))
            {
                return;
            }

            PokerGameWindow gameWindow = new PokerGameWindow(game.Table);
            //gameWindow.Owner = MainWindow.Instance;
            gameWindow.Show();
        }

        /// <summary>
        /// Wchodzi na podany kliknięty stół
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NormalDoubleClick(object sender, RoutedEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext;

            if (item is NormalGameModel)
            {
                OpenGameWindow(item as NormalGameModel);
            }
        }

        /// <summary>
        /// Otwiera okno turniejowe
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TournamentDoubleClick(object sender, RoutedEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext;

            if (item is ITournamentGameModel)
            {
                OpenTournamentWindow(item as ITournamentGameModel);
            }
        }

        private void NormalGameModeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = NormalGameModeList.SelectedItem as NormalGameModel;

            if (item != null && item is NormalGameModel)
            {
                TableSideInformationControl.SetNormalModeNote(NormalGameModeList.SelectedItem as NormalGameModel);
            }
        }

        private void TournamentGameModeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = TournamentGameModeList.SelectedItem as ITournamentGameModel;

            if (item != null && item is ITournamentGameModel)
            {
                TableSideInformationControl.SetTournamentModeNote(TournamentGameModeList.SelectedItem as ITournamentGameModel);
            }
        }

        private void TournamentFilterChange()
        {
            //Ustawienia filtrowania
            if (TournamentGameModeList == null || TournamentGameModeList.ItemsSource == null) return;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(TournamentGameModeList.ItemsSource);
            view.Filter = (object item) =>
            {
                var model = (ITournamentGameModel)item;

                bool GlobalFilter = true;
                bool GlobalFilterQuery = true;

                // FILTROWANE //
                var IsSearch = (bool)(this.TournamentFilter_SearchQuery.Text != "");
                var SearchQuery = (model.TournamentModel.Name.IndexOf(this.TournamentFilter_SearchQuery.Text, StringComparison.OrdinalIgnoreCase) >= 0);

                if (IsSearch)
                    GlobalFilterQuery &= SearchQuery;

                var IsFull = (bool)TournamentFilter_ShowOnlyIncomming.IsChecked;
                var FullQuery = (model.TournamentModel.State == Enums.TournamentState.Announced || model.TournamentModel.State == Enums.TournamentState.Registration);

                if (IsFull)
                    GlobalFilterQuery &= FullQuery;

                return GlobalFilterQuery;
            };
        }

        private void NormalFilterChange()
        {
            //Ustawienia filtrowania
            if (NormalGameModeList == null || NormalGameModeList.ItemsSource == null) return;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(NormalGameModeList.ItemsSource);
            view.Filter = (object item) =>
            {
                var model = (NormalGameModel)item;

                bool GlobalFilter = true;
                bool GlobalFilterQuery = true;

                // FILTROWANE //
                var IsSearch = (bool)(this.NormalFilter_SearchQuery.Text != "");
                var SearchQuery = (model.Name.IndexOf(this.NormalFilter_SearchQuery.Text, StringComparison.OrdinalIgnoreCase) >= 0);

                if (IsSearch)
                    GlobalFilterQuery &= SearchQuery;

                var IsFull = (bool)NormalFilter_HideFull.IsChecked;
                var FullQuery = (model.Table.Players < model.Table.Seats);

                if (IsFull)
                    GlobalFilterQuery &= FullQuery;

                var IsEmpty = (bool)NormalFilter_HideEmpty.IsChecked;
                var EmptyQuery = (model.Table.Players != 0);

                if (IsEmpty)
                    GlobalFilterQuery &= EmptyQuery;

                //Stawki
                switch (NormalFilter_Stakes.SelectedIndex)
                {
                    case 1: //mikro 2k i mniej
                        GlobalFilterQuery &= model.Maximum < 2000m;
                        break;
                    case 2: //niskie
                        GlobalFilterQuery &= model.Minimum < 5000m && model.Minimum >= 2000m;
                        break;
                    case 3: //srednie
                        GlobalFilterQuery &= model.Minimum < 10000m && model.Minimum >= 5000m;
                        break;
                    case 4: //wysokie
                        GlobalFilterQuery &= model.Minimum >= 10000m;
                        break;
                    default:
                    case 0:
                        GlobalFilter &= true;
                        break;
                }

                //Ilość graczy
                switch (NormalFilter_MaxPlrs.SelectedIndex)
                {
                    case 1: //7-10
                        GlobalFilterQuery &= model.Table.Seats >= 7 && model.Table.Seats < 10;
                        break;
                    case 2: //6
                        GlobalFilterQuery &= model.Table.Seats == 6;
                        break;
                    case 3: //2
                        GlobalFilterQuery &= model.Table.Seats == 2;
                        break;
                    default:
                    case 0:
                        GlobalFilter &= true;
                        break;
                }

                //Limity
                if (NormalFilter_Limits.SelectedIndex != 0)
                {
                    GlobalFilterQuery &= model.Table.Limit == (Enums.LimitType)NormalFilter_Limits.SelectedIndex - 1;
                }

                //Typ gry
                switch (NormalFilter_Type.SelectedIndex)
                {
                    case 1: //holdem
                        GlobalFilterQuery &= model.Table.Game == Enums.PokerGameType.Holdem;
                        break;
                    case 2: //omaha
                        GlobalFilterQuery &= model.Table.Game == Enums.PokerGameType.Omaha;
                        break;
                    default:
                    case 0:
                        GlobalFilter &= true;
                        break;
                }

                return GlobalFilterQuery;
            };
        }

        private void NormalTemplateStandard_Checked(object sender, RoutedEventArgs e)
        {
            //NormalTemplateGroupButton.IsChecked = false;
        }

        private void NormalTemplateGroup_Checked(object sender, RoutedEventArgs e)
        {
            //NormalTemplateStandardButton.IsChecked = false;
        }

        private void ExtendButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ResizeFilterPanels_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void RemoveFilters_Click(object sender, RoutedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(NormalGameModeList.ItemsSource);
            view.Filter = (object item) =>
            {
                return true;
            };

            //Domyślne przyciski
            NormalFilter_Type.SelectedIndex = 0;
            NormalFilter_Stakes.SelectedIndex = 0;
            NormalFilter_MaxPlrs.SelectedIndex = 0;
            NormalFilter_Limits.SelectedIndex = 0;
            NormalFilter_HideFull.IsChecked = false;
            NormalFilter_HideEmpty.IsChecked = false;
            NormalFilter_SearchQuery.Text = "";

            NormalFilterChange();
        }

        private void RemoveFiltersTournament_Click(object sender, RoutedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(TournamentGameModeList.ItemsSource);
            view.Filter = (object item) =>
            {
                return true;
            };

            //Domyślne przyciski
            TournamentFilter_ShowOnlyIncomming.IsChecked = false;
            TournamentFilter_SearchQuery.Text = "";

            TournamentFilterChange();
        }

        private void LobbyTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                Properties.Settings.Default.LastLobbyTab = LobbyTab.SelectedIndex;
                Properties.Settings.Default.Save();

                switch (LobbyTab.SelectedIndex)
                {
                    case 0:
                        RefreshNormalMode();
                        break;
                    case 1:
                        RefreshTournamentMode();
                        break;
                }
            }
        }

        private void NormalFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NormalFilterChange();
        }

        private void NormalFilter_Checked(object sender, RoutedEventArgs e)
        {
            NormalFilterChange();
        }

        private void NormalFilter_KeyDown(object sender, KeyEventArgs e)
        {
            NormalFilterChange();
        }

        private void TournamentFilter_Checked(object sender, RoutedEventArgs e)
        {
            TournamentFilterChange();
        }

        private void TournamentFilter_KeyDown(object sender, KeyEventArgs e)
        {
            TournamentFilterChange();
        }

        private void AdsMiniRight_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (AdsMiniRight.Tag == null) return;
            var adModel = AdsMiniRight.Tag as AdvertisingModel;

            System.Diagnostics.Process.Start(adModel.Url);
        }

    }
}
