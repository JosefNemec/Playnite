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
using Playnite.Models;
using System.Collections.ObjectModel;
using PlayniteUI.Windows;
using Playnite.Database;
using Playnite.Providers.Origin;
using PlayniteUI.Controls;
using System.Globalization;
using Playnite.Services;
using Playnite.Providers.Uplay;

namespace PlayniteUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowBase, INotifyPropertyChanged
    {
        private Settings config;
        public Settings Config
        {
            get
            {
                return config;
            }

            set
            {
                config = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Config"));
            }
        }

        private bool importSteamCatWizard = false;
        private ulong importSteamCatWizardId = 0;

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static object gamesLock = new object();
        private WindowPositionHandler positionManager;
        private GamesStats gamesStats;
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

        public GamesCollectionView GamesView
        {
            get; set;
        }

        public MainWindow()
        {
            Config = Settings.Instance;
            InitializeComponent();
            positionManager = new WindowPositionHandler(this, "Main", Config);
            Application.Current.MainWindow = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BringToForeground();
            NotificationsWin.AutoOpen = true;
            positionManager.RestoreSizeAndLocation();

            Config.PropertyChanged += Config_PropertyChanged;
            Config.FilterSettings.PropertyChanged += FilterSettings_PropertyChanged;

            gamesStats = new GamesStats(GameDatabase.Instance);
            MenuMainMenu.DataContext = this;
            MenuViewSettings.DataContext = Config;
            FilterSelector.DataContext = new FilterSelectorConfig(gamesStats, Config.FilterSettings);
            CheckFilterView.DataContext = Config.FilterSettings;
            GridGamesView.AppSettings = Config;

            if (!Config.FirstTimeWizardComplete)
            {
                var window = new FirstTimeStartupWindow()
                {
                    Owner = this
                };

                if (window.ShowDialog() == true)
                {
                    config.SteamSettings.IdSource = window.SteamImportLibByName ? SteamIdSource.Name : SteamIdSource.LocalUser;
                    Config.SteamSettings.AccountId = window.SteamIdLibImport;
                    Config.SteamSettings.IntegrationEnabled = window.SteamEnabled;
                    Config.SteamSettings.LibraryDownloadEnabled = window.SteamImportLibrary;
                    Config.SteamSettings.AccountName = window.SteamAccountName;
                    Config.GOGSettings.IntegrationEnabled = window.GOGEnabled;
                    Config.GOGSettings.LibraryDownloadEnabled = window.GogImportLibrary;
                    Config.OriginSettings.IntegrationEnabled = window.OriginEnabled;
                    Config.OriginSettings.LibraryDownloadEnabled = window.OriginImportLibrary;
                    Config.UplaySettings.IntegrationEnabled = window.UplayEnabled;
                    importSteamCatWizard = window.SteamImportCategories;
                    importSteamCatWizardId = window.SteamIdCategoryImport;

                    if (window.DatabaseLocation == FirstTimeStartupWindow.DbLocation.Custom)
                    {
                        Config.DatabasePath = window.DatabasePath;
                    }
                    else
                    {
                        FileSystem.CreateFolder(Paths.UserProgramDataPath);
                        Config.DatabasePath = System.IO.Path.Combine(Paths.UserProgramDataPath, "games.db");
                    }

                    GameDatabase.Instance.OpenDatabase(Config.DatabasePath);
                    AddInstalledGames(window.ImportedGames);

                    Config.FirstTimeWizardComplete = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Config"));
                }
            }            

            if (!Config.EmulatorWizardComplete)
            {
                GameDatabase.Instance.OpenDatabase(Config.DatabasePath);
                var window = new EmulatorImportWindow(DialogType.Wizard)
                {
                    Owner = this
                };

                if (window.ShowDialog() == true)
                {
                    Config.EmulatorWizardComplete = true;
                }
            }

            if (!Config.MigrationV2PcPlatformAdded)
            {
                GameDatabase.Instance.OpenDatabase(Config.DatabasePath);
                var db = GameDatabase.Instance;
                foreach (var game in db.GamesCollection.Find(a => a.Provider != Provider.Custom).ToList())
                {
                    db.AssignPcPlatform(game);
                    db.UpdateGameInDatabase(game);
                }

                Config.MigrationV2PcPlatformAdded = true;                
            }

            LoadGames(Config.UpdateLibStartup);
            CheckUpdate();
            SendUsageData();
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

        public void BringToForeground()
        {
            Topmost = true;
            Topmost = false;
        }

        private void PipeService_CommandExecuted(object sender, CommandExecutedEventArgs args)
        {
            logger.Info(@"Executing command ""{0}"" from pipe with arguments ""{1}""", args.Command, args.Args);

            switch (args.Command)
            {
                case CmdlineCommands.Focus:
                    Show();
                    WindowState = WindowState.Normal;
                    Activate();
                    BringToForeground();
                    break;

                case CmdlineCommands.Launch:
                    var game = GameDatabase.Instance.GamesCollection.FindById(int.Parse(args.Args));
                    if (game == null)
                    {
                        logger.Error("Cannot start game, game {0} not found.", args.Args);
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

        private void WindowMain_Closed(object sender, EventArgs e)
        {
            if (!Config.CloseToTray || !Config.EnableTray)
            {
                Application.Current.Shutdown(0);
            }
        }

        private void WindowMain_Closing(object sender, CancelEventArgs e)
        {
            if (Config.CloseToTray && Config.EnableTray)
            {
                Visibility = Visibility.Hidden;
                e.Cancel = true;
            }
        }

        private void WindowMain_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && Config.MinimizeToTray && Config.EnableTray)
            {
                Visibility = Visibility.Hidden;
            }
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
            // Convert to list to not enumerate on original DB list due to threading issues in LiteDB
            var games = database.GamesCollection.Find(a => a.Provider == provider && !a.IsProviderDataUpdated).ToList();

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

        private async void LoadGames(bool downloadLibUpdates)
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
                    PlayniteMessageBox.Show("Failed to open library database: " + exc.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                LiteDBImageToImageConverter.ClearCache();
                GamesView?.Dispose();
                GamesView = new GamesCollectionView(database, Config);
                BindingOperations.EnableCollectionSynchronization(GamesView.Items, gamesLock);

                try
                {
                    GamesEditor.Instance.UpdateJumpList();
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Failed to set update JumpList data: ");
                }
                
                ListGamesView.ItemsSource = GamesView.CollectionView;
                ImagesGamesView.ItemsSource = GamesView.CollectionView;
                GridGamesView.ItemsSource = GamesView.CollectionView;

                if (downloadLibUpdates)
                {
                    GamesLoaderHandler.CancelToken = new CancellationTokenSource();
                    GamesLoaderHandler.ProgressTask = Task.Factory.StartNew(() =>
                    {
                        ProgressControl.Visible = Visibility.Visible;
                        ProgressControl.ProgressValue = 0;
                        ProgressControl.Text = "Importing installed games...";

                        try
                        {
                            if (Config.UplaySettings.IntegrationEnabled)
                            {
                                database.UpdateInstalledGames(Provider.Uplay);
                                NotificationsWin.RemoveMessage(NotificationCodes.UplayInstalledImportError);
                            }
                        }
                        catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                        {
                            logger.Error(e, "Failed to import installed Uplay games.");
                            NotificationsWin.AddMessage(new NotificationMessage(NotificationCodes.UplayInstalledImportError, "Failed to import installed Uplay games:" + e.Message, NotificationType.Error, () =>
                            {

                            }));
                        }

                        try
                        {
                            if (Config.GOGSettings.IntegrationEnabled)
                            {
                                database.UpdateInstalledGames(Provider.GOG);
                                NotificationsWin.RemoveMessage(NotificationCodes.GOGLInstalledImportError);
                            }
                        }
                        catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                        {
                            logger.Error(e, "Failed to import installed GOG games.");
                            NotificationsWin.AddMessage(new NotificationMessage(NotificationCodes.GOGLInstalledImportError, "Failed to import installed GOG games:" + e.Message, NotificationType.Error, () =>
                            {

                            }));
                        }

                        try
                        {
                            if (Config.SteamSettings.IntegrationEnabled)
                            {
                                database.UpdateInstalledGames(Provider.Steam);
                                NotificationsWin.RemoveMessage(NotificationCodes.SteamInstalledImportError);
                            }
                        }
                        catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                        {
                            logger.Error(e, "Failed to import installed Steam games.");
                            NotificationsWin.AddMessage(new NotificationMessage(NotificationCodes.SteamInstalledImportError, "Failed to import installed Steam games: " + e.Message, NotificationType.Error, () =>
                            {

                            }));
                        }

                        try
                        {
                            if (Config.OriginSettings.IntegrationEnabled)
                            {
                                database.UpdateInstalledGames(Provider.Origin);
                                NotificationsWin.RemoveMessage(NotificationCodes.OriginInstalledImportError);
                            }
                        }
                        catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                        {
                            logger.Error(e, "Failed to import installed Origin games.");
                            NotificationsWin.AddMessage(new NotificationMessage(NotificationCodes.OriginInstalledImportError, "Failed to import installed Origin games: " + e.Message, NotificationType.Error, () =>
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
                        catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
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
                                if (config.SteamSettings.IdSource == SteamIdSource.Name)
                                {
                                    database.SteamUserName = Config.SteamSettings.AccountName;
                                }
                                else
                                {
                                    database.SteamUserName = Config.SteamSettings.AccountId.ToString();
                                }

                                database.UpdateOwnedGames(Provider.Steam);
                                NotificationsWin.RemoveMessage(NotificationCodes.SteamLibDownloadError);
                            }
                        }
                        catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                        {
                            logger.Error(e, "Failed to download Steam library updates.");
                            NotificationsWin.AddMessage(new NotificationMessage(NotificationCodes.SteamLibDownloadError, "Failed to download Steam library updates: " + e.Message, NotificationType.Error, () =>
                            {

                            }));
                        }

                        if (importSteamCatWizard && importSteamCatWizardId != 0)
                        {
                            ProgressControl.Text = "Importing Steam categories...";

                            try
                            {
                                var steamLib = new SteamLibrary();
                                GameDatabase.Instance.ImportCategories(steamLib.GetCategorizedGames(importSteamCatWizardId));
                            }
                            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                            {
                                logger.Error(e, "Failed to import Steam categories.");
                                NotificationsWin.AddMessage(new NotificationMessage(NotificationCodes.SteamLibDownloadError, "Failed to import Steam categories: " + e.Message, NotificationType.Error, () =>
                                {

                                }));
                            }
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
                        catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                        {
                            logger.Error(e, "Failed to download Origin library updates.");
                            NotificationsWin.AddMessage(new NotificationMessage(NotificationCodes.OriginLibDownloadError, "Failed to download Origin library updates: " + e.Message, NotificationType.Error, () =>
                            {

                            }));
                        }

                        ProgressControl.Text = "Downloading images and game details...";
                        ProgressControl.ProgressMin = 0;

                        var gamesCount = 0;
                        gamesCount = database.GamesCollection.Count(a => a.Provider != Provider.Custom && !a.IsProviderDataUpdated);
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
                        }),

                        // Uplay metada download thread
                        Task.Factory.StartNew(() =>
                        {
                            DownloadMetadata(database, Provider.Uplay, ProgressControl, GamesLoaderHandler.CancelToken.Token);
                        })
                        };

                        Task.WaitAll(tasks.ToArray());

                        ProgressControl.Text = "Library update finished";

                        Thread.Sleep(1500);
                        ProgressControl.Visible = Visibility.Collapsed;
                    });

                    await GamesLoaderHandler.ProgressTask;
                }
            }
            finally
            {
                GamesEditor.Instance.OnPropertyChanged("LastGames");
                GameAdditionAllowed = true;
            }
        }

        private void Config_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Language")
            {
                Localization.SetLanguage(Config.Language);
                return;
            }

            Config.SaveSettings();
        }

        private void FilterSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Active")
            {
                return;
            }

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
            LoadGames(true);
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

        private void AddEmulatedGames_Click(object sender, RoutedEventArgs e)
        {
            var window = new EmulatorImportWindow(DialogType.GameImport)
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
            {
                window.Model.AddSelectedGamesToDB();
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
                LoadGames(true);
            }
        }

        private void Platforms_Click(object sender, RoutedEventArgs e)
        {
            var window = new PlatformsWindow()
            {
                Owner = this
            };

            window.ConfigurePlatforms(GameDatabase.Instance);
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

        private void TrayPlaynite_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            BringToForeground();
        }

        private void MenuLastGamesGame_Click(object sender, RoutedEventArgs e)
        {
            var game = (sender as MenuItem).DataContext as IGame;
            GamesEditor.Instance.PlayGame(game);
        }

        private void CheckUpdate()
        {
            Task.Factory.StartNew(() =>
            {
                var update = new Update();
                UpdateWindow updateWindow = null;

                while (true)
                {
                    try
                    {
                        if ((updateWindow == null || !updateWindow.IsVisible) && update.IsUpdateAvailable)
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
                                    updateWindow = new UpdateWindow()
                                    {
                                        Owner = this
                                    };

                                    updateWindow.SetUpdate(update);
                                    updateWindow.Show();
                                    updateWindow.Focus();
                                });
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        logger.Error(exc, "Failed to process update.");
                    }

                    Thread.Sleep(4 * 60 * 60 * 1000);
                }
            });
        }

        private void SendUsageData()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var client = new ServicesClient();
                    client.PostUserUsage();
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Failed to post user usage data.");
                }
            });
        }

        private void ThirdPartyToolMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = e.OriginalSource as MenuItem;
                var tool = item.DataContext as ThirdPartyTool;
                tool.Start();
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Failed to start 3rd party tool.");
            }
        }

        private void ButtonFriends_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"steam://open/friends");
        }
    }
}
