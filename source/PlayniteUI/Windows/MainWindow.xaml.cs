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
using System.IO;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Threading;
using NLog;
using Playnite;
using Playnite.Providers.GOG;
using Playnite.Providers.Steam;
using Playnite.Providers.Custom;
using Playnite.Models;
using System.Collections.ObjectModel;
using PlayniteUI.Windows;
using Playnite.Database;
using Playnite.Providers.Origin;
using PlayniteUI.Controls;

namespace PlayniteUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public Settings Config
        {
            get; set;
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static object gamesLock = new object();
        private WindowPositionHandler positionManager;
        private GamesStats gamesStats = new GamesStats();
        public NotificationsWindow NotificationsWin = new NotificationsWindow();

        private PipeService pipeService;
        private PipeServer pipeServer;

        public event PropertyChangedEventHandler PropertyChanged;

        private bool gameAdditionAllowed = true;
        public bool GameAdditionAllowed
        {
            get
            {
                return gameAdditionAllowed;
            }
            set
            {
                gameAdditionAllowed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GameAdditionAllowed"));
            }
        }

        public string FilterText
        {
            get
            {
                return SearchBoxFilter.Text;
            }

            set
            {
                if (MainCollectionView == null)
                {
                    return;
                }

                MainCollectionView.Refresh();
            }
        }

        public ListCollectionView MainCollectionView
        {
            get; set;
        }

        public MainWindow()
        {
            InitializeComponent();
            positionManager = new WindowPositionHandler(this, "Main");
            Application.Current.MainWindow = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Config = Settings.Instance;
            NotificationsWin.AutoOpen = true;

            positionManager.RestoreSizeAndLocation(Config);

            GogSettings.DefaultIcon = @"/Images/gogicon.png";
            GogSettings.DefaultImage = @"/Images/custom_cover_background.png";
            SteamSettings.DefaultIcon = @"/Images/steamicon.png";
            SteamSettings.DefaultImage = @"/Images/custom_cover_background.png";
            OriginSettings.DefaultIcon = @"/Images/originicon.png";
            OriginSettings.DefaultImage = @"/Images/custom_cover_background.png";
            CustomGameSettings.DefaultIcon = @"/Images/applogo.png";
            CustomGameSettings.DefaultImage = @"/Images/custom_cover_background.png";
            CustomGameSettings.DefaultBackgroundImage = @"/Images/default_background.png";

            Config.PropertyChanged += Config_PropertyChanged;
            Config.FilterSettings.PropertyChanged += FilterSettings_PropertyChanged;

            MenuMainMenu.DataContext = this;
            MenuViewSettings.DataContext = Config;
            FilterSelector.DataContext = new Controls.FilterSelectorConfig(gamesStats, Config.FilterSettings);
            CheckFilterView.DataContext = Config.FilterSettings;
            GridGamesView.HeaderMenu.DataContext = Config;
            
            if (!Config.FirstTimeWizardComplete)
            {
                var window = new FirstTimeStartupWindow()
                {
                    Owner = this
                };

                if (window.ShowDialog() == true)
                {
                    Config.SteamSettings.IntegrationEnabled = window.SteamEnabled;
                    Config.SteamSettings.LibraryDownloadEnabled = window.SteamImportLibrary;
                    Config.SteamSettings.AccountName = window.SteamAccountName;
                    Config.GOGSettings.IntegrationEnabled = window.GOGEnabled;
                    Config.GOGSettings.LibraryDownloadEnabled = window.GogImportLibrary;
                    Config.OriginSettings.IntegrationEnabled = window.OriginEnabled;
                    Config.OriginSettings.LibraryDownloadEnabled = window.OriginImportLibrary;

                    if (window.DatabaseLocation == FirstTimeStartupWindow.DbLocation.Custom)
                    {
                        Config.DatabasePath = window.DatabasePath;
                    }
                    else
                    {
                        FileSystem.CreateFolder(Paths.UserProgramDataPath);
                        Config.DatabasePath = System.IO.Path.Combine(Paths.UserProgramDataPath, "games.db");
                    }

                    GameDatabase.Instance.OpenDatabase(Config.DatabasePath, true);
                    AddInstalledGames(window.ImportedGames);

                    Config.FirstTimeWizardComplete = true;
                }
            }

            LoadGames();

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                var update = new Update();

                try
                {
                    if (update.IsUpdateAvailable)
                    {
                        update.DownloadUpdate();

                        try
                        {
                            update.DownloadReleaseNotes();
                        }
                        catch (Exception exc)
                        {
                            logger.Warn(exc, "Failed to download release notes.");
                        }

                        Dispatcher.Invoke(() =>
                        {
                            var window = new UpdateWindow()
                            {
                                Owner = this
                            };

                            window.SetUpdate(update);
                            window.Show();
                            window.Focus();
                        });
                    }
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Failed to process update.");
                }
            });

            Focus();

            pipeService = new PipeService();
            pipeService.CommandExecuted += PipeService_CommandExecuted;
            pipeServer = new PipeServer(Settings.GetAppConfigValue("PipeEndpoint"));
            pipeServer.StartServer(pipeService);

            var args = Environment.GetCommandLineArgs();
            if (args.Count() > 0 && args.Contains("-command"))
            {
                var commandArgs = args[2].Split(new char[] { ':' });
                var command = commandArgs[0];
                var cmdArgs = commandArgs.Count() > 1 ? commandArgs[1] : string.Empty;
                PipeService_CommandExecuted(this, new CommandExecutedEventArgs(command, cmdArgs));
            }
        }

        private void PipeService_CommandExecuted(object sender, CommandExecutedEventArgs args)
        {
            logger.Info(@"Executing command ""{0}"" from pipe with arguments ""{1}""", args.Command, args.Args);

            switch (args.Command)
            {
                case CmdlineCommands.Focus:
                    SystemCommands.RestoreWindow(this);
                    Activate();
                    Focus();
                    break;

                case CmdlineCommands.Launch:
                    var game = GameDatabase.Instance.Games.FirstOrDefault(a => a.Id == int.Parse(args.Args));
                    if (game == null)
                    {
                        logger.Error("Cannot start game, game {0} not found.", args.Command);
                    }
                    else
                    {
                        GamesEditor.Instance.PlayGame(game);
                    }

                    break;

                default:
                    logger.Warn("Unknown command received");
                    break;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        private void AddInstalledGames(List<InstalledGameMetadata> games)
        {
            if (games == null)
            {
                return;
            }

            foreach (var game in games)
            {
                if (game.Icon != null)
                {
                    var iconId = "images/custom/" + game.Icon.Name;
                    GameDatabase.Instance.AddImage(iconId, game.Icon.Name, game.Icon.Data);
                    game.Game.Icon = iconId;
                }

                GameDatabase.Instance.AddGame(game.Game);
            }
        }

        private void DownloadMetadata(GameDatabase database, Provider provider, ProgressControl progresser, CancellationToken token)
        {
            var games = database.Games.Where(a => a.Provider == provider && !a.IsProviderDataUpdated).ToList();

            foreach (var game in games)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                ProgressControl.ProgressValue++;

                try
                {
                    database.UpdateGameWithMetadata(game);
                }
                catch (Exception e)
                {
                    logger.Error(e, string.Format("Failed to download metadata for id:{0}, provider:{1}.", game.ProviderId, game.Provider));
                }
            }
        }

        private async void LoadGames()
        {
            if (GamesLoaderHandler.ProgressTask != null && GamesLoaderHandler.ProgressTask.Status == TaskStatus.Running)
            {
                GamesLoaderHandler.CancelToken.Cancel();
                await GamesLoaderHandler.ProgressTask;
            }

            GameAdditionAllowed = false;

            try
            {
                if (string.IsNullOrEmpty(Config.DatabasePath))
                {
                    return;
                }

                var database = GameDatabase.Instance;

                try
                {
                    database.OpenDatabase(Config.DatabasePath);
                }
                catch (Exception exc)
                {
                    GameAdditionAllowed = false;
                    MessageBox.Show("Failed to open library database: " + exc.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                BindingOperations.EnableCollectionSynchronization(database.Games, gamesLock);
                database.LoadGamesFromDb(Config);
                ListGamesView.ItemsSource = database.Games;
                ImagesGamesView.ItemsSource = database.Games;
                GridGamesView.ItemsSource = database.Games;
                MainCollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(database.Games);

                Config_PropertyChanged(this, null);

                GamesLoaderHandler.CancelToken = new CancellationTokenSource();
                GamesLoaderHandler.ProgressTask = Task.Factory.StartNew(() =>
                {
                    ProgressControl.Visible = Visibility.Visible;
                    ProgressControl.ProgressValue = 0;
                    ProgressControl.Text = "Importing installed games...";

                    try
                    {
                        if (Config.GOGSettings.IntegrationEnabled)
                        {
                            database.UpdateInstalledGames(Provider.GOG);
                            NotificationsWin.RemoveMessage(NotificationCodes.GOGLInstalledImportError);

                            if (!Config.GOGSettings.LibraryDownloadEnabled)
                            {
                                database.UnloadNotInstalledGames(Provider.GOG);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Failed to import installed GOG games.");
                        NotificationsWin.AddMessage(new NotificationMessage(NotificationCodes.GOGLInstalledImportError, e.Message, NotificationType.Error, () =>
                        {

                        }));
                    }

                    try
                    {
                        if (Config.SteamSettings.IntegrationEnabled)
                        {
                            database.UpdateInstalledGames(Provider.Steam);
                            NotificationsWin.RemoveMessage(NotificationCodes.SteamInstalledImportError);

                            if (!Config.SteamSettings.LibraryDownloadEnabled)
                            {
                                database.UnloadNotInstalledGames(Provider.Steam);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Failed to import installed Steam games.");
                        NotificationsWin.AddMessage(new NotificationMessage(NotificationCodes.SteamInstalledImportError, e.Message, NotificationType.Error, () =>
                        {

                        }));
                    }

                    try
                    {
                        if (Config.OriginSettings.IntegrationEnabled)
                        {
                            database.UpdateInstalledGames(Provider.Origin);
                            NotificationsWin.RemoveMessage(NotificationCodes.OriginInstalledImportError);

                            if (!Config.OriginSettings.LibraryDownloadEnabled)
                            {
                                database.UnloadNotInstalledGames(Provider.Origin);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Failed to import installed Origin games.");
                        NotificationsWin.AddMessage(new NotificationMessage(NotificationCodes.OriginInstalledImportError, e.Message, NotificationType.Error, () =>
                        {

                        }));
                    }

                    ProgressControl.Text = "Downloading GOG library updates...";

                    try
                    {
                        if (Config.GOGSettings.IntegrationEnabled && Config.GOGSettings.LibraryDownloadEnabled)
                        {
                            database.UpdateOwnedGames(Provider.GOG);
                            NotificationsWin.RemoveMessage(NotificationCodes.GOGLibDownloadError);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Failed to download GOG library updates.");
                        NotificationsWin.AddMessage(new NotificationMessage(NotificationCodes.GOGLibDownloadError, "Failed to download GOG library updates: " + e.Message, NotificationType.Error, () =>
                        {

                        }));
                    }

                    ProgressControl.Text = "Downloading Steam library updates...";

                    try
                    {
                        if (Config.SteamSettings.IntegrationEnabled && Config.SteamSettings.LibraryDownloadEnabled)
                        {
                            database.SteamUserName = Config.SteamSettings.AccountName;
                            database.UpdateOwnedGames(Provider.Steam);
                            NotificationsWin.RemoveMessage(NotificationCodes.SteamLibDownloadError);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Failed to download Steam library updates.");
                        NotificationsWin.AddMessage(new NotificationMessage(NotificationCodes.SteamLibDownloadError, "Failed to download Steam library updates: " + e.Message, NotificationType.Error, () =>
                        {

                        }));
                    }

                    ProgressControl.Text = "Downloading Origin library updates...";

                    try
                    {
                        if (Config.OriginSettings.IntegrationEnabled && Config.OriginSettings.LibraryDownloadEnabled)
                        {
                            database.UpdateOwnedGames(Provider.Origin);
                            NotificationsWin.RemoveMessage(NotificationCodes.OriginLibDownloadError);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Failed to download Origin library updates.");
                        NotificationsWin.AddMessage(new NotificationMessage(NotificationCodes.OriginLibDownloadError, "Failed to download Origin library updates: " + e.Message, NotificationType.Error, () =>
                        {

                        }));
                    }

                    gamesStats.SetGames(database.Games);
                    ProgressControl.Text = "Downloading images and game details...";
                    ProgressControl.ProgressMin = 0;
                    
                    var gamesCount = 0;
                    gamesCount = database.Games.Where(a => a.Provider != Provider.Custom && !a.IsProviderDataUpdated).Count();
                    if (gamesCount > 0)
                    {
                        gamesCount -= 1;
                    }

                    ProgressControl.ProgressMax = gamesCount;

                    var tasks = new List<Task>
                    {
                        // Steam metada download thread
                        Task.Factory.StartNew(() =>
                        {
                            DownloadMetadata(database, Provider.Steam, ProgressControl, GamesLoaderHandler.CancelToken.Token);
                        }),


                        // Origin metada download thread
                        Task.Factory.StartNew(() =>
                        {
                            DownloadMetadata(database, Provider.Origin, ProgressControl, GamesLoaderHandler.CancelToken.Token);
                        }),


                        // GOG metada download thread
                        Task.Factory.StartNew(() =>
                        {
                            DownloadMetadata(database, Provider.GOG, ProgressControl, GamesLoaderHandler.CancelToken.Token);
                        })
                    };

                    Task.WaitAll(tasks.ToArray());

                    ProgressControl.Text = "Library update finished";
                    
                    Thread.Sleep(1500);
                    ProgressControl.Visible = Visibility.Collapsed;
                });

                await GamesLoaderHandler.ProgressTask;
            }
            finally
            {
                GameAdditionAllowed = true;
            }
        }

        private bool GamesFilter(object item)
        {
            var game = (IGame)item;

            // ------------------ Installed
            bool installedResult = false;
            if (Config.FilterSettings.Installed && game.IsInstalled)
            {
                installedResult = true;
            }
            else if (!Config.FilterSettings.Installed)
            {
                installedResult = true;
            }

            // ------------------ Hidden
            bool hiddenResult = true;
            if (Config.FilterSettings.Hidden && game.Hidden)
            {
                hiddenResult = true;
            }
            else if (!Config.FilterSettings.Hidden && game.Hidden)
            {
                hiddenResult = false;
            }
            else if (Config.FilterSettings.Hidden && !game.Hidden)
            {
                hiddenResult = false;
            }

            // ------------------ Providers
            bool providersFilter = false;
            if (Config.FilterSettings.Providers.All(a => a.Value == false))
            {
                providersFilter = true;
            }
            else
            {
                if (Config.FilterSettings.Providers[game.Provider] == true)
                {
                    providersFilter = true;
                }
                else
                {
                    providersFilter = false;
                }
            }

            // ------------------ Name filter
            bool textResult;
            if (string.IsNullOrEmpty(SearchBoxFilter.Text))
            {
                textResult = true;
            }
            else
            {
                textResult = (game.Name.IndexOf(SearchBoxFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            return installedResult && hiddenResult && textResult && providersFilter;
        }

        private void Config_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e != null && !(new string[] { "SortingOrder", "GroupingOrder", "GamesViewType" }).Contains(e.PropertyName))
            {
                return;
            }

            if (e != null && e.PropertyName == "GamesViewType")
            {
                //TabControlView.SelectedIndex = (int)Config.GamesViewType;
                return;
            }

            if (GameDatabase.Instance.Games == null)
            {
                return;
            }
            
            using (MainCollectionView.DeferRefresh())
            {

                if (e == null)
                {
                    logger.Debug("Doing complete view refresh.");
                    if (Config.SortingOrder == SortOrder.Activity)
                    {
                        MainCollectionView.SortDescriptions.Add(new SortDescription("LastActivity", ListSortDirection.Descending));
                    }

                    MainCollectionView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

                    if (Config.GroupingOrder == GroupOrder.Store)
                    {
                        MainCollectionView.GroupDescriptions.Clear();
                        MainCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("Provider"));
                        MainCollectionView.SortDescriptions.Insert(0, new SortDescription("Provider", ListSortDirection.Ascending));
                    }
                    else if (Config.GroupingOrder == GroupOrder.Category)
                    {
                        MainCollectionView.GroupDescriptions.Clear();
                        MainCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("Categories"));
                    }

                    if (MainCollectionView.LiveGroupingProperties.Count > 0)
                    {
                        MainCollectionView.LiveGroupingProperties.Clear();
                    }

                    MainCollectionView.LiveGroupingProperties.Add("Categories");

                    if (MainCollectionView.LiveFilteringProperties.Count > 0)
                    {
                        MainCollectionView.LiveFilteringProperties.Clear();
                    }

                    MainCollectionView.LiveFilteringProperties.Add("Hidden");
                    MainCollectionView.LiveFilteringProperties.Add("Provider");
                    MainCollectionView.Filter = GamesFilter;

                    //TabControlView.SelectedIndex = (int)Config.GamesViewType;
                }
                else
                {
                    logger.Debug("Doing refresh of listview - " + e.PropertyName);
                    if (e.PropertyName == "SortingOrder")
                    {
                        MainCollectionView.SortDescriptions.Clear();

                        switch (Config.SortingOrder)
                        {
                            case SortOrder.Activity:
                                MainCollectionView.SortDescriptions.Add(new SortDescription("LastActivity", ListSortDirection.Descending));
                                MainCollectionView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                                break;

                            case SortOrder.Name:
                                MainCollectionView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                                break;
                        }
                    }

                    if (e.PropertyName == "GroupingOrder")
                    {
                        MainCollectionView.GroupDescriptions.Clear();
                        var sortItem = MainCollectionView.SortDescriptions.First();
                        if (sortItem.PropertyName == "Provider" || sortItem.PropertyName == "Categories")
                        {
                            MainCollectionView.SortDescriptions.Remove(sortItem);
                        }

                        switch (Config.GroupingOrder)
                        {
                            case GroupOrder.None:
                                break;

                            case GroupOrder.Store:
                                MainCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("Provider"));
                                MainCollectionView.SortDescriptions.Insert(0, new SortDescription("Provider", ListSortDirection.Ascending));
                                break;

                            case GroupOrder.Category:
                                MainCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("Categories"));
                                break;
                        }
                    }
                }

                MainCollectionView.IsLiveSorting = true;
                MainCollectionView.IsLiveFiltering = true;
                MainCollectionView.IsLiveGrouping = true;
            }            
        }

        private void FilterSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (MainCollectionView == null)
            {
                return;
            }

            MainCollectionView.Refresh();
            Config.SaveSettings();
        }

        private void ImageLogo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MenuMainMenu.PlacementTarget = (UIElement)sender;
            MenuMainMenu.IsOpen = true;
        }

        private void ViewConfigElement_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MenuViewSettings.PlacementTarget = (UIElement)sender;
            MenuViewSettings.IsOpen = true;
        }

        private void ListViewSelection_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Config.GamesViewType = ViewType.List;
        }

        private void ImagesViewSelection_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Config.GamesViewType = ViewType.Images;
        }

        private void GridViewSelection_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Config.GamesViewType = ViewType.Grid;
        }

        private void ReloadGames_Click(object sender, RoutedEventArgs e)
        {
            LoadGames();
            MenuMainMenu.IsOpen = false;
        }

        private void AddNewGame_Click(object sender, RoutedEventArgs e)
        {
            var newGame = new Game()
            {
                Name = "New Game",
                Provider = Provider.Custom
            };

            GameDatabase.Instance.AddGame(newGame);
            MenuMainMenu.IsOpen = false;

            if (GamesEditor.Instance.EditGame(newGame) == true)
            {
                GameDatabase.Instance.UpdateGameInDatabase(newGame);
                switch (Settings.Instance.GamesViewType)
                {
                    case ViewType.List:
                        ListGamesView.ListGames.SelectedItem = newGame;
                        ListGamesView.ListGames.ScrollIntoView(newGame);
                        break;

                    case ViewType.Grid:
                        GridGamesView.GridGames.SelectedItem = newGame;
                        GridGamesView.GridGames.ScrollIntoView(newGame);
                        break;
                }
            }
            else
            {
                GameDatabase.Instance.DeleteGame(newGame);
            }
        }

        private void AddInstalledGames_Click(object sender, RoutedEventArgs e)
        {
            var window = new InstalledGamesWindow()
            {
                Owner = this
            };

            window.ShowDialog();

            if (window.DialogResult == true)
            {
                AddInstalledGames(window.Games);
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var configWindow = new SettingsWindow()
            {
                DataContext = Config,
                Owner = this
            };

            configWindow.ShowDialog();

            if (configWindow.ProviderIntegrationChanged || configWindow.DatabaseLocationChanged)
            {
                LoadGames();
            }
        }

        private void Exitappp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow()
            {
                Owner = this
            };

            aboutWindow.ShowDialog();
        }

        private void Issue_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://github.com/JosefNemec/Playnite/issues/new");
            MenuMainMenu.IsOpen = false;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                positionManager.SavePosition(Config);
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsLoaded)
            {
                positionManager.SaveSize(Config);
            }
        }
    }
}
