using Playnite.API;
using Playnite.Common;
using Playnite.Database;
using Playnite.Metadata;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.Settings;
using Playnite.Commands;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Playnite.ViewModels;
using Playnite.DesktopApp.Windows;
using System.Windows.Controls;
using Playnite.SDK.Exceptions;
using Playnite.Common.Media.Icons;
using Playnite.DesktopApp.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing.Imaging;
using Playnite.DesktopApp.Controls;
using System.Diagnostics;
using Playnite.SDK.Events;
using Playnite.Emulators;

namespace Playnite.DesktopApp.ViewModels
{
    public partial class DesktopAppViewModel : MainViewModelBase, IDisposable
    {
        public static ILogger Logger = LogManager.GetLogger();
        private static object gamesLock = new object();
        protected bool ignoreCloseActions = false;
        protected bool ignoreSelectionChanges = false;
        private readonly SynchronizationContext context;
        private PlayniteApplication application;
        private Controls.LibraryStatistics statsView;
        private Controls.Views.Library libraryView;

        public PlayniteAPI PlayniteApi { get; set;  }
        public ExtensionFactory Extensions { get; set; }
        public IWindowFactory Window { get; }
        public IDialogsFactory Dialogs { get; }
        public IResourceProvider Resources { get; }
        public GameDatabase Database { get; set; }
        public DesktopGamesEditor GamesEditor { get; }

        private Control activeView;
        public Control ActiveView
        {
            get => activeView;
            set
            {
                activeView = value;
                OnPropertyChanged();
            }
        }

        private StatisticsViewModel libraryStats;
        public StatisticsViewModel LibraryStats
        {
            get => libraryStats;
            set
            {
                libraryStats = value;
                OnPropertyChanged();
            }
        }

        private GameDetailsViewModel selectedGameDetails;
        public GameDetailsViewModel SelectedGameDetails
        {
            get => selectedGameDetails;
            set
            {
                selectedGameDetails = value;
                OnPropertyChanged();
            }
        }

        private GamesCollectionViewEntry selectedGame;
        public new GamesCollectionViewEntry SelectedGame
        {
            get => selectedGame;
            set
            {
                if (ignoreSelectionChanges)
                {
                    return;
                }

                if (value == selectedGame && SelectedGameDetails?.Game == value)
                {
                    return;
                }

                SelectedGameDetails?.Dispose();
                if (value == null)
                {
                    SelectedGameDetails = null;
                }
                else
                {
                    if (AppSettings.ViewSettings.GamesViewType == ViewType.Details ||
                        (AppSettings.ViewSettings.GamesViewType == ViewType.Grid && AppSettings.GridViewSideBarVisible))
                    {
                        SelectedGameDetails = new GameDetailsViewModel(value, AppSettings, GamesEditor, Dialogs, Resources);
                    }
                    else
                    {
                        SelectedGameDetails = null;
                    }
                }

                selectedGame = value;
                OnPropertyChanged();
            }
        }

        private List<GamesCollectionViewEntry> selectedGames;
        public new List<GamesCollectionViewEntry> SelectedGames
        {
            get => selectedGames;
            set
            {
                selectedGames = value;
                OnPropertyChanged();
            }
        }

        private IList<object> selectedGamesBinder;
        public IList<object> SelectedGamesBinder
        {
            get => selectedGamesBinder;
            set
            {
                var oldValue = SelectedGames;
                selectedGamesBinder = value;
                if (selectedGamesBinder == null)
                {
                    SelectedGames = null;
                }
                else
                {
                    SelectedGames = selectedGamesBinder.Cast<GamesCollectionViewEntry>().ToList();
                }

                OnPropertyChanged();
                if (!IsDisposing)
                {
                    Extensions.InvokeOnGameSelected(
                        oldValue?.Select(a => a.Game).ToList(),
                        SelectedGames?.Select(a => a.Game).ToList());
                }
            }
        }

        private List<ThirdPartyTool> thirdPartyTools = new List<ThirdPartyTool>();
        public List<ThirdPartyTool> ThirdPartyTools
        {
            get => thirdPartyTools;
            set
            {
                thirdPartyTools = value;
                OnPropertyChanged();
            }
        }

        private bool gameAdditionAllowed = true;
        public bool GameAdditionAllowed
        {
            get => gameAdditionAllowed;
            set
            {
                gameAdditionAllowed = value;
                OnPropertyChanged();
            }
        }

        private string progressStatus;
        public new string ProgressStatus
        {
            get => progressStatus;
            set
            {
                progressStatus = value;
                OnPropertyChanged();
            }
        }

        private double progressValue;
        public new double ProgressValue
        {
            get => progressValue;
            set
            {
                progressValue = value;
                OnPropertyChanged();
            }
        }

        private double progressTotal;
        public new double ProgressTotal
        {
            get => progressTotal;
            set
            {
                progressTotal = value;
                OnPropertyChanged();
            }
        }

        private bool progressVisible = false;
        public new bool ProgressVisible
        {
            get => progressVisible;
            set
            {
                progressVisible = value;
                OnPropertyChanged();
            }
        }

        private bool searchOpened = false;
        public bool SearchOpened
        {
            get => searchOpened;
            set
            {
                searchOpened = value;
                OnPropertyChanged();
            }
        }

        private Visibility visibility = Visibility.Visible;
        public Visibility Visibility
        {
            get => visibility;
            set
            {
                visibility = value;
                OnPropertyChanged();
            }
        }

        private WindowState windowState = WindowState.Normal;
        public WindowState WindowState
        {
            get => windowState;
            set
            {
                if (value == WindowState.Minimized && AppSettings.MinimizeToTray && AppSettings.EnableTray)
                {
                    Visibility = Visibility.Hidden;
                    ImageSourceManager.Cache.Clear();
                }

                windowState = value;
                OnPropertyChanged();
            }
        }

        private DatabaseStats gamesStats;
        public DatabaseStats GamesStats
        {
            get => gamesStats;
            set
            {
                gamesStats = value;
                OnPropertyChanged();
            }
        }

        private DatabaseFilter databaseFilters;
        public DatabaseFilter DatabaseFilters
        {
            get => databaseFilters;
            set
            {
                databaseFilters = value;
                OnPropertyChanged();
            }
        }

        private DatabaseExplorer databaseExplorer;
        public DatabaseExplorer DatabaseExplorer
        {
            get => databaseExplorer;
            set
            {
                databaseExplorer = value;
                OnPropertyChanged();
            }
        }

        public DesktopAppViewModel() : base(ApplicationMode.Desktop)
        {
        }

        public DesktopAppViewModel(
            GameDatabase database,
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            PlayniteSettings settings,
            DesktopGamesEditor gamesEditor,
            PlayniteAPI playniteApi,
            ExtensionFactory extensions,
            PlayniteApplication app) : base(ApplicationMode.Desktop)
        {
            context = SynchronizationContext.Current;
            application = app;
            Window = window;
            Dialogs = dialogs;
            Resources = resources;
            Database = database;
            GamesEditor = gamesEditor;
            AppSettings = settings;
            PlayniteApi = playniteApi;
            Extensions = extensions;
            ((NotificationsAPI)PlayniteApi.Notifications).ActivationRequested += DesktopAppViewModel_ActivationRequested; ;
            AppSettings.FilterSettings.PropertyChanged += FilterSettings_PropertyChanged;
            AppSettings.ViewSettings.PropertyChanged += ViewSettings_PropertyChanged;
            AppSettings.PropertyChanged += AppSettings_PropertyChanged;
            GamesStats = new DatabaseStats(database);
            InitializeCommands();
        }

        private void DesktopAppViewModel_ActivationRequested(object sender, NotificationsAPI.ActivationRequestEventArgs e)
        {
            PlayniteApi.Notifications.Remove(e.Message.Id);
            AppSettings.NotificationPanelVisible = false;
            e.Message.ActivationAction();
        }

        private void ViewSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppSettings.ViewSettings.GamesViewType) &&
                AppSettings.ViewSettings.GamesViewType == ViewType.Grid &&
                AppSettings.GridViewSideBarVisible &&
                SelectedGameDetails == null &&
                SelectedGame != null)
            {
                SelectedGameDetails = new GameDetailsViewModel(SelectedGame, AppSettings, GamesEditor, Dialogs, Resources);
            }
            else if (e.PropertyName == nameof(AppSettings.ViewSettings.GamesViewType))
            {
                SelectedGame = null;
                SelectedGameDetails = null;
            }
        }

        private void AppSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        private void FilterSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        public void StartThirdPartyTool(ThirdPartyTool tool)
        {
            try
            {
                tool.Start();
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                Logger.Error(e, "Failed to start 3rd party tool.");
                Dialogs.ShowErrorMessage(Resources.GetString("LOCAppStartupError") + "\n\n" + e.Message, Resources.GetString("LOCStartupError"));
            }
        }

        public void RemoveGameSelection()
        {
            SelectedGame = null;
            SelectedGamesBinder = null;
        }

        public void OpenSteamFriends()
        {
            try
            {
                ProcessStarter.StartUrl(@"steam://open/friends");
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                Logger.Error(e, "Failed to open Steam friends.");
            }
        }

        public static void ReportIssue()
        {
            CrashHandlerViewModel.ReportIssue();
        }

        public void ShutdownApp()
        {
            Dispose();
            application.Quit();
        }

        public void RestartAppSafe()
        {
            CloseView();
            application.Restart(new CmdLineOptions { SafeStartup = true });
        }

        protected void InitializeView()
        {
            LibraryStats = new StatisticsViewModel(Database, Extensions, AppSettings, PlayniteApi, (g) =>
            {
                SwitchToLibraryView();
                SelectedGame = GamesView.Items.FirstOrDefault(a => g.Id == a.Id);
            });

            LoadSideBarItems();
            DatabaseFilters = new DatabaseFilter(Database, Extensions, AppSettings, AppSettings.FilterSettings);
            DatabaseExplorer = new DatabaseExplorer(Database, Extensions, AppSettings);

            var openProgress = new ProgressViewViewModel(new ProgressWindowFactory(),
            (_) =>
            {
                if (!Database.IsOpen)
                {
                    Database.SetDatabasePath(AppSettings.DatabasePath);
                    Database.OpenDatabase();
                }
            }, new GlobalProgressOptions("LOCOpeningDatabase"));

            if (openProgress.ActivateProgress().Result != true)
            {
                Logger.Error(openProgress.FailException, "Failed to open library database.");
                var message = Resources.GetString("LOCDatabaseOpenError") + $"\n{openProgress.FailException?.Message}";
                Dialogs.ShowErrorMessage(message, "");
                GameAdditionAllowed = false;
                return;
            }

            GamesView = new DesktopCollectionView(Database, AppSettings, Extensions);
            BindingOperations.EnableCollectionSynchronization(GamesView.Items, gamesLock);
            if (GamesView.CollectionView.Count > 0)
            {
                SelectGame((GamesView.CollectionView.GetItemAt(0) as GamesCollectionViewEntry).Id);
            }
            else
            {
                SelectedGame = null;
            }

            try
            {
                GamesEditor.UpdateJumpList();
            }
            catch (Exception exc)
            {
                Logger.Error(exc, "Failed to set update JumpList data: ");
            }

            Extensions.NotifiyOnApplicationStarted();

            try
            {
                application.Discord = new DiscordManager(AppSettings.DiscordPresenceEnabled);
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                Logger.Error(e, "Failed to initialize Discord manager.");
            }

            LoadSoftwareToolsSidebarItems();
        }

        private List<Game> ImportLibraryGames(LibraryPlugin plugin, CancellationToken token)
        {
            var addedGames = new List<Game>();
            if (token.IsCancellationRequested)
            {
                return addedGames;
            }

            Logger.Info($"Importing games from {plugin.Name} plugin.");
            ProgressStatus = Resources.GetString(LOC.ProgressImportinGames).Format(plugin.Name);

            try
            {
                addedGames.AddRange(Database.ImportGames(plugin, AppSettings.ForcePlayTimeSync, AppSettings.ImportExclusionList.Items));
                RemoveMessage($"{plugin.Id} - download");
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                Logger.Error(e, $"Failed to import games from plugin: {plugin.Name}");
                AddMessage(new NotificationMessage(
                    $"{plugin.Id} - download",
                    Resources.GetString(LOC.LibraryImportError).Format(plugin.Name) + $"\n{e.Message}",
                    NotificationType.Error));
            }

            return addedGames;
        }

        public async Task UpdateLibrary(bool metaForNewGames)
        {
            await UpdateLibraryData((token) =>
            {
                var addedGames = new List<Game>();
                foreach (var plugin in Extensions.LibraryPlugins)
                {
                    if (token.IsCancellationRequested)
                    {
                        return addedGames;
                    }

                    addedGames.AddRange(ImportLibraryGames(plugin, token));
                }

                var importedRoms = Database.GetImportedRomFiles();
                foreach (var scanConfig in Database.GameScanners.Where(a => a.InGlobalUpdate).ToList())
                {
                    if (token.IsCancellationRequested)
                    {
                        return addedGames;
                    }

                    addedGames.AddRange(ImportEmulatedGames(scanConfig, importedRoms, token));
                }

                return addedGames;
            }, AppSettings.DownloadMetadataOnImport);
        }

        public async Task UpdateLibrary(LibraryPlugin plugin)
        {
            await UpdateLibraryData((token) =>
            {
                return ImportLibraryGames(plugin, token);
            }, AppSettings.DownloadMetadataOnImport);
        }

        private List<Game> ImportEmulatedGames(GameScannerConfig scanConfig, List<string> importedFiles, CancellationToken token)
        {
            var addedGames = new List<Game>();
            if (token.IsCancellationRequested)
            {
                return addedGames;
            }

            Logger.Info($"Importing emulated games from {scanConfig.Name} config.");
            ProgressStatus = Resources.GetString(LOC.ProgressImportinEmulatedGames).Format(scanConfig.Directory);

            try
            {
                var scanned = GameScanner.Scan(scanConfig, Database, importedFiles, token).Select(a => a.ToGame()).ToList();
                if (scanned.HasItems())
                {
                    addedGames.AddRange(scanned);
                    Database.Games.Add(scanned);
                }

                RemoveMessage($"{scanConfig.Id} - import");
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                Logger.Error(e, $"Failed to import emulated games from config:\n{scanConfig.Directory}\n{scanConfig.EmulatorId}\n{scanConfig.EmulatorProfileId}");
                AddMessage(new NotificationMessage(
                    $"{scanConfig.Id} - import",
                    Resources.GetString(LOC.LibraryImportEmulatedError).Format(scanConfig.Name) + $"\n{e.Message}",
                    NotificationType.Error));
            }

            return addedGames;
        }

        public async Task UpdateEmulationLibrary(GameScannerConfig config)
        {
            await UpdateLibraryData((token) =>
            {
                return ImportEmulatedGames(config, Database.GetImportedRomFiles(), token);
            }, AppSettings.DownloadMetadataOnImport);
        }

        public async Task UpdateEmulationLibrary()
        {
            await UpdateLibraryData((token) =>
            {
                var addedGames = new List<Game>();
                var importedRoms = Database.GetImportedRomFiles();
                foreach (var scanConfig in Database.GameScanners.Where(a => a.InGlobalUpdate))
                {
                    addedGames.AddRange(ImportEmulatedGames(scanConfig, importedRoms, token));
                }

                return addedGames;
            }, AppSettings.DownloadMetadataOnImport);
        }

        private async Task UpdateLibraryData(Func<CancellationToken, List<Game>> updateAction, bool downloadMetadata)
        {
            if (GlobalTaskHandler.ProgressTask != null && GlobalTaskHandler.ProgressTask.Status == TaskStatus.Running)
            {
                GlobalTaskHandler.CancelToken.Cancel();
                await GlobalTaskHandler.ProgressTask;
            }

            GameAdditionAllowed = false;

            try
            {
                GlobalTaskHandler.CancelToken = new CancellationTokenSource();
                GlobalTaskHandler.ProgressTask = Task.Run(() =>
                {
                    DatabaseFilters.IgnoreDatabaseUpdates = true;
                    ProgressVisible = true;
                    ProgressValue = 0;
                    ProgressTotal = 1;

                    var addedGames = updateAction(GlobalTaskHandler.CancelToken.Token);
                    if (GlobalTaskHandler.CancelToken.IsCancellationRequested)
                    {
                        return;
                    }

                    ProgressStatus = Resources.GetString(LOC.ProgressLibImportFinish);
                    Thread.Sleep(1000);
                    if (addedGames.Any() && downloadMetadata)
                    {
                        Logger.Info($"Downloading metadata for {addedGames.Count} new games.");
                        ProgressValue = 0;
                        ProgressTotal = addedGames.Count;
                        ProgressStatus = Resources.GetString(LOC.ProgressMetadata);
                        using (var downloader = new MetadataDownloader(Database, Extensions.MetadataPlugins, Extensions.LibraryPlugins))
                        {
                            downloader.DownloadMetadataAsync(addedGames, AppSettings.MetadataSettings, AppSettings,
                                (g, i, t) =>
                                {
                                    ProgressValue = i + 1;
                                    ProgressStatus = Resources.GetString(LOC.ProgressMetadata) + $" [{ProgressValue}/{ProgressTotal}]";
                                },
                                GlobalTaskHandler.CancelToken).Wait();
                        }
                    }
                });

                await GlobalTaskHandler.ProgressTask;
                Extensions.NotifiyOnLibraryUpdated();
            }
            finally
            {
                GameAdditionAllowed = true;
                ProgressVisible = false;
                DatabaseFilters.IgnoreDatabaseUpdates = false;
            }
        }

        public async Task CheckForAddonUpdates()
        {
            if (GlobalTaskHandler.ProgressTask != null && GlobalTaskHandler.ProgressTask.Status == TaskStatus.Running)
            {
                GlobalTaskHandler.CancelToken.Cancel();
                await GlobalTaskHandler.ProgressTask;
            }

            GlobalTaskHandler.CancelToken = new CancellationTokenSource();
            GlobalTaskHandler.ProgressTask = Task.Run(() =>
            {
                ProgressVisible = true;
                ProgressValue = 0;
                ProgressTotal = 1;
                ProgressStatus = Resources.GetString(LOC.AddonLookingForUpdates);

                try
                {
                    var updates = Addons.CheckAddonUpdates(application.ServicesClient);
                    if (updates.HasItems())
                    {
                        AddMessage(new NotificationMessage("AddonUpdateAvailable", Resources.GetString(LOC.AddonUpdatesAvailable), NotificationType.Info,
                            () => {
                                new AddonsViewModel(
                                     new AddonsWindowFactory(),
                                     PlayniteApi,
                                     Dialogs,
                                     Resources,
                                     application.ServicesClient,
                                     Extensions,
                                     AppSettings,
                                     application,
                                     updates).OpenView();
                            }));
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    Logger.Error(e, "Failed to check for addon updates.");
                }
                finally
                {
                    ProgressVisible = false;
                }
            });

            await GlobalTaskHandler.ProgressTask;
        }

        public async Task DownloadMetadata(MetadataDownloaderSettings settings, List<Game> games)
        {
            GameAdditionAllowed = false;

            try
            {
                if (GlobalTaskHandler.ProgressTask != null && GlobalTaskHandler.ProgressTask.Status == TaskStatus.Running)
                {
                    GlobalTaskHandler.CancelToken.Cancel();
                    await GlobalTaskHandler.ProgressTask;
                }

                DatabaseFilters.IgnoreDatabaseUpdates = true;
                GlobalTaskHandler.CancelToken = new CancellationTokenSource();
                ProgressVisible = true;
                ProgressValue = 0;
                ProgressTotal = games.Count;
                ProgressStatus = Resources.GetString("LOCProgressMetadata");

                using (var downloader = new MetadataDownloader(Database, Extensions.MetadataPlugins, Extensions.LibraryPlugins))
                {
                    GlobalTaskHandler.ProgressTask =
                        downloader.DownloadMetadataAsync(games, settings, AppSettings,
                            (g, i, t) =>
                            {
                                ProgressValue = i + 1;
                                ProgressStatus = Resources.GetString("LOCProgressMetadata") + $" [{ProgressValue}/{ProgressTotal}]";
                            },
                            GlobalTaskHandler.CancelToken);
                    await GlobalTaskHandler.ProgressTask;
                }
            }
            finally
            {
                ProgressVisible = false;
                GameAdditionAllowed = true;
                DatabaseFilters.IgnoreDatabaseUpdates = false;
            }
        }

        public async Task DownloadMetadata(MetadataDownloaderSettings settings)
        {
            List<Game> games = null;
            if (settings.GamesSource == MetadataGamesSource.Selected)
            {
                if (SelectedGames != null && SelectedGames.Count() > 0)
                {
                    games = SelectedGames.Select(a => a.Game).Distinct().ToList();
                }
                else
                {
                    return;
                }
            }
            else if (settings.GamesSource == MetadataGamesSource.AllFromDB)
            {
                games = Database.Games.ToList();
            }
            else if (settings.GamesSource == MetadataGamesSource.Filtered)
            {
                games = GamesView.CollectionView.Cast<GamesCollectionViewEntry>().Select(a => a.Game).Distinct().ToList();
            }

            await DownloadMetadata(settings, games);
        }

        public async void DownloadMetadata(MetadataDownloadViewModel model)
        {
            if (model.OpenView(MetadataDownloadViewModel.ViewMode.Manual, AppSettings.MetadataSettings.GetClone()) != true)
            {
                return;
            }

            await DownloadMetadata(model.Settings);
        }

        public void RestoreWindow()
        {
            Window.RestoreWindow();
        }

        public void AddCustomGame(IWindowFactory window)
        {
            var newGame = new Game()
            {
                Name = "New Game",
                IsInstalled = true
            };

            Database.Games.Add(newGame);
            if (GamesEditor.EditGame(newGame) == true)
            {
                var viewEntry = GamesView.Items.First(a => a.Game.Id == newGame.Id);
                SelectedGame = viewEntry;
            }
            else
            {
                Database.Games.Remove(newGame);
            }
}

        public async void ImportWindowsStoreGames(InstalledGamesViewModel model)
        {
            if (model.OpenViewOnWindowsApps() == true && model.SelectedGames?.Any() == true)
            {
                var addedGames = InstalledGamesViewModel.AddImportableGamesToDb(model.SelectedGames, Database);
                if (AppSettings.DownloadMetadataOnImport)
                {
                    if (!GlobalTaskHandler.IsActive)
                    {
                        await DownloadMetadata(AppSettings.MetadataSettings, addedGames);
                    }
                    else
                    {
                        Logger.Warn("Skipping metadata download for manually added games, some global task is already in progress.");
                    }
                }
            }
        }

        public async void ImportInstalledGames(InstalledGamesViewModel model, string path)
        {
            if (model.OpenView(path) == true && model.SelectedGames?.Any() == true)
            {
                var addedGames = InstalledGamesViewModel.AddImportableGamesToDb(model.SelectedGames, Database);
                if (AppSettings.DownloadMetadataOnImport)
                {
                    if (!GlobalTaskHandler.IsActive)
                    {
                        await DownloadMetadata(AppSettings.MetadataSettings, addedGames);
                    }
                    else
                    {
                        Logger.Warn("Skipping metadata download for manually added games, some global task is already in progress.");
                    }
                }
            }
        }

        public async void ImportEmulatedGames(EmulatedGamesImportViewModel model)
        {
            if (model.OpenView() != true || !model.ImportedGames.HasItems())
            {
                return;
            }

            if (AppSettings.DownloadMetadataOnImport)
            {
                if (!GlobalTaskHandler.IsActive)
                {
                    await DownloadMetadata(AppSettings.MetadataSettings, model.ImportedGames);
                }
                else
                {
                    Logger.Warn("Skipping metadata download for manually added emulated games, some global task is already in progress.");
                }
            }
        }

        public void OpenAboutWindow(AboutViewModel model)
        {
            model.OpenView();
        }

        public void OpenSettings(SettingsViewModel model)
        {
            model.OpenView();
        }

        public void OpenIntegrationSettings(LibraryIntegrationsViewModel model)
        {
            model.OpenView();
        }

        public void ConfigureEmulators(EmulatorsViewModel model)
        {
            model.OpenView();
        }

        private void ConfigureSoftwareTools(ToolsConfigViewModel model)
        {
            model.OpenView();
        }

        public void ConfigureDatabaseFields(DatabaseFieldsManagerViewModel model)
        {
            model.OpenView();
        }

        public void SelectGame(Guid id)
        {
            var viewEntry = GamesView.Items.FirstOrDefault(a => a.Game.Id == id);
            SelectedGame = viewEntry;
        }

        protected virtual void OnClosing(CancelEventArgs args)
        {
            if (ignoreCloseActions)
            {
                return;
            }

            if (AppSettings.CloseToTray && AppSettings.EnableTray)
            {
                Visibility = Visibility.Hidden;
                args.Cancel = true;
                ImageSourceManager.Cache.Clear();
            }
            else
            {
                if (GlobalTaskHandler.IsActive)
                {
                    if (Dialogs.ShowMessage(
                        Resources.GetString("LOCBackgroundProgressCancelAskExit"),
                        Resources.GetString("LOCCrashClosePlaynite"),
                        MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    {
                        args.Cancel = true;
                        return;
                    }
                }

                ShutdownApp();
            }
        }

        private void OnFileDropped(DragEventArgs args)
        {
            if (args.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])args.Data.GetData(DataFormats.FileDrop);
                if (files.Count() == 1)
                {
                    Window.RestoreWindow();

                    var path = files[0];
                    if (File.Exists(path))
                    {
                        var ext = Path.GetExtension(path).ToLower();
                        if (ext.Equals(PlaynitePaths.PackedThemeFileExtention, StringComparison.OrdinalIgnoreCase))
                        {
                            application.InstallThemeFile(path);
                        }
                        else if (ext.Equals(PlaynitePaths.PackedExtensionFileExtention, StringComparison.OrdinalIgnoreCase))
                        {
                            application.InstallExtensionFile(path);
                        }
                        else
                        {
                            // Other file types to be added in #501
                            if (!(new List<string>() { ".exe", ".lnk" }).Contains(ext))
                            {
                                return;
                            }

                            var game = GameExtensions.GetGameFromExecutable(path);
                            var icoPath = game.Icon;
                            game.Icon = null;
                            if (icoPath.IsNullOrEmpty())
                            {
                                var exePath = game.GetRawExecutablePath();
                                if (!string.IsNullOrEmpty(exePath))
                                {
                                    icoPath = exePath;
                                }
                            }

                            if (icoPath?.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                using (var ms = new MemoryStream())
                                {
                                    if (IconExtractor.ExtractMainIconFromFile(icoPath, ms))
                                    {
                                        var iconName = Guid.NewGuid().ToString() + ".ico";
                                        game.Icon = Database.AddFile(iconName, ms.ToArray(), game.Id);
                                    }
                                }
                            }
                            else if (!icoPath.IsNullOrEmpty())
                            {
                                game.Icon = Database.AddFile(icoPath, game.Id);
                            }

                            Database.Games.Add(game);
                            Database.AssignPcPlatform(game);
                            if (GamesEditor.EditGame(game) == true)
                            {
                                SelectGame(game.Id);
                            }
                            else
                            {
                                Database.Games.Remove(game);
                            }
                        }
                    }
                    else if (Directory.Exists(path))
                    {
                        var instModel = new InstalledGamesViewModel(
                           new InstalledGamesWindowFactory(),
                           Dialogs);
                        ImportInstalledGames(instModel, path);
                    }
                }
            }
        }

        public void AddMessage(NotificationMessage message)
        {
            PlayniteApi.Notifications.Add(message);
        }

        public void RemoveMessage(string id)
        {
            PlayniteApi.Notifications.Remove(id);
        }

        public void ClearMessages()
        {
            PlayniteApi.Notifications.RemoveAll();
            AppSettings.NotificationPanelVisible = false;
        }

        public void CheckForUpdate()
        {
            try
            {
                var updater = new Updater(application);
                if (updater.IsUpdateAvailable)
                {
                    var model = new UpdateViewModel(updater, new UpdateWindowFactory(), Resources, Dialogs);
                    model.OpenView();
                }
                else
                {
                    Dialogs.ShowMessage(Resources.GetString("LOCUpdateNoNewUpdateMessage"), string.Empty);
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                Logger.Error(e, "Failed to check for update.");
                Dialogs.ShowErrorMessage(Resources.GetString("LOCUpdateCheckFailMessage"), Resources.GetString("LOCUpdateError"));
            }
        }

        public void SwitchToFullscreenMode()
        {
            if (GlobalTaskHandler.IsActive)
            {
                Dialogs.ActivateGlobalProgress(
                    (_) => GlobalTaskHandler.CancelAndWait(),
                    new GlobalProgressOptions("LOCOpeningFullscreenModeMessage"));
            }

            CloseView();
            application.QuitAndStart(
                PlaynitePaths.FullscreenExecutablePath,
                new CmdLineOptions()
                {
                    SkipLibUpdate = true,
                    StartInFullscreen = true,
                    MasterInstance = true
                }.ToString());
        }

        public void PlayRandomGame()
        {
            var model = new RandomGameSelectViewModel(
                Database,
                GamesView,
                new RandomGameSelectWindowFactory(),
                Resources);
            if (model.OpenView() == true && model.SelectedGame != null)
            {
                var selection = GamesView.Items.FirstOrDefault(a => a.Id == model.SelectedGame.Id);
                if (selection != null)
                {
                    SelectedGame = selection;
                    GamesEditor.PlayGame(selection.Game);
                }
            }
        }

        public void OpenView()
        {
            Window.Show(this);
            application.UpdateScreenInformation(Window.Window);
            Window.Window.LocationChanged += Window_LocationChanged;

            if (AppSettings.StartMinimized)
            {
                WindowState = WindowState.Minimized;
            }
            else
            {
                Window.RestoreWindow();
            }

            InitializeView();
        }

        public virtual void CloseView()
        {
            ignoreCloseActions = true;
            Window.Close();
            ignoreCloseActions = false;
            Dispose();
        }

        public async void CancelProgress()
        {
            await GlobalTaskHandler.CancelAndWaitAsync();
        }

        public virtual void ClearFilters()
        {
            AppSettings.FilterSettings.ClearFilters();
            ActiveFilterPreset = null;
        }

        public void Dispose()
        {
            IsDisposing = true;
            GamesView?.Dispose();
            GamesStats?.Dispose();
            AppSettings.FilterSettings.PropertyChanged -= FilterSettings_PropertyChanged;
            Window.Window.LocationChanged -= Window_LocationChanged;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            application.UpdateScreenInformation(Window.Window);
        }

        public bool OpenPluginSettings(Guid pluginId)
        {
            if (!Extensions.Plugins.ContainsKey(pluginId))
            {
                Logger.Error($"Cannot open plugin settings, plugin is not loaded {pluginId}");
                return false;
            }

            var model = new PluginSettingsViewModel(
                new PluginSettingsWindowFactory(),
                Resources,
                Dialogs,
                Extensions,
                pluginId);
            return model.OpenView() ?? false;
        }

        private void StartSoftwareTool(AppSoftware app)
        {
            try
            {
                ProcessStarter.StartProcess(app.Path, app.Arguments, app.WorkingDir);
            }
            catch (Exception e)  when (!PlayniteEnvironment.ThrowAllErrors)
            {
                Logger.Error(e, "Failed to start app tool.");
                Dialogs.ShowErrorMessage(
                    Resources.GetString("LOCAppStartupError") + "\n\n" +
                    e.Message,
                    "LOCStartupError");
            }
        }

        public void SwitchToLibraryView()
        {
            SidebarItems.First(a => a.SideItem is MainSidebarViewItem item && item.AppView == ApplicationView.Library).Command.Execute(null);
        }

        internal void ProcessUriRequest(PlayniteUriEventArgs args)
        {
            var arguments = args.Arguments;
            if (args.Arguments.Count() == 0)
            {
                return;
            }

            var command = arguments[0];
            switch (command)
            {
                case UriCommands.ShowGame:
                    if (Guid.TryParse(arguments[1], out var gameId))
                    {
                        var game = Database.Games[gameId];
                        if (game == null)
                        {
                            Logger.Error($"Cannot display game, game {arguments[1]} not found.");
                        }
                        else
                        {
                            RestoreWindow();
                            SwitchToLibraryView();
                            SelectedGame = GamesView.Items.FirstOrDefault(a => game.Id == a.Id);
                        }
                    }
                    else
                    {
                        Logger.Error($"Can't display game, failed to parse game id: {arguments[1]}");
                    }

                    break;

                default:
                    Logger.Warn($"Uknown URI command {command}");
                    break;
            }
        }
    }
}