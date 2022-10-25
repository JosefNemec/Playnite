using Playnite.API;
using Playnite.Common;
using Playnite.Database;
using Playnite.Emulators;
using Playnite.Metadata;
using Playnite.Plugins;
using Playnite.Scripting.PowerShell;
using Playnite.SDK;
using Playnite.SDK.Exceptions;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.ViewModels
{
    public interface IMainViewModelBase
    {
        string ProgressStatus { get; set; }
        double ProgressValue { get; set; }
        double ProgressTotal { get; set; }
        bool ProgressActive { get; set; }
        BaseCollectionView GamesView { get; set; }
        GamesCollectionViewEntry SelectedGame { get; }
        List<GamesCollectionViewEntry> SelectedGames { get; set; }
    }

    public abstract class MainViewModelBase : ObservableObject
    {
        public static ILogger Logger = LogManager.GetLogger();

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
        public string ProgressStatus
        {
            get => progressStatus;
            set
            {
                progressStatus = value;
                OnPropertyChanged();
            }
        }

        private double progressValue;
        public double ProgressValue
        {
            get => progressValue;
            set
            {
                progressValue = value;
                OnPropertyChanged();
            }
        }

        private double progressTotal;
        public double ProgressTotal
        {
            get => progressTotal;
            set
            {
                progressTotal = value;
                OnPropertyChanged();
            }
        }

        private bool progressActive;
        public bool ProgressActive
        {
            get => progressActive;
            set
            {
                progressActive = value;
                OnPropertyChanged();
            }
        }

        private bool updatesAvailable = false;
        public bool UpdatesAvailable
        {
            get => updatesAvailable;
            set
            {
                updatesAvailable = value;
                OnPropertyChanged();
            }
        }

        private BaseCollectionView gamesView;
        public BaseCollectionView GamesView
        {
            get => gamesView;
            set
            {
                gamesView = value;
                OnPropertyChanged();
            }
        }

        public List<FilterPreset> SortedFilterPresets
        {
            get => Database.FilterPresets.OrderBy(a => a.Name).ToList();
        }

        public List<FilterPreset> SortedFilterFullscreenPresets
        {
            get => Database.FilterPresets.Where(a => a.ShowInFullscreeQuickSelection).OrderBy(a => a.Name).ToList();
        }

        public bool IsDisposing { get; set; } = false;
        public RelayCommand<object> AddFilterPresetCommand { get; private set; }
        public RelayCommand<FilterPreset> RenameFilterPresetCommand { get; private set; }
        public RelayCommand<FilterPreset> RemoveFilterPresetCommand { get; private set; }
        public RelayCommand<FilterPreset> ApplyFilterPresetCommand { get; private set; }
        public RelayCommand CancelProgressCommand { get; private set; }
        public RelayCommand<object> OpenUpdatesCommand { get; private set; }
        public RelayCommand StartInteractivePowerShellCommand { get; private set; }
        public RelayCommand RestartInSafeMode { get; private set; }
        public RelayCommand BackupDataCommand { get; private set; }
        public RelayCommand RestoreDataBackupCommand { get; private set; }

        public IGameDatabaseMain Database { get; }
        public PlayniteApplication App { get; }
        public IDialogsFactory Dialogs { get; }
        public IResourceProvider Resources { get; }
        public ExtensionFactory Extensions { get; set; }
        public bool IgnoreFilterChanges { get; set; } = false;
        public IWindowFactory Window { get; }

        public MainViewModelBase(
            IGameDatabaseMain database,
            PlayniteApplication app,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            ExtensionFactory extensions,
            IWindowFactory window)
        {
            Database = database;
            App = app;
            Dialogs = dialogs;
            Resources = resources;
            Extensions = extensions;
            Window = window;

            ApplyFilterPresetCommand = new RelayCommand<FilterPreset>((a) =>
            {
                ApplyFilterPreset(a);
            });

            RemoveFilterPresetCommand = new RelayCommand<FilterPreset>((a) =>
            {
                RemoveFilterPreset(a);
            }, (a) => a != null);

            RenameFilterPresetCommand = new RelayCommand<FilterPreset>((a) =>
            {
                RenameFilterPreset(a);
            }, (a) => a != null);

            AddFilterPresetCommand = new RelayCommand<object>((a) =>
            {
                AddFilterPreset();
            });

            OpenUpdatesCommand = new RelayCommand<object>((_) =>
            {
                OpenUpdates();
            });

            CancelProgressCommand = new RelayCommand(() =>
            {
                CancelProgress();
            }, () => GlobalTaskHandler.CancelToken?.IsCancellationRequested == false);

            StartInteractivePowerShellCommand = new RelayCommand(() =>
            {
                try
                {
                    Scripting.PowerShell.PowerShellRuntime.StartInteractiveSession(new Dictionary<string, object>
                    {
                        { "PlayniteApi", App.PlayniteApiGlobal }
                    });
                }
                catch (Exception e)
                {
                    Dialogs.ShowErrorMessage("Failed to start interactive PowerShell.\n" + e.Message);
                }
            });

            RestartInSafeMode = new RelayCommand(() => RestartAppSafe());
            BackupDataCommand = new RelayCommand(() => BackupData());
            RestoreDataBackupCommand = new RelayCommand(() => RestoreDataBackup());
        }

        private PlayniteSettings appSettings;
        public PlayniteSettings AppSettings
        {
            get => appSettings;
            set
            {
                appSettings = value;
                OnPropertyChanged();
            }
        }

        private FilterPreset activeFilterPreset;
        public FilterPreset ActiveFilterPreset
        {
            get => activeFilterPreset;
            set
            {
                if (activeFilterPreset == value)
                {
                    return;
                }

                activeFilterPreset = value;
                if (App.Mode == ApplicationMode.Desktop)
                {
                    AppSettings.SelectedFilterPreset = value?.Id ?? Guid.Empty;
                }
                else
                {
                    AppSettings.Fullscreen.SelectedFilterPreset = value?.Id ?? Guid.Empty;
                }

                ApplyFilterPreset(value);
                OnPropertyChanged();
            }
        }

        public void ApplyFilterPreset(Guid presetId)
        {
            var preset = Database.FilterPresets[presetId];
            if (preset == null)
            {
                Logger.Error($"Cannot apply filter, filter preset {presetId} not found.");
            }
            else
            {
                ActiveFilterPreset = preset;
            }
        }

        private void ApplyFilterPreset(FilterPreset preset)
        {
            if (preset == null)
            {
                return;
            }

            if (ActiveFilterPreset != preset)
            {
                ActiveFilterPreset = preset;
                return;
            }

            if (GamesView != null)
            {
                GamesView.IgnoreViewConfigChanges = true;
            }

            IgnoreFilterChanges = true;
            var filter = App.Mode == ApplicationMode.Desktop ? AppSettings.FilterSettings : AppSettings.Fullscreen.FilterSettings;
            var view = App.Mode == ApplicationMode.Desktop ? AppSettings.ViewSettings : (ViewSettingsBase)AppSettings.Fullscreen.ViewSettings;
            filter.ApplyFilter(preset.Settings);
            if (preset.SortingOrder != null)
            {
                view.SortingOrder = preset.SortingOrder.Value;
            }

            if (preset.SortingOrderDirection != null)
            {
                view.SortingOrderDirection = preset.SortingOrderDirection.Value;
            }

            if (App.Mode == ApplicationMode.Desktop && preset.GroupingOrder != null)
            {
                AppSettings.ViewSettings.GroupingOrder = preset.GroupingOrder.Value;
            }

            if (GamesView != null)
            {
                IgnoreFilterChanges = false;
                GamesView.IgnoreViewConfigChanges = false;
                GamesView.RefreshView();
                if (GamesView.CollectionView.Count > 0)
                {
                    SelectGame((GamesView.CollectionView.GetItemAt(0) as GamesCollectionViewEntry).Id);
                }
            }
        }

        private void RenameFilterPreset(FilterPreset preset)
        {
            if (preset == null)
            {
                return;
            }

            var options = new List<MessageBoxToggle>
            {
                new MessageBoxToggle(LOC.FilterPresetShowOnFSTopPanel, preset.ShowInFullscreeQuickSelection)
            };

            var res = Dialogs.SelectString(LOC.EnterName, string.Empty, preset.Name, options);
            if (res.Result && !res.SelectedString.IsNullOrEmpty())
            {
                preset.Name = res.SelectedString;
                preset.ShowInFullscreeQuickSelection = options[0].Selected;
                Database.FilterPresets.Update(preset);
            }

            OnPropertyChanged(nameof(SortedFilterPresets));
            OnPropertyChanged(nameof(SortedFilterFullscreenPresets));
        }

        private void RemoveFilterPreset(FilterPreset preset)
        {
            if (preset == null)
            {
                return;
            }

            if (Dialogs.ShowMessage(LOC.AskRemoveItemMessage, LOC.AskRemoveItemTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Database.FilterPresets.Remove(preset);
                if (ActiveFilterPreset == preset)
                {
                    ActiveFilterPreset = null;
                }

                OnPropertyChanged(nameof(SortedFilterPresets));
                OnPropertyChanged(nameof(SortedFilterFullscreenPresets));
            }
        }

        private void AddFilterPreset()
        {
            var options = new List<MessageBoxToggle>
            {
                new MessageBoxToggle(LOC.FilterPresetSaveViewOptions, true),
                new MessageBoxToggle(LOC.FilterPresetShowOnFSTopPanel, false)
            };

            var overwriteExisting = false;
            var res = Dialogs.SelectString(LOC.EnterName, string.Empty, string.Empty, options);
            if (res.Result && !res.SelectedString.IsNullOrEmpty())
            {
                var existingPreset = Database.FilterPresets.FirstOrDefault(a => string.Equals(a.Name, res.SelectedString, StringComparison.InvariantCultureIgnoreCase));
                if (existingPreset != null)
                {
                    var dialogRes = Dialogs.ShowMessage(LOC.FilterPresetNameConflict, "", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (dialogRes == MessageBoxResult.Cancel)
                    {
                        return;
                    }

                    if (dialogRes == MessageBoxResult.Yes)
                    {
                        overwriteExisting = true;
                    }
                }

                var filter = App.Mode == ApplicationMode.Desktop ? AppSettings.FilterSettings : AppSettings.Fullscreen.FilterSettings;
                var preset = new FilterPreset
                {
                    Name = res.SelectedString,
                    Settings = filter.AsPresetSettings(),
                    ShowInFullscreeQuickSelection = options[1].Selected
                };

                if (options[0].Selected)
                {
                    var view = App.Mode == ApplicationMode.Desktop ? AppSettings.ViewSettings : (ViewSettingsBase)AppSettings.Fullscreen.ViewSettings;
                    preset.SortingOrder = view.SortingOrder;
                    preset.SortingOrderDirection = view.SortingOrderDirection;
                    if (App.Mode == ApplicationMode.Desktop)
                    {
                        preset.GroupingOrder = AppSettings.ViewSettings.GroupingOrder;
                    }
                }

                if (existingPreset != null && overwriteExisting)
                {
                    preset.Id = existingPreset.Id;
                    Database.FilterPresets.Update(preset);
                    ActiveFilterPreset = existingPreset;
                }
                else
                {
                    Database.FilterPresets.Add(preset);
                    ActiveFilterPreset = preset;
                }

                OnPropertyChanged(nameof(SortedFilterPresets));
                OnPropertyChanged(nameof(SortedFilterFullscreenPresets));
            }
        }

        private void OpenUpdates()
        {
            new UpdateViewModel(
                new Updater(App),
                new UpdateWindowFactory(),
                new ResourceProvider(),
                Dialogs,
                App.Mode).OpenView();
        }

        public abstract NotificationMessage GetAddonUpdatesFoundMessage(List<AddonUpdate> updates);

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
                ProgressActive = true;
                ProgressValue = 0;
                ProgressTotal = 1;
                ProgressStatus = Resources.GetString(LOC.AddonLookingForUpdates);

                try
                {
                    var updates = Addons.CheckAddonUpdates(App.ServicesClient);
                    if (updates.HasItems())
                    {
                        App.Notifications.Add(GetAddonUpdatesFoundMessage(updates));
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    Logger.Error(e, "Failed to check for addon updates.");
                }
                finally
                {
                    ProgressActive = false;
                }
            });

            await GlobalTaskHandler.ProgressTask;
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
                addedGames.AddRange(Database.ImportGames(plugin, token, AppSettings.PlaytimeImportMode));
                App.Notifications.Remove($"{plugin.Id} - download");
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                Logger.Error(e, $"Failed to import games from plugin: {plugin.Name}");
                App.Notifications.Add(new NotificationMessage(
                    $"{plugin.Id} - download",
                    Resources.GetString(LOC.LibraryImportError).Format(plugin.Name) + $"\n{e.Message}",
                    NotificationType.Error));
            }

            return addedGames;
        }

        public async Task ProcessStartupLibUpdate()
        {
            if (App.CmdLine.SkipLibUpdate)
            {
                Logger.Warn("Startup library update disabled via cmdline.");
                return;
            }

            if (!await Common.Network.GetIsConnectedToInternet())
            {
                Logger.Warn("Startup library update disabled because of no internet connection.");
                return;
            }

            var updateLibs = AppSettings.ShouldCheckLibraryOnStartup();
            var updateEmu = AppSettings.ShouldCheckEmuLibraryOnStartup();
            if (!updateLibs && !updateEmu)
            {
                return;
            }

            await UpdateLibrary(AppSettings.DownloadMetadataOnImport, updateLibs, updateEmu);
        }

        public async Task UpdateLibrary(bool metaForNewGames, bool updateIntegrations, bool updateEmu)
        {
            if (!GameAdditionAllowed)
            {
                return;
            }

            await UpdateLibraryData((token) =>
            {
                var addedGames = new List<Game>();
                if (updateIntegrations)
                {
                    foreach (var plugin in Extensions.LibraryPlugins)
                    {
                        if (token.IsCancellationRequested)
                        {
                            return addedGames;
                        }

                        addedGames.AddRange(ImportLibraryGames(plugin, token));
                    }

                    AppSettings.LastLibraryUpdateCheck = DateTimes.Now;
                }

                if (updateEmu)
                {
                    foreach (var scanConfig in Database.GameScanners.Where(a => a.InGlobalUpdate).ToList())
                    {
                        if (token.IsCancellationRequested)
                        {
                            return addedGames;
                        }

                        addedGames.AddRange(ImportEmulatedGames(scanConfig, token));
                    }

                    AppSettings.LastEmuLibraryUpdateCheck = DateTimes.Now;
                }

                if (AppSettings.ScanLibInstallSizeOnLibUpdate)
                {
                    UpdateGamesInstallSizes(token, Database.Games, LOC.ProgressScanningGamesInstallSize);
                }

                return addedGames;
            }, metaForNewGames);
        }

        public async Task UpdateLibrary(LibraryPlugin plugin)
        {
            if (!GameAdditionAllowed)
            {
                return;
            }

            await UpdateLibraryData((token) =>
            {
                var addedGames = ImportLibraryGames(plugin, token);
                if (AppSettings.ScanLibInstallSizeOnLibUpdate)
                {
                    UpdateGamesInstallSizes(token, Database.Games, LOC.ProgressScanningGamesInstallSize);
                }

                return addedGames;
            }, AppSettings.DownloadMetadataOnImport);
        }

        public void UpdateGamesInstallSizes(CancellationToken token, IEnumerable<Game> games, string progressMessageLocKey)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            try
            {
                ProgressActive = true;
                ProgressValue = 0;
                ProgressTotal = games.Count() + 1;

                Logger.Info($"Starting Library Install Size scan");
                ProgressStatus = Resources.GetString(progressMessageLocKey);
                var errorStrings = new List<string>();
                var errorsCount = 0;
                using (Database.Games.BufferedUpdate())
                {
                    foreach (var game in games)
                    {
                        if (token.IsCancellationRequested)
                        {
                            Logger.Info($"Library Install Size scan was cancelled");
                            break;
                        }

                        try
                        {
                            App.GamesEditor.UpdateGameSize(game, false, true, true);
                        }
                        catch (Exception e)
                        {
                            errorsCount++;
                            if (errorStrings.Count < 10)
                            {
                                errorStrings.Add($"{game.Name}: {e.Message}");
                            }
                        }

                        ProgressValue++;
                    }
                }

                Logger.Info($"Finished Library Install Size scan");
                if (errorsCount > 0)
                {
                    var errorMessage = ResourceProvider.GetString("LOCCalculateGamesSizeErrorMessage").Format(errorsCount)
                        + $"\n\n" + string.Join("\n", errorStrings);
                    if (errorsCount > 10)
                    {
                        errorMessage += "\n...";
                    }

                    App.Notifications.Add(new NotificationMessage(
                            $"LibUpdateScanSizeError - {DateTime.Now}",
                            ResourceProvider.GetString("LOCCalculateGamesSizeErrorMessage").Format(errorsCount),
                            NotificationType.Error,
                            () =>
                            {
                                Dialogs.ShowMessage(
                                    errorMessage,
                                    Resources.GetString("LOCCalculateGameSizeErrorCaption"),
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        )
                    );
                }
            }
            finally
            {
                ProgressActive = false;
            }
        }

        private List<Game> ImportEmulatedGames(GameScannerConfig scanConfig, CancellationToken token)
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
                var scanned = new GameScanner(scanConfig, Database).Scan(
                    token,
                    out var newPlatforms,
                    out var newRegions).Select(a => a.ToGame()).ToList();
                if (scanned.HasItems())
                {
                    var statusSettings = Database.GetCompletionStatusSettings();
                    if (newPlatforms.HasItems())
                    {
                        Database.Platforms.Add(newPlatforms);
                    }

                    if (newRegions.HasItems())
                    {
                        Database.Regions.Add(newRegions);
                    }

                    if (statusSettings.DefaultStatus != Guid.Empty)
                    {
                        scanned.ForEach(g => g.CompletionStatusId = statusSettings.DefaultStatus);
                    }

                    addedGames.AddRange(scanned);
                    Database.Games.Add(scanned);
                }

                App.Notifications.Remove($"{scanConfig.Id} - import");
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                Logger.Error(e, $"Failed to import emulated games from config:\n{scanConfig.Directory}\n{scanConfig.EmulatorId}\n{scanConfig.EmulatorProfileId}");
                App.Notifications.Add(new NotificationMessage(
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
                var addedGames = ImportEmulatedGames(config, token);
                if (AppSettings.ScanLibInstallSizeOnLibUpdate)
                {
                    UpdateGamesInstallSizes(token, addedGames, LOC.ProgressScanningImportedGamesInstallSize);
                }

                return addedGames;
            }, AppSettings.DownloadMetadataOnImport);
        }

        public async Task UpdateEmulationLibrary()
        {
            await UpdateLibraryData((token) =>
            {
                var addedGames = new List<Game>();
                foreach (var scanConfig in Database.GameScanners.Where(a => a.InGlobalUpdate))
                {
                    addedGames.AddRange(ImportEmulatedGames(scanConfig, token));
                }

                if (AppSettings.ScanLibInstallSizeOnLibUpdate)
                {
                    UpdateGamesInstallSizes(token, addedGames, LOC.ProgressScanningImportedGamesInstallSize);
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
                    ProgressActive = true;
                    ProgressValue = 0;
                    ProgressTotal = 1;

                    var addedGames = updateAction(GlobalTaskHandler.CancelToken.Token);
                    if (GlobalTaskHandler.CancelToken.IsCancellationRequested)
                    {
                        return;
                    }

                    ProgressActive = true;
                    ProgressStatus = Resources.GetString(LOC.ProgressLibImportFinish);
                    Thread.Sleep(1000);
                    if (addedGames.Any() && downloadMetadata)
                    {
                        Logger.Info($"Downloading metadata for {addedGames.Count} new games.");
                        ProgressValue = 0;
                        ProgressTotal = addedGames.Count;
                        string progressBaseStr = ProgressStatus = Resources.GetString(LOC.ProgressMetadata);
                        using (var downloader = new MetadataDownloader(Database, Extensions.MetadataPlugins, Extensions.LibraryPlugins))
                        {
                            downloader.DownloadMetadataAsync(addedGames, AppSettings.MetadataSettings, AppSettings,
                                (g, i, t) =>
                                {
                                    ProgressValue = i + 1;
                                    ProgressStatus = $"{progressBaseStr} [{ProgressValue}/{ProgressTotal}]";
                                },
                                GlobalTaskHandler.CancelToken.Token).Wait();
                        }
                    }

                    if (addedGames.Any() && AppSettings.GameSortingNameAutofill)
                    {
                        Logger.Info($"Setting Sorting Name for {addedGames.Count} new games.");
                        ProgressStatus = Resources.GetString(LOC.SortingNameAutofillProgress);
                        var c = new SortableNameConverter(AppSettings.GameSortingNameRemovedArticles, batchOperation: addedGames.Count > 20);
                        using (Database.BufferedUpdate())
                        {
                            foreach (var game in addedGames)
                            {
                                if (GlobalTaskHandler.CancelToken.IsCancellationRequested)
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
                });

                await GlobalTaskHandler.ProgressTask;
                Extensions.NotifiyOnLibraryUpdated();
            }
            finally
            {
                GameAdditionAllowed = true;
                ProgressActive = false;
                DatabaseFilters.IgnoreDatabaseUpdates = false;
            }
        }

        public async void CancelProgress()
        {
            await GlobalTaskHandler.CancelAndWaitAsync();
        }

        public virtual void SelectGame(Guid id, bool restoreView = false)
        {
        }

        private void RunAppScript(string script, string eventName)
        {
            if (script.IsNullOrWhiteSpace())
            {
                return;
            }

            try
            {
                if (!PowerShellRuntime.IsInstalled)
                {
                    throw new Exception(ResourceProvider.GetString(LOC.ErrorPowerShellNotInstalled));
                }

                using (var runtime = new PowerShellRuntime($"app {eventName} script"))
                {
                    runtime.Execute(
                        script,
                        PlaynitePaths.ProgramPath,
                        new Dictionary<string, object> { { "PlayniteApi", App.PlayniteApiGlobal } });
                }
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                Logger.Error(exc, $"Failed to execute {eventName} script.");
                Logger.Debug(script);
                var message = ResourceProvider.GetString(LOC.ErrorApplicationScript) + Environment.NewLine + exc.Message;
                if (exc is ScriptRuntimeException scriptExc)
                {
                    message = message + Environment.NewLine + Environment.NewLine + scriptExc.ScriptStackTrace;
                }

                Dialogs.ShowMessage(
                    message,
                    "",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public void RunStartupScript()
        {
            if (!App.CmdLine.SafeStartup)
            {
                RunAppScript(AppSettings.AppStartupScript, "startup");
            }
        }

        public void RunShutdowScript()
        {
            RunAppScript(AppSettings.AppShutdownScript, "shutdown");
        }

        public void RestartAppSafe()
        {
            CloseView();
            App.Restart(new CmdLineOptions { SafeStartup = true });
        }

        public abstract void CloseView();
        public virtual IEnumerable<SearchItem> GetSearchCommands()
        {
            yield break;
        }

        public abstract void OpenSettings(int settingsPageIndex);
        public void StartGame(Game game)
        {
            App.GamesEditor.PlayGame(game);
        }

        public void InstallGame(Game game)
        {
            App.GamesEditor.InstallGame(game);
        }

        public abstract void EditGame(Game game);
        public abstract void AssignCategories(Game game);

        public void StartSoftwareTool(AppSoftware app)
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

        private void RestoreDataBackup()
        {
            var backupFile = Dialogs.SelectFile("Playnite Backup|*.zip");
            if (backupFile.IsNullOrEmpty())
            {
                return;
            }

            List<BackupDataItem> restoreOptions = null;
            try
            {
                restoreOptions = Backup.GetRestoreSelections(backupFile);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Failed to read backup file {backupFile}");
                return;
            }

            var restoreItems = restoreOptions.Select(a => new SelectableNamedObject<BackupDataItem>(a, a.GetDescription(), true)).ToList();
            if (ItemSelector.SelectMultiple(
                LOC.MenuRestoreBackup,
                LOC.BackupRestoreMessage,
                restoreItems,
                out var selectedRestoreItems))
            {
                var options = new BackupRestoreOptions
                {
                    BackupFile = backupFile,
                    DataDir = PlaynitePaths.ConfigRootPath,
                    LibraryDir = GameDatabase.GetFullDbPath(AppSettings.DatabasePath),
                    RestoreItems = selectedRestoreItems,
                    RestoreLibrarySettingsPath = AppSettings.DatabasePath
                };

                FileSystem.WriteStringToFile(PlaynitePaths.RestoreBackupActionFile, Serialization.ToJson(options));
                App.Restart(new CmdLineOptions
                {
                    RestoreBackup = PlaynitePaths.RestoreBackupActionFile,
                    SkipLibUpdate = true
                });
            }
        }

        private void BackupData()
        {
            var backupFile = Dialogs.SaveFile("Playnite Backup|*.zip", true);
            if (backupFile.IsNullOrEmpty())
            {
                return;
            }

            List<BackupDataItem> backupOptions = new List<BackupDataItem> { BackupDataItem.LibraryFiles, BackupDataItem.Extensions, BackupDataItem.ExtensionsData, BackupDataItem.Themes };
            var restoreItems = backupOptions.Select(a => new SelectableNamedObject<BackupDataItem>(a, a.GetDescription(), true)).ToList();
            if (ItemSelector.SelectMultiple(
                LOC.MenuBackupData,
                LOC.BackupDataBackupMessage,
                restoreItems,
                out var selectedBackupItems))
            {
                var options = new BackupOptions
                {
                    OutputFile = backupFile,
                    DataDir = PlaynitePaths.ConfigRootPath,
                    LibraryDir = GameDatabase.GetFullDbPath(AppSettings.DatabasePath),
                    BackupItems = selectedBackupItems
                };

                FileSystem.WriteStringToFile(PlaynitePaths.BackupActionFile, Serialization.ToJson(options));
                App.Restart(new CmdLineOptions
                {
                    Backup = PlaynitePaths.BackupActionFile,
                    SkipLibUpdate = true
                });
            }
        }
    }
}
