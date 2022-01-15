using Playnite.API;
using Playnite.Database;
using Playnite.Emulators;
using Playnite.Metadata;
using Playnite.Plugins;
using Playnite.SDK;
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
        GamesCollectionViewEntry SelectedGame { get; set; }
        IEnumerable<GamesCollectionViewEntry> SelectedGames { get; set; }
    }

    public abstract class MainViewModelBase : ObservableObject, IMainViewModelBase
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
        public GamesCollectionViewEntry SelectedGame { get; set; }
        public IEnumerable<GamesCollectionViewEntry> SelectedGames { get; set; }
        public RelayCommand<object> AddFilterPresetCommand { get; private set; }
        public RelayCommand<FilterPreset> RenameFilterPresetCommand { get; private set; }
        public RelayCommand<FilterPreset> RemoveFilterPresetCommand { get; private set; }
        public RelayCommand<FilterPreset> ApplyFilterPresetCommand { get; private set; }
        public RelayCommand CancelProgressCommand { get; private set; }
        public RelayCommand<object> OpenUpdatesCommand { get; private set; }
        public RelayCommand StartInteractivePowerShellCommand { get; private set; }

        public GameDatabase Database { get; }
        public PlayniteApplication App { get; }
        public IDialogsFactory Dialogs { get; }
        public PlayniteAPI PlayniteApi { get; set; }
        public IResourceProvider Resources { get; }
        public ExtensionFactory Extensions { get; set; }
        public bool IgnoreFilterChanges { get; set; } = false;

        public MainViewModelBase(
            GameDatabase database,
            PlayniteApplication app,
            IDialogsFactory dialogs,
            PlayniteAPI playniteApi,
            IResourceProvider resources,
            ExtensionFactory extensions)
        {
            Database = database;
            App = app;
            Dialogs = dialogs;
            PlayniteApi = playniteApi;
            Resources = resources;
            Extensions = extensions;

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
                        { "PlayniteApi", PlayniteApi }
                    });
                }
                catch (Exception e)
                {
                    Dialogs.ShowErrorMessage("Failed to start interactive PowerShell.\n" + e.Message);
                }
            });
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
                    Settings = filter.GetClone(),
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
                        PlayniteApi.Notifications.Add(GetAddonUpdatesFoundMessage(updates));
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
                addedGames.AddRange(Database.ImportGames(plugin, AppSettings.ForcePlayTimeSync, token));
                PlayniteApi.Notifications.Remove($"{plugin.Id} - download");
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                Logger.Error(e, $"Failed to import games from plugin: {plugin.Name}");
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    $"{plugin.Id} - download",
                    Resources.GetString(LOC.LibraryImportError).Format(plugin.Name) + $"\n{e.Message}",
                    NotificationType.Error));
            }

            return addedGames;
        }

        public async Task UpdateLibrary(bool metaForNewGames, bool updateEmu)
        {
            if (!GameAdditionAllowed)
            {
                return;
            }

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

                if (updateEmu)
                {
                    var importedRoms = Database.GetImportedRomFiles();
                    foreach (var scanConfig in Database.GameScanners.Where(a => a.InGlobalUpdate).ToList())
                    {
                        if (token.IsCancellationRequested)
                        {
                            return addedGames;
                        }

                        addedGames.AddRange(ImportEmulatedGames(scanConfig, importedRoms, token));
                    }
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
                var scanned = new GameScanner(scanConfig, Database, importedFiles).Scan(
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

                PlayniteApi.Notifications.Remove($"{scanConfig.Id} - import");
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                Logger.Error(e, $"Failed to import emulated games from config:\n{scanConfig.Directory}\n{scanConfig.EmulatorId}\n{scanConfig.EmulatorProfileId}");
                PlayniteApi.Notifications.Add(new NotificationMessage(
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
                    ProgressActive = true;
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

        public virtual void SelectGame(Guid id)
        {
        }
    }
}
