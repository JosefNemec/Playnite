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
    public partial class DesktopAppViewModel : MainViewModelBase, IDisposable, IMainViewModelBase
    {
        private static object gamesLock = new object();
        protected bool ignoreCloseActions = false;
        protected bool ignoreSelectionChanges = false;
        private readonly SynchronizationContext context;
        private Controls.LibraryStatistics statsView;
        private Controls.Views.Library libraryView;
        private SearchViewModel currentGlobalSearch;

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
                if (selectedGameDetails != value)
                {
                    selectedGameDetails?.Dispose();
                    selectedGameDetails = value;
                }
            }
        }

        public GamesCollectionViewEntry SelectedGame { get => SelectedGames?.Count > 0 ? SelectedGames[0] : null; }

        private List<GamesCollectionViewEntry> selectedGames;
        public List<GamesCollectionViewEntry> SelectedGames
        {
            get => selectedGames;
            set
            {
                if (ignoreSelectionChanges)
                {
                    return;
                }

                var oldValue = selectedGames;
                selectedGames = value;
                GamesCollectionViewEntry toSelect = null;
                if (selectedGames?.Count > 0)
                {
                    toSelect = selectedGames[0];
                }

                if (SelectedGameDetails?.Game != toSelect)
                {
                    if (toSelect == null)
                    {
                        SelectedGameDetails = null;
                    }
                    else
                    {
                        SelectedGameDetails = new GameDetailsViewModel(toSelect, AppSettings, GamesEditor, Dialogs, Resources);
                    }
                }

                OnPropertyChanged(nameof(SelectedGameDetails));
                OnPropertyChanged(nameof(SelectedGame));
                OnPropertyChanged();
                if (!IsDisposing)
                {
                    Extensions.InvokeOnGameSelected(
                        oldValue?.Select(a => a.Game).Distinct().ToList(),
                        SelectedGames?.Select(a => a.Game).Distinct().ToList());
                }

                ignoreSelectionChanges = true;
                SelectedGamesBinder = selectedGames?.Cast<object>().ToList();
                ignoreSelectionChanges = false;
            }
        }

        // SelectedGamesBinder is only used as a glue to bind to ListBox because its
        // SelectedItems is IList which can't bind anything else.
        private IList<object> selectedGamesBinder;
        public IList<object> SelectedGamesBinder
        {
            get => selectedGamesBinder;
            set
            {
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

        /// <summary>
        /// This constructor should be used on from <see cref="DesignMainViewModel"/> for Blend usage!
        /// </summary>
        public DesktopAppViewModel(
            IGameDatabaseMain database,
            PlayniteApplication app,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            ExtensionFactory extensions) : base(database, app, dialogs, resources, extensions, null)
        {
        }

        public DesktopAppViewModel(
            IGameDatabaseMain database,
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            PlayniteSettings settings,
            DesktopGamesEditor gamesEditor,
            ExtensionFactory extensions,
            PlayniteApplication app) : base(database, app, dialogs, resources, extensions, window)
        {
            context = SynchronizationContext.Current;
            GamesEditor = gamesEditor;
            AppSettings = settings;
            App.Notifications.ActivationRequested += DesktopAppViewModel_ActivationRequested;
            App.Notifications.CloseRequested += Notifications_CloseRequested;
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

        private void DesktopAppViewModel_ActivationRequested(object sender, NotificationsAPI.MessageEventArgs e)
        {
            App.Notifications.Remove(e.Message.Id);
            AppSettings.NotificationPanelVisible = false;
            e.Message.ActivationAction();
        }

        private void Notifications_CloseRequested(object sender, NotificationsAPI.MessageEventArgs e)
        {
            App.Notifications.Remove(e.Message.Id);
            if (App.Notifications.Messages.Count == 0)
            {
                AppSettings.NotificationPanelVisible = false;
            }
        }

        private void ViewSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!IgnoreFilterChanges && ActiveFilterPreset != null)
            {
                if (ActiveFilterPreset.SortingOrder != null)
                {
                    ActiveFilterPreset = null;
                }
            }

            if (e.PropertyName == nameof(ViewSettings.GamesViewType))
            {
                // This is done to keep behavior same as in P9 because it could otherwise break some plugins
                // that set behavior of custom UI elements based on active view and they refresh on game seletion change.
                if (SelectedGames != null)
                {
                    SelectedGames = null;
                }
            }
        }

        private void AppSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var oldDetailsListIconProperties = GamesCollectionViewEntry.DetailsListIconProperties;
            var oldGridViewCoverProperties = GamesCollectionViewEntry.GridViewCoverProperties;
            if (e.PropertyName == nameof(PlayniteSettings.GridItemWidth) ||
                e.PropertyName == nameof(PlayniteSettings.CoverAspectRatio) ||
                e.PropertyName == nameof(PlayniteSettings.CoverArtStretch) ||
                e.PropertyName == nameof(PlayniteSettings.ImageScalerMode) ||
                e.PropertyName == nameof(PlayniteSettings.DetailsViewListIconSize))
            {
                GamesCollectionViewEntry.InitItemViewProperties(App, AppSettings);
            }

            var notifyProps = new List<string>();
            if (e.PropertyName == nameof(PlayniteSettings.DetailsViewListIconSize) && oldDetailsListIconProperties != GamesCollectionViewEntry.DetailsListIconProperties)
            {
                notifyProps.Add(nameof(GamesCollectionViewEntry.DetailsListIconObjectCached));
                notifyProps.Add(nameof(GamesCollectionViewEntry.DefaultDetailsListIconObjectCached));
            }

            if ((e.PropertyName == nameof(PlayniteSettings.GridItemWidth) ||
                e.PropertyName == nameof(PlayniteSettings.CoverAspectRatio) ||
                e.PropertyName == nameof(PlayniteSettings.CoverArtStretch)) && oldGridViewCoverProperties != GamesCollectionViewEntry.GridViewCoverProperties)
            {
                notifyProps.Add(nameof(GamesCollectionViewEntry.GridViewCoverObjectCached));
                notifyProps.Add(nameof(GamesCollectionViewEntry.DefaultGridViewCoverObjectCached));
            }

            if (notifyProps.HasItems())
            {
                GamesView.NotifyItemPropertyChanges(notifyProps.ToArray());
            }

            if (e.PropertyName == nameof(PlayniteSettings.SystemSearchHotkey))
            {
                RegisterSystemSearchHotkey();
            }
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
            SelectedGames = null;
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

        protected void InitializeView()
        {
            GamesCollectionViewEntry.InitItemViewProperties(App, AppSettings);
            LibraryStats = new StatisticsViewModel(Database, Extensions, AppSettings, SwitchToLibraryView, (g) =>
            {
                SwitchToLibraryView();
                SelectGame(g.Id);
            });

            LoadSideBarItems();
            DatabaseFilters = new DatabaseFilter(Database, Extensions, AppSettings, AppSettings.FilterSettings);
            DatabaseExplorer = new DatabaseExplorer(Database, Extensions, AppSettings, this);

            var openProgress = new ProgressViewViewModel(
                new ProgressWindowFactory(),
                new GlobalProgressOptions(LOC.OpeningDatabase));

            if (openProgress.ActivateProgress((_) =>
            {
                if (!Database.IsOpen)
                {
                    Database.SetDatabasePath(AppSettings.DatabasePath);
                    Database.OpenDatabase();
                }
            }).Result != true)
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
                SelectedGames = null;
            }

            try
            {
                GamesEditor.UpdateJumpList();
            }
            catch (Exception exc)
            {
                Logger.Error(exc, "Failed to set update JumpList data: ");
            }

            RunStartupScript();
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

            RegisterSystemSearchHotkey();
            if (Database.IsOpen)
            {
                Database.Games.ItemCollectionChanged += Games_ItemCollectionChanged;
            }
        }

        private void Games_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<Game> e)
        {
            if (e.RemovedItems.HasItems() && SelectedGameDetails != null)
            {
                if (e.RemovedItems.Any(a => a.Id == SelectedGameDetails.Game.Id))
                {
                    SelectedGameDetails = null;
                    OnPropertyChanged(nameof(SelectedGameDetails));
                }
            }
        }

        public override NotificationMessage GetAddonUpdatesFoundMessage(List<AddonUpdate> updates)
        {
            return new NotificationMessage("AddonUpdateAvailable", Resources.GetString(LOC.AddonUpdatesAvailable), NotificationType.Info, () =>
            {
                new AddonsViewModel(
                     new AddonsWindowFactory(),
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
                SelectGame(newGame.Id);
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

                if (!GlobalTaskHandler.IsActive)
                {
                    GlobalTaskHandler.CancelToken = new CancellationTokenSource();
                    GlobalTaskHandler.ProgressTask = Task.Run(() => UpdateGamesInstallSizes(GlobalTaskHandler.CancelToken.Token, addedGames, LOC.ProgressScanningImportedGamesInstallSize));
                    await GlobalTaskHandler.ProgressTask;
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

                if (!GlobalTaskHandler.IsActive)
                {
                    GlobalTaskHandler.CancelToken = new CancellationTokenSource();
                    GlobalTaskHandler.ProgressTask = Task.Run(() => UpdateGamesInstallSizes(GlobalTaskHandler.CancelToken.Token, addedGames, LOC.ProgressScanningImportedGamesInstallSize));
                    await GlobalTaskHandler.ProgressTask;
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

            if (!GlobalTaskHandler.IsActive)
            {
                GlobalTaskHandler.CancelToken = new CancellationTokenSource();
                GlobalTaskHandler.ProgressTask = Task.Run(() => UpdateGamesInstallSizes(GlobalTaskHandler.CancelToken.Token, model.ImportedGames, LOC.ProgressScanningImportedGamesInstallSize));
                await GlobalTaskHandler.ProgressTask;
            }

            await SetSortingNames(model.ImportedGames);
        }

        public void OpenAboutWindow(AboutViewModel model)
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

        public override void SelectGame(Guid id, bool restoreView = false)
        {
            var viewEntry = GamesView.Items.FirstOrDefault(a => a.Game.Id == id);
            if (viewEntry != null)
            {
                SelectedGames = new List<GamesCollectionViewEntry>(1) { viewEntry };
            }

            if (restoreView && Window?.Window?.IsActive == false)
            {
                Window.RestoreWindow();
            }
        }

        public void SelectGames(IEnumerable<Guid> gameIds, bool restoreView = false)
        {
            if (!gameIds.HasItems())
            {
                return;
            }

            var entries = GamesView.Items.Where(a => gameIds.Contains(a.Game.Id));
            if (entries.HasItems())
            {
                SelectedGames = entries.ToList();
            }

            if (restoreView && Window?.Window?.IsActive == false)
            {
                Window.RestoreWindow();
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
            App.Notifications.RemoveAll();
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
                if (Dialogs.ShowMessage(
                    Resources.GetString(LOC.BackgroundProgressCancelAskSwitchMode),
                    Resources.GetString(LOC.MenuOpenFullscreen),
                    MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }

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
            model.OpenView();
            if (model.SelectedAction == RandomGameSelectAction.Play)
            {
                SelectGame(model.SelectedGame.Id);
                GamesEditor.PlayGame(model.SelectedGame);
            }
            else if (model.SelectedAction == RandomGameSelectAction.Navigate)
            {
                if (AppSettings.ViewSettings.GamesViewType == DesktopView.List)
                {
                    AppSettings.ViewSettings.GamesViewType = DesktopView.Details;
                }
                else if (AppSettings.ViewSettings.GamesViewType == DesktopView.Grid)
                {
                    if (!AppSettings.GridViewSideBarVisible)
                    {
                        AppSettings.GridViewSideBarVisible = true;
                    }
                }

                SelectGame(model.SelectedGame.Id);
            }
        }

        public void ViewSelectRandomGame()
        {
            var count = GamesView.CollectionView.Count;
            if (count == 1)
            {
                SelectGame((GamesView.CollectionView.GetItemAt(0) as GamesCollectionViewEntry).Id);
            }
            else if (count > 1)
            {
                while (true)
                {
                    var index = GlobalRandom.Next(0, count);
                    var newGame = GamesView.CollectionView.GetItemAt(index) as GamesCollectionViewEntry;
                    if (SelectedGame == null || SelectedGame != newGame)
                    {
                        SelectGame(newGame.Id);
                        break;
                    }
                }
            }
        }

        public void OpenView()
        {
            if (App.CmdLine.StartClosedToTray && AppSettings.EnableTray)
            {
                Visibility = Visibility.Hidden;
            }

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

        public override void CloseView()
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
                            SelectGame(game.Id);
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

        public override IEnumerable<SearchItem> GetSearchCommands()
        {
            SearchItem createItemG<T>(string root, string name, RelayCommand<T> command, T commandParam = null, object icon = null) where T : class
            {
                return new SearchItem(
                    root.IsNullOrEmpty() ? name.GetLocalized() : $"{root.GetLocalized()} > {name.GetLocalized()}",
                    LOC.Open,
                    () => command.Execute(commandParam),
                    icon);
            }

            SearchItem createItemH(string root, string name, RelayCommand command, object icon = null)
            {
                return new SearchItem(
                    root.IsNullOrEmpty() ? name.GetLocalized() : $"{root.GetLocalized()} > {name.GetLocalized()}",
                    LOC.Open,
                    () => command.Execute(),
                    icon);
            }

            SearchItem createItem<T>(string root, string name, RelayCommand<T> command, T commandParam, object icon = null)
            {
                return new SearchItem(
                    root.IsNullOrEmpty() ? name.GetLocalized() : $"{root.GetLocalized()} > {name.GetLocalized()}",
                    LOC.Open,
                    () => command.Execute(commandParam),
                    icon);
            }

            // Add game
            yield return createItemG(LOC.MenuAddGame, LOC.MenuAddGameManual, AddCustomGameCommand);
            yield return createItemG(LOC.MenuAddGame, LOC.MenuAddGameInstalled, AddInstalledGamesCommand);
            yield return createItemG(LOC.MenuAddGame, LOC.MenuAddGameEmulated, AddEmulatedGamesCommand);
            yield return createItemG(LOC.MenuAddGame, LOC.MenuAddWindowsStore, AddWindowsStoreGamesCommand);

            // Library
            yield return createItemG(LOC.Library, LOC.MenuConfigureIntegrations, OpenLibraryIntegrationsConfigCommand);
            yield return createItemG(LOC.Library, LOC.MenuLibraryManagerTitle, OpenDbFieldsManagerCommand);
            yield return createItemG(LOC.Library, LOC.MenuConfigureEmulatorsMenuTitle, OpenEmulatorsCommand);
            yield return createItemG(LOC.Library, LOC.MenuDownloadMetadata, DownloadMetadataCommand);
            yield return createItemG(LOC.Library, LOC.MenuSoftwareTools, OpenSoftwareToolsCommand);
            yield return createItemH(LOC.Library, LOC.MenuBackupData, BackupDataCommand, "BackupIcon");
            yield return createItemH(LOC.Library, LOC.MenuRestoreBackup, RestoreDataBackupCommand, "RestoreBackupIcon");

            // Library update
            foreach (var plugin in Extensions.LibraryPlugins)
            {
                yield return createItemG(LOC.MenuReloadLibrary, plugin.Name, UpdateLibraryCommand, plugin, plugin.LibraryIcon);
            }

            // Extensions main menu items
            foreach (var item in MenuItems.GetSearchExtensionsMainMenuItem(this))
            {
                yield return item;
            }

            // Extensions global commands
            foreach (var item in MenuItems.GetGlobalPluginCommands(this))
            {
                yield return item;
            }

            // Switch mode
            yield return new SearchItem(LOC.MenuOpenFullscreen, LOC.Activate, () => SwitchToFullscreenMode(), "FullscreenModeIcon");

            // Settings
            yield return new SearchItem(LOC.MenuPlayniteSettingsTitle, LOC.Open, () => OpenSettingsCommand.Execute(null), "SettingsIcon");

            // Plugin settings
            foreach (var plugin in Extensions.Plugins)
            {
                yield return createItem(LOC.ExtensionSettingsMenu, plugin.Value.Description.Name, OpenPluginSettingsCommand, plugin.Key, plugin.Value.PluginIcon);
            }

            // Random game
            yield return new SearchItem(LOC.MenuSelectRandomGame, LOC.Open, () => SelectRandomGameCommand.Execute(null), "DiceIcon");

            // Addons window
            yield return new SearchItem(LOC.MenuAddons, LOC.Open, () => OpenAddonsCommand.Execute(null), "AddonsIcon");

            // Open client
            foreach (var tool in ThirdPartyTools)
            {
                yield return createItem(LOC.MenuOpenClient, tool.Name, ThirdPartyToolOpenCommand, tool, tool.Icon);
            }

            // Check for updates
            yield return new SearchItem(LOC.CheckForUpdates, LOC.Activate, () => CheckForUpdateCommand.Execute(null));

            // Help
            yield return new SearchItem(LOC.MenuAbout, LOC.Open, () => OpenAboutCommand.Execute(null), "AboutPlayniteIcon");
            yield return new SearchItem(LOC.CrashRestartSafe, LOC.Activate, () => RestartInSafeMode.Execute(null));
            yield return createItemG(LOC.MenuHelpTitle, "Wiki / FAQ", GlobalCommands.NavigateUrlCommand, UrlConstants.Wiki);
            yield return createItemG(LOC.MenuHelpTitle, LOC.MenuIssues, ReportIssueCommand);
            yield return createItemG(LOC.MenuHelpTitle, LOC.SDKDocumentation, GlobalCommands.NavigateUrlCommand, UrlConstants.SdkDocs);

            // Restore window
            yield return new SearchItem(LOC.RestoreWindow, LOC.Activate, () => Window.RestoreWindow());

            // Exit
            yield return new SearchItem(LOC.ExitPlaynite, LOC.Activate, () => ShutdownCommand.Execute(null), "ExitIcon");
        }

        public override void OpenSettings(int settingsPageIndex)
        {
            new SettingsViewModel(Database,
                AppSettings,
                new SettingsWindowFactory(),
                Dialogs,
                Resources,
                Extensions,
                App).OpenView((DesktopSettingsPage)settingsPageIndex);
        }

        public void OpenSettings()
        {
            OpenSettings(0);
        }

        public override void EditGame(Game game)
        {
            if (GamesEditor.EditGame(game) == true)
            {
                SelectGame(game.Id);
            }
        }

        public override void AssignCategories(Game game)
        {
            if (GamesEditor.SetGameCategories(game) == true)
            {
                SelectGame(game.Id);
            }
        }

        public void OpenSearch()
        {
            if (AppSettings.ShowTopPanelSearchBox && AppSettings.GlobalSearchOpenWithLegacySearch)
            {
                OpenGlobalSearch();
            }
            else if (AppSettings.ShowTopPanelSearchBox && !AppSettings.GlobalSearchOpenWithLegacySearch)
            {
                FocusSearchBox();
            }
            else
            {
                OpenGlobalSearch();
            }
        }

        public void FocusSearchBox()
        {
            SearchOpened = false;
            SearchOpened = true;
        }

        public void OpenGlobalSearch()
        {
            if (currentGlobalSearch != null)
            {
                if (currentGlobalSearch.Active)
                {
                    currentGlobalSearch.Close();
                }
                else
                {
                    currentGlobalSearch.Focus();
                }
            }
            else
            {
                CreateAndSetGlobalSearchView();
                currentGlobalSearch.OpenSearch();
            }
        }

        public void OpenSearch(string searchTerm)
        {
            currentGlobalSearch?.Close();
            CreateAndSetGlobalSearchView();
            currentGlobalSearch.OpenSearch(searchTerm);
        }

        public void OpenSearch(SearchContext context, string searchTerm)
        {
            currentGlobalSearch?.Close();
            CreateAndSetGlobalSearchView();
            currentGlobalSearch.OpenSearch(context, searchTerm);
        }

        private SearchViewModel CreateAndSetGlobalSearchView()
        {
            currentGlobalSearch = new SearchViewModel(
              new SearchWindowFactory(),
              Database,
              Extensions,
              this);
            currentGlobalSearch.SearchClosed += (_, __) => currentGlobalSearch = null;
            return currentGlobalSearch;
        }

        public void RegisterSystemSearchHotkey()
        {
            UnregisterSystemSearchHotkey();
            if (AppSettings.SystemSearchHotkey == null)
            {
                return;
            }

            try
            {
                Window.Window.RegisterHotKeyHandler(1337, AppSettings.SystemSearchHotkey, () =>
                {
                    OpenGlobalSearch();
                });
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                Logger.Error(e, "Failed to register system search hotkey.");
            }
        }

        public void UnregisterSystemSearchHotkey()
        {
            try
            {
                Window.Window.UnregisterHotKeyHandler(1337);
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                Logger.Error(e, "Failed to unregister system search hotkey.");
            }
        }
    }
}