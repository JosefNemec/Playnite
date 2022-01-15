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
        private static object gamesLock = new object();
        protected bool ignoreCloseActions = false;
        protected bool ignoreSelectionChanges = false;
        private readonly SynchronizationContext context;
        private Controls.LibraryStatistics statsView;
        private Controls.Views.Library libraryView;

        public IWindowFactory Window { get; }
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
                    SelectedGameDetails = new GameDetailsViewModel(value, AppSettings, GamesEditor, Dialogs, Resources);
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

        public DesktopAppViewModel() : base(null, null, null, null, null, null)
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
            PlayniteApplication app) : base(database, app, dialogs, playniteApi, resources, extensions)
        {
            context = SynchronizationContext.Current;
            Window = window;
            GamesEditor = gamesEditor;
            AppSettings = settings;
            ((NotificationsAPI)PlayniteApi.Notifications).ActivationRequested += DesktopAppViewModel_ActivationRequested;
            AppSettings.FilterSettings.PropertyChanged += FilterSettings_PropertyChanged;
            AppSettings.FilterSettings.FilterChanged += FilterSettings_FilterChanged;
            AppSettings.ViewSettings.PropertyChanged += ViewSettings_PropertyChanged;
            AppSettings.PropertyChanged += AppSettings_PropertyChanged;
            GamesStats = new DatabaseStats(database);
            InitializeCommands();
        }

        private void FilterSettings_FilterChanged(object sender, FilterChangedEventArgs e)
        {
            if (!IgnoreFilterChanges)
            {
                ActiveFilterPreset = null;
            }
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

            if (!IgnoreFilterChanges && ActiveFilterPreset != null)
            {
                if (ActiveFilterPreset.SortingOrder != null)
                {
                    ActiveFilterPreset = null;
                }
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
            App.Quit();
        }

        public void RestartAppSafe()
        {
            CloseView();
            App.Restart(new CmdLineOptions { SafeStartup = true });
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
            DatabaseExplorer = new DatabaseExplorer(Database, Extensions, AppSettings, this);

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
                App.Discord = new DiscordManager(AppSettings.DiscordPresenceEnabled);
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                Logger.Error(e, "Failed to initialize Discord manager.");
            }

            LoadSoftwareToolsSidebarItems();
            OnPropertyChanged(nameof(SortedFilterPresets));
            OnPropertyChanged(nameof(SortedFilterFullscreenPresets));
            if (AppSettings.SelectedFilterPreset != Guid.Empty)
            {
                ActiveFilterPreset = Database.FilterPresets.FirstOrDefault(a => a.Id == AppSettings.SelectedFilterPreset);
            }
        }

        public override NotificationMessage GetAddonUpdatesFoundMessage(List<AddonUpdate> updates)
        {
            return new NotificationMessage("AddonUpdateAvailable", Resources.GetString(LOC.AddonUpdatesAvailable), NotificationType.Info, () =>
            {
                new AddonsViewModel(
                     new AddonsWindowFactory(),
                     PlayniteApi,
                     Dialogs,
                     Resources,
                     App.ServicesClient,
                     Extensions,
                     AppSettings,
                     App,
                     updates).OpenView();
            });
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
                ProgressActive = true;
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
                            GlobalTaskHandler.CancelToken.Token);
                    await GlobalTaskHandler.ProgressTask;
                }
            }
            finally
            {
                ProgressActive = false;
                GameAdditionAllowed = true;
                DatabaseFilters.IgnoreDatabaseUpdates = false;
            }
        }

        public async Task SetSortingNames(List<Game> games)
        {
            if (!AppSettings.GameSortingNameAutofill)
            {
                return;
            }

            GameAdditionAllowed = false;

            try
            {
                if (GlobalTaskHandler.ProgressTask != null && GlobalTaskHandler.ProgressTask.Status == TaskStatus.Running)
                {
                    Logger.Info("Waiting on other global task to complete before setting Sorting Name for newly added games.");
                    await GlobalTaskHandler.ProgressTask;
                }

                DatabaseFilters.IgnoreDatabaseUpdates = true;
                GlobalTaskHandler.CancelToken = new CancellationTokenSource();
                ProgressActive = true;
                ProgressStatus = Resources.GetString(LOC.SortingNameAutofillProgress);

                var c = new SortableNameConverter(AppSettings.GameSortingNameRemovedArticles, batchOperation: games.Count > 20);
                using (Database.BufferedUpdate())
                {
                    foreach (var game in games)
                    {
                        if (GlobalTaskHandler.CancelToken.Token.IsCancellationRequested)
                        {
                            break;
                        }
                        string sortingName = c.Convert(game.Name);
                        if (sortingName != game.Name)
                        {
                            game.SortingName = sortingName;
                            Database.Games.Update(game);
                        }
                    }
                }
            }
            finally
            {
                ProgressActive = false;
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
                IsInstalled = true,
                CompletionStatusId = Database.GetCompletionStatusSettings().DefaultStatus
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

                await SetSortingNames(addedGames);
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

                await SetSortingNames(addedGames);
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

            await SetSortingNames(model.ImportedGames);
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

        public override void SelectGame(Guid id)
        {
            var viewEntry = GamesView.Items.FirstOrDefault(a => a.Game.Id == id);
            if (viewEntry != null)
            {
                SelectedGame = viewEntry;
            }
        }

        public void SelectGames(IEnumerable<Guid> gameIds)
        {
            var entries = GamesView.Items.Where(a => gameIds.Contains(a.Game.Id));
            if (entries.HasItems())
            {
                SelectedGamesBinder = entries.Cast<object>().ToList();
            }
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
                            App.InstallThemeFile(path);
                        }
                        else if (ext.Equals(PlaynitePaths.PackedExtensionFileExtention, StringComparison.OrdinalIgnoreCase))
                        {
                            App.InstallExtensionFile(path);
                        }
                        else
                        {
                            // Other file types to be added in #501
                            if (!(new List<string>() { ".exe", ".lnk", ".url" }).Contains(ext))
                            {
                                return;
                            }

                            Game game = null;
                            try
                            {
                                game = GameExtensions.GetGameFromExecutable(path);
                            }
                            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                            {
                                Logger.Error(e, "Failed to get game data from file.");
                                return;
                            }

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

                            if (!icoPath.IsNullOrEmpty())
                            {
                                game.Icon = Database.AddFile(icoPath, game.Id, true);
                            }

                            Database.Games.Add(game);
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

        public void ClearMessages()
        {
            PlayniteApi.Notifications.RemoveAll();
            AppSettings.NotificationPanelVisible = false;
        }

        public void CheckForUpdate()
        {
            try
            {
                var updater = new Updater(App);
                if (updater.IsUpdateAvailable)
                {
                    var model = new UpdateViewModel(updater, new UpdateWindowFactory(), Resources, Dialogs, App.Mode);
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

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            CheckForAddonUpdates();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public void SwitchToFullscreenMode()
        {
            Logger.Info("Switching to Fullscreen mode.");
            if (GlobalTaskHandler.IsActive)
            {
                var dialogRes = Dialogs.ActivateGlobalProgress((_) =>
                    {
                        var waitRes = GlobalTaskHandler.CancelAndWait(30_000);
                        if (waitRes == false)
                        {
                            Logger.Error("Active global task failed to finish in time when switching to fullscreen mode.");
                        }
                    },
                    new GlobalProgressOptions(LOC.OpeningFullscreenModeMessage));
                if (dialogRes.Error != null)
                {
                    Logger.Error(dialogRes.Error, "Cancelling global task when switching to fullscreen mode failed.");
                }
            }

            CloseView();
            App.QuitAndStart(
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
            App.UpdateScreenInformation(Window.Window);
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
            App.UpdateScreenInformation(Window.Window);
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
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
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