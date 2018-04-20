using NLog;
using Playnite;
using Playnite.Database;
using Playnite.MetaProviders;
using Playnite.Providers.Steam;
using Playnite.SDK;
using Playnite.SDK.Models;
using PlayniteUI.Commands;
using PlayniteUI.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PlayniteUI.ViewModels
{
    public class MainViewModel : ObservableObject, IDisposable
    {
        public static Logger Logger = LogManager.GetCurrentClassLogger();
        private static object gamesLock = new object();
        protected bool ignoreCloseActions = false;
        private readonly SynchronizationContext context;
        
        public IWindowFactory Window;
        public IDialogsFactory Dialogs;
        public IResourceProvider Resources;
        public GameDatabase Database;
        public GamesEditor GamesEditor;
        public bool IsFullscreenView
        {
            get; protected set;
        } = false;

        private GameDetailsViewModel selectedGameDetails;
        public GameDetailsViewModel SelectedGameDetails
        {
            get => selectedGameDetails;
            set
            {
                selectedGameDetails = value;
                OnPropertyChanged("SelectedGameDetails");
            }
        }

        private GameViewEntry selectedGame;
        public GameViewEntry SelectedGame
        {
            get => selectedGame;
            set
            {
                if (value == null)
                {
                    SelectedGameDetails = null;
                }
                else
                {
                    SelectedGameDetails = new GameDetailsViewModel(value, AppSettings, GamesEditor, Dialogs, Resources);
                }

                selectedGame = value;
                OnPropertyChanged("SelectedGame");
            }
        }
        
        public IEnumerable<GameViewEntry> SelectedGames
        {
            get
            {
                if (selectedGamesBinder == null)
                {
                    return null;
                }

                return selectedGamesBinder.Cast<GameViewEntry>();
            }
        }

        private IList<object> selectedGamesBinder;
        public IList<object> SelectedGamesBinder
        {
            get => selectedGamesBinder;
            set
            {
                selectedGamesBinder = value;
                OnPropertyChanged("SelectedGamesBinder");
                OnPropertyChanged("SelectedGames");
            }
        }

        private GamesCollectionView gamesView;
        public GamesCollectionView GamesView
        {
            get => gamesView;
            set
            {
                gamesView = value;
                OnPropertyChanged("GamesView");
            }
        }

        private ObservableCollection<NotificationMessage> messages;
        public ObservableCollection<NotificationMessage> Messages
        {
            get => messages;
            set
            {
                messages = value;
                OnPropertyChanged("Messages");
            }
        }

        private ObservableCollection<ThirdPartyTool> thirdPartyTools;
        public ObservableCollection<ThirdPartyTool> ThirdPartyTools
        {
            get => thirdPartyTools;
            set
            {
                thirdPartyTools = value;
                OnPropertyChanged("ThirdPartyTools");
            }
        }

        private bool gameAdditionAllowed = true;
        public bool GameAdditionAllowed
        {
            get => gameAdditionAllowed;
            set
            {
                gameAdditionAllowed = value;
                OnPropertyChanged("GameAdditionAllowed");
            }
        }

        private string progressStatus;
        public string ProgressStatus
        {
            get => progressStatus;
            set
            {
                progressStatus = value;
                context.Post((a) => OnPropertyChanged("ProgressStatus"), null);
            }
        }

        private double progressValue;
        public double ProgressValue
        {
            get => progressValue;
            set
            {
                progressValue = value;
                context.Post((a) => OnPropertyChanged("ProgressValue"), null);
            }
        }

        private double progressTotal;
        public double ProgressTotal
        {
            get => progressTotal;
            set
            {
                progressTotal = value;
                context.Post((a) => OnPropertyChanged("ProgressTotal"), null);
            }
        }

        private bool progressVisible = false;
        public bool ProgressVisible
        {
            get => progressVisible;
            set
            {
                progressVisible = value;
                context.Post((a) => OnPropertyChanged("ProgressVisible"), null );
            }
        }

        private bool showGameSidebar = false;
        public bool ShowGameSidebar
        {
            get => showGameSidebar;
            set
            {
                showGameSidebar = value;
                OnPropertyChanged("ShowGameSidebar");
            }
        }

        private bool mainMenuOpened = false;
        public bool MainMenuOpened
        {
            get => mainMenuOpened;
            set
            {
                mainMenuOpened = value;
                OnPropertyChanged("MainMenuOpened");
            }
        }

        private Visibility visibility = Visibility.Visible;
        public Visibility Visibility
        {
            get => visibility;
            set
            {
                visibility = value;
                OnPropertyChanged("Visibility");
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
                }

                windowState = value;
                OnPropertyChanged("WindowState");
            }
        }

        private Settings appSettings;
        public Settings AppSettings
        {
            get => appSettings;
            private set
            {
                appSettings = value;
                OnPropertyChanged("AppSettings");
            }
        }

        private DatabaseStats gamesStats;
        public DatabaseStats GamesStats
        {
            get => gamesStats;
            private set
            {
                gamesStats = value;
                OnPropertyChanged("GamesStats");
            }
        }

        public RelayCommand<object> OpenFilterPanelCommand
        {
            get => new RelayCommand<object>((game) =>
            {
                AppSettings.FilterPanelVisible = true;
            });
        }

        public RelayCommand<object> CloseFilterPanelCommand
        {
            get => new RelayCommand<object>((game) =>
            {
                AppSettings.FilterPanelVisible = false;
            });
        }

        public RelayCommand<object> OpenMainMenuCommand
        {
            get => new RelayCommand<object>((game) =>
            {
                MainMenuOpened = true;
            });
        }

        public RelayCommand<object> CloseMainMenuCommand
        {
            get => new RelayCommand<object>((game) =>
            {
                MainMenuOpened = false;
            });
        }

        public RelayCommand<Game> StartGameCommand
        {
            get => new RelayCommand<Game>((game) =>
            {
                if (game != null)
                {
                    GamesEditor.PlayGame(game);
                }
                else if (SelectedGame != null)
                {
                    GamesEditor.PlayGame(SelectedGame.Game);
                }
            });
        }

        public RelayCommand<ThirdPartyTool> ThridPartyToolOpenCommand
        {
            get => new RelayCommand<ThirdPartyTool>((tool) =>
            {
                MainMenuOpened = false;
                StartThirdPartyTool(tool);
            });
        }

        public RelayCommand<object> UpdateGamesCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                MainMenuOpened = false;
                UpdateDatabase(true);
            }, (a) => GameAdditionAllowed || !Database.IsOpen);
        }

        public RelayCommand<object> OpenSteamFriendsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                OpenSteamFriends();
            });
        }

        public RelayCommand<object> ReportIssueCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                MainMenuOpened = false;
                ReportIssue();
            });
        }

        public RelayCommand<object> ShutdownCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                MainMenuOpened = false;
                ignoreCloseActions = true;
                ShutdownApp();
            });
        }

        public RelayCommand<object> ShowWindowCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RestoreWindow();
            });
        }

        public RelayCommand<CancelEventArgs> WindowClosingCommand
        {
            get => new RelayCommand<CancelEventArgs>((args) =>
            {
                OnClosing(args);
            });
        }

        public RelayCommand<DragEventArgs> FileDroppedCommand
        {
            get => new RelayCommand<DragEventArgs>((args) =>
            {
                OnFileDropped(args);
            });
        }

        public RelayCommand<object> OpenAboutCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                MainMenuOpened = false;
                OpenAboutWindow(new AboutViewModel(AboutWindowFactory.Instance, Dialogs, Resources));
            });
        }

        public RelayCommand<object> OpenPlatformsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                MainMenuOpened = false;
                ConfigurePlatforms(
                    new PlatformsViewModel(Database,
                    PlatformsWindowFactory.Instance,
                    Dialogs,
                    Resources));
            }, (a) => Database.IsOpen);
        }

        public RelayCommand<object> OpenSettingsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                MainMenuOpened = false;
                OpenSettings(
                    new SettingsViewModel(Database,
                    AppSettings,
                    SettingsWindowFactory.Instance,
                    Dialogs,
                    Resources));
            });
        }

        public RelayCommand<object> AddCustomGameCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                MainMenuOpened = false;
                AddCustomGame(GameEditWindowFactory.Instance);
            }, (a) => Database.IsOpen);
        }

        public RelayCommand<object> AddInstalledGamesCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                MainMenuOpened = false;
                ImportInstalledGames(
                    new InstalledGamesViewModel(
                    Database,
                    InstalledGamesWindowFactory.Instance,
                    Dialogs));
            }, (a) => Database.IsOpen);
        }

        public RelayCommand<object> AddEmulatedGamesCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                MainMenuOpened = false;
                ImportEmulatedGames(
                    new EmulatorImportViewModel(Database,
                    EmulatorImportViewModel.DialogType.GameImport,
                    EmulatorImportWindowFactory.Instance,
                    Dialogs,
                    Resources));
            }, (a) => Database.IsOpen);
        }

        public RelayCommand<bool> OpenThemeTesterCommand
        {
            get => new RelayCommand<bool>((fullscreen) =>
            {
                var window = new ThemeTesterWindow();
                window.SkinType = fullscreen ? ThemeTesterWindow.SourceType.Fullscreen : ThemeTesterWindow.SourceType.Normal;
                window.Show();
            });
        }

        public RelayCommand<object> OpenFullScreenCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                OpenFullScreen();
            });
        }

        public RelayCommand<object> CancelProgressCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CancelProgress();
            }, (a) => !GlobalTaskHandler.CancelToken.IsCancellationRequested);
        }

        public RelayCommand<object> ClearMessagesCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ClearMessages();
            }, (a) => Messages.Count > 0);
        }

        public RelayCommand<object> DownloadMetadataCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                MainMenuOpened = false;
                DownloadMetadata(new MetadataDownloadViewModel(MetadataDownloadWindowFactory.Instance));
            }, (a) => GameAdditionAllowed);
        }

        public RelayCommand<object> ClearFiltersCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ClearFilters();
            });
        }

        public RelayCommand<object> RemoveGameSelectionCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RemoveGameSelection();
            });
        }

        public RelayCommand<ExtensionFunction> InvokeExtensionFunctionCommand
        {
            get => new RelayCommand<ExtensionFunction>((f) =>
            {
                MainMenuOpened = false;
                App.CurrentApp.Api?.InvokeExtension(f);
            });
        }

        public RelayCommand<object> ReloadScriptsCommand
        {
            get => new RelayCommand<object>((f) =>
            {
                MainMenuOpened = false;
                App.CurrentApp.Api?.LoadScripts();
            });
        }

        public RelayCommand<GameViewEntry> ShowGameSideBarCommand
        {
            get => new RelayCommand<GameViewEntry>((f) =>
            {
                SelectedGame = f;
                ShowGameSidebar = true;
            });
        }

        public RelayCommand<object> CloseGameSideBarCommand
        {
            get => new RelayCommand<object>((f) =>
            {
                ShowGameSidebar = false;
            });
        }

        public MainViewModel(
            GameDatabase database,
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            Settings settings,
            GamesEditor gamesEditor)
        {
            context = SynchronizationContext.Current;
            this.Window = window;
            this.Dialogs = dialogs;
            this.Resources = resources;
            this.Database = database;
            this.GamesEditor = gamesEditor;
            Messages = new ObservableCollection<NotificationMessage>();
            AppSettings = settings;

            try
            {
                ThirdPartyTools = new ObservableCollection<ThirdPartyTool>(ThirdPartyToolsList.GetDefaultInstalledTools());
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                Logger.Error(e, "failed to load third party tools");
            }

            AppSettings.PropertyChanged += AppSettings_PropertyChanged;
            AppSettings.FilterSettings.PropertyChanged += FilterSettings_PropertyChanged;
            GamesStats = new DatabaseStats(database);
        }

        private void FilterSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Active")
            {
                AppSettings.SaveSettings();

                if (e.PropertyName != "Name")
                {
                    AppSettings.FilterPanelVisible = true;
                }
            }
        }

        private void AppSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Language")
            {
                Localization.SetLanguage(AppSettings.Language);
            }

            AppSettings.SaveSettings();
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
            }
        }

        public void RemoveGameSelection()
        {
            SelectedGame = null;
            SelectedGamesBinder = null;
        }

        public void OpenSteamFriends()
        {
            System.Diagnostics.Process.Start(@"steam://open/friends");
        }

        public void ReportIssue()
        {
            System.Diagnostics.Process.Start(@"https://github.com/JosefNemec/Playnite/issues/new");
        }

        public void ShutdownApp()
        {
            App.CurrentApp.Quit();
        }

        protected void InitializeView()
        {
            try
            {
                if (GameDatabase.GetMigrationRequired(AppSettings.DatabasePath))
                {
                    var progressModel = new ProgressViewViewModel(new ProgressWindowFactory(),
                    () =>
                    {
                        try
                        {
                            GameDatabase.MigrateDatabase(AppSettings.DatabasePath);
                        }
                        catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                        {
                            Logger.Error(exc, "Failed to migrate database to new version.");
                            throw;
                        }
                    })
                    {
                        ProgressText = Resources.FindString("LOCDBUpgradeProgress")
                    };

                    if (progressModel.ActivateProgress() == false)
                    {
                        Dialogs.ShowMessage(Resources.FindString("LOCDBUpgradeFail"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                        GameAdditionAllowed = true;
                        return;
                    }
                }

                if (!Database.IsOpen)
                {
                    Database.OpenDatabase();
                }
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                Logger.Error(exc, "Failed to open database.");
                var message = Resources.FindString("LOCDatabaseOpenError") + $" {exc.Message}";
                if (exc is System.IO.IOException || exc is UnauthorizedAccessException)
                {
                    message = string.Format(Resources.FindString("LOCDatabaseOpenAccessError"), AppSettings.DatabasePath);
                }

                Dialogs.ShowMessage(
                        message,
                        Resources.FindString("LOCDatabaseErroTitle"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                GameAdditionAllowed = false;
                return;
            }

            GamesView = new GamesCollectionView(Database, AppSettings, IsFullscreenView);
            BindingOperations.EnableCollectionSynchronization(GamesView.Items, gamesLock);
            if (GamesView.CollectionView.Count > 0)
            {
                SelectGame((GamesView.CollectionView.GetItemAt(0) as GameViewEntry).ProviderId);
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
        }

        public async void UpdateDatabase(bool updateLibrary)
        {
            await UpdateDatabase(updateLibrary, 0, true);
        }

        public async Task UpdateDatabase(bool updateLibrary, ulong steamImportCatId, bool metaForNewGames)
        {
            if (!Database.IsOpen)
            {
                throw new Exception("Cannot load new games, database is not loaded.");
            }

            if (GlobalTaskHandler.ProgressTask != null && GlobalTaskHandler.ProgressTask.Status == TaskStatus.Running)
            {
                GlobalTaskHandler.CancelToken.Cancel();
                await GlobalTaskHandler.ProgressTask;
            }

            GameAdditionAllowed = false;           

            try
            {
                if (!updateLibrary)
                {
                    return;
                }

                GlobalTaskHandler.CancelToken = new CancellationTokenSource();
                GlobalTaskHandler.ProgressTask = Task.Factory.StartNew(() =>
                {
                    var addedGames = new List<Game>();
                    ProgressVisible = true;
                    ProgressValue = 0;
                    ProgressTotal = 1;
                    ProgressStatus = Resources.FindString("LOCProgressInstalledGames");

                    try
                    {
                        if (AppSettings.BattleNetSettings.IntegrationEnabled)
                        {
                            addedGames.AddRange(Database.UpdateInstalledGames(Provider.BattleNet));
                            RemoveMessage(NotificationCodes.BattleNetInstalledImportError);
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        Logger.Error(e, "Failed to import installed Battle.net games.");
                        AddMessage(new NotificationMessage(
                            NotificationCodes.BattleNetInstalledImportError,
                            Resources.FindString("LOCBnetInstalledImportError") + $" {e.Message}",
                            NotificationType.Error, null));
                    }

                    try
                    {
                        if (AppSettings.UplaySettings.IntegrationEnabled)
                        {
                            addedGames.AddRange(Database.UpdateInstalledGames(Provider.Uplay));
                            RemoveMessage(NotificationCodes.UplayInstalledImportError);
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        Logger.Error(e, "Failed to import installed Uplay games.");
                        AddMessage(new NotificationMessage(
                            NotificationCodes.UplayInstalledImportError,
                            Resources.FindString("LOCUplayInstalledImportError") + $" {e.Message}",
                            NotificationType.Error, null));
                    }

                    try
                    {
                        if (AppSettings.GOGSettings.IntegrationEnabled)
                        {
                            addedGames.AddRange(Database.UpdateInstalledGames(Provider.GOG));
                            RemoveMessage(NotificationCodes.GOGLInstalledImportError);
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        Logger.Error(e, "Failed to import installed GOG games.");
                        AddMessage(new NotificationMessage(
                            NotificationCodes.GOGLInstalledImportError,
                            Resources.FindString("LOCGOGInstalledImportError") + $" {e.Message}",
                            NotificationType.Error, null));
                    }

                    try
                    {
                        if (AppSettings.SteamSettings.IntegrationEnabled)
                        {
                            addedGames.AddRange(Database.UpdateInstalledGames(Provider.Steam));
                            RemoveMessage(NotificationCodes.SteamInstalledImportError);
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        Logger.Error(e, "Failed to import installed Steam games.");
                        AddMessage(new NotificationMessage(
                            NotificationCodes.SteamInstalledImportError,
                            Resources.FindString("LOCSteamInstalledImportError") + $" {e.Message}",
                            NotificationType.Error, null));
                    }

                    try
                    {
                        if (AppSettings.OriginSettings.IntegrationEnabled)
                        {
                            addedGames.AddRange(Database.UpdateInstalledGames(Provider.Origin));
                            RemoveMessage(NotificationCodes.OriginInstalledImportError);
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        Logger.Error(e, "Failed to import installed Origin games.");
                        AddMessage(new NotificationMessage(
                            NotificationCodes.OriginInstalledImportError,
                            Resources.FindString("LOCOriginInstalledImportError") + $" {e.Message}",
                            NotificationType.Error, null));
                    }

                    ProgressStatus = Resources.FindString("LOCProgressGOGLibImport");

                    try
                    {
                        if (AppSettings.GOGSettings.IntegrationEnabled && AppSettings.GOGSettings.LibraryDownloadEnabled)
                        {
                            addedGames.AddRange(Database.UpdateOwnedGames(Provider.GOG));
                            RemoveMessage(NotificationCodes.GOGLibDownloadError);
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        Logger.Error(e, "Failed to download GOG library updates.");
                        AddMessage(new NotificationMessage(
                            NotificationCodes.GOGLibDownloadError,
                            Resources.FindString("LOCGOGLibraryImportError") + $" {e.Message}",
                            NotificationType.Error, null));
                    }

                    ProgressStatus = Resources.FindString("LOCProgressSteamLibImport");

                    try
                    {
                        if (AppSettings.SteamSettings.IntegrationEnabled && AppSettings.SteamSettings.LibraryDownloadEnabled)
                        {
                            addedGames.AddRange(Database.UpdateOwnedGames(Provider.Steam));
                            RemoveMessage(NotificationCodes.SteamLibDownloadError);
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        Logger.Error(e, "Failed to download Steam library updates.");
                        AddMessage(new NotificationMessage(
                            NotificationCodes.SteamLibDownloadError,
                            Resources.FindString("LOCSteamLibraryImportError") + $" {e.Message}",
                            NotificationType.Error, null));
                    }
                                        
                    if (steamImportCatId > 0)
                    {
                        ProgressStatus = Resources.FindString("LOCProgressSteamCategoryImport");

                        try
                        {
                            var steamLib = new SteamLibrary();
                            Database.ImportCategories(steamLib.GetCategorizedGames(steamImportCatId));
                        }
                        catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                        {
                            Logger.Error(e, "Failed to import Steam categories.");
                            AddMessage(new NotificationMessage(
                                NotificationCodes.SteamCatImportError,
                                Resources.FindString("LOCSteamCategoryImportError") + $" {e.Message}",
                                NotificationType.Error, null));
                        }
                    }

                    ProgressStatus = Resources.FindString("LOCProgressOriginLibImport");

                    try
                    {
                        if (AppSettings.OriginSettings.IntegrationEnabled && AppSettings.OriginSettings.LibraryDownloadEnabled)
                        {
                            addedGames.AddRange(Database.UpdateOwnedGames(Provider.Origin));
                            RemoveMessage(NotificationCodes.OriginLibDownloadError);
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        Logger.Error(e, "Failed to download Origin library updates.");
                        AddMessage(new NotificationMessage(
                            NotificationCodes.OriginLibDownloadError,
                            Resources.FindString("LOCOriginLibraryImportError") + $" {e.Message}",
                            NotificationType.Error, null));
                    }

                    ProgressStatus = Resources.FindString("LOCProgressBattleNetLibImport");

                    try
                    {
                        if (AppSettings.BattleNetSettings.IntegrationEnabled && AppSettings.BattleNetSettings.LibraryDownloadEnabled)
                        {
                            addedGames.AddRange(Database.UpdateOwnedGames(Provider.BattleNet));
                            RemoveMessage(NotificationCodes.BattleNetLibDownloadImportError);
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        Logger.Error(e, "Failed to download Battle.net library updates.");
                        AddMessage(new NotificationMessage(
                            NotificationCodes.BattleNetLibDownloadImportError,
                            Resources.FindString("LOCBnetLibraryImportError") + $" {e.Message}",
                            NotificationType.Error, null));
                    }
                                        
                    ProgressStatus = Resources.FindString("LOCProgressLibImportFinish");
                    Thread.Sleep(1500);

                    if (addedGames.Any() && metaForNewGames)
                    {
                        Logger.Info($"Downloading metadata for {addedGames.Count} new games.");
                        ProgressValue = 0;
                        ProgressTotal = addedGames.Count;
                        ProgressStatus = Resources.FindString("LOCProgressMetadata");
                        var metaSettings = new MetadataDownloaderSettings();
                        metaSettings.ConfigureFields(MetadataSource.Store, true);
                        metaSettings.CoverImage.Source = MetadataSource.IGDBOverStore;
                        metaSettings.Name = new MetadataFieldSettings(true, MetadataSource.Store);
                        var downloader = new MetadataDownloader();
                        downloader.DownloadMetadataThreaded(
                            addedGames,
                            Database,
                            metaSettings,
                            (g, i, t) => ProgressValue = i + 1,
                            GlobalTaskHandler.CancelToken).Wait();
                    }
                });

                await GlobalTaskHandler.ProgressTask;
            }
            finally
            {
                GameAdditionAllowed = true;
                ProgressVisible = false;
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
                games = Database.GamesCollection.FindAll().ToList();
            }
            else if (settings.GamesSource == MetadataGamesSource.Filtered)
            {
                games = GamesView.CollectionView.Cast<GameViewEntry>().Select(a => a.Game).Distinct().ToList();
            }

            GameAdditionAllowed = false;

            try
            {
                if (GlobalTaskHandler.ProgressTask != null && GlobalTaskHandler.ProgressTask.Status == TaskStatus.Running)
                {
                    GlobalTaskHandler.CancelToken.Cancel();
                    await GlobalTaskHandler.ProgressTask;
                }

                GlobalTaskHandler.CancelToken = new CancellationTokenSource();
                ProgressVisible = true;
                ProgressValue = 0;
                ProgressTotal = games.Count;
                ProgressStatus = Resources.FindString("LOCProgressMetadata");
                var downloader = new MetadataDownloader();
                GlobalTaskHandler.ProgressTask =
                    downloader.DownloadMetadataThreaded(games, Database, settings, (g, i, t) => ProgressValue = i + 1, GlobalTaskHandler.CancelToken);
                await GlobalTaskHandler.ProgressTask;
            }
            finally
            {
                ProgressVisible = false;
                GameAdditionAllowed = true;
            }
        }


        public async void DownloadMetadata(MetadataDownloadViewModel model)
        {
            if (model.OpenView(MetadataDownloadViewModel.ViewMode.Manual) != true)
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
                Provider = Provider.Custom
            };

            Database.AddGame(newGame);
            if (GamesEditor.EditGame(newGame) == true)
            {
                var viewEntry = GamesView.Items.First(a => a.Game.ProviderId == newGame.ProviderId);
                SelectedGame = viewEntry;
            }
            else
            {
                Database.DeleteGame(newGame);
            }
        }

        public void ImportInstalledGames(InstalledGamesViewModel model)
        {
            model.OpenView();
        }

        public void ImportEmulatedGames(EmulatorImportViewModel model)
        {
            model.OpenView();
        }
        
        public void OpenAboutWindow(AboutViewModel model)
        {
            model.OpenView();
        }

        public void OpenSettings(SettingsViewModel model)
        {
            var currentSkin = Themes.CurrentTheme;
            var currentColor = Themes.CurrentColor;

            if (model.OpenView() == true)
            {
                if (model.ProviderIntegrationChanged)
                {
                    UpdateDatabase(true);
                }
            }
            else
            {
                if (Themes.CurrentTheme != currentSkin || Themes.CurrentColor != currentColor)
                {
                    Themes.ApplyTheme(currentSkin, currentColor);
                }
            }
        }

        public void ConfigurePlatforms(PlatformsViewModel model)
        {
            model.OpenView();
        }

        public void SetViewType(ViewType type)
        {
            AppSettings.GamesViewType = type;
        }

        public void SelectGame(string providerId)
        {
            var viewEntry = GamesView.Items.FirstOrDefault(a => a.Game.ProviderId == providerId);
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
            }
            else
            {
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
                    Window.BringToForeground();

                    var path = files[0];
                    if (File.Exists(path))
                    {
                        // Other file types to be added in #501
                        if (!(new List<string>() { ".exe", ".lnk" }).Contains(Path.GetExtension(path).ToLower()))
                        {
                            return;
                        }

                        var game = Programs.GetGameFromExecutable(path);
                        Database.AddGame(game);
                        Database.AssignPcPlatform(game);
                        GamesEditor.EditGame(game);
                        SelectGame(game.ProviderId);
                    }
                    else if (Directory.Exists(path))
                    {
                        var instMode = new InstalledGamesViewModel(
                           Database,
                           InstalledGamesWindowFactory.Instance,
                           Dialogs);

                        instMode.OpenView(path);
                    }
                }
            }
        }

        public void AddMessage(NotificationMessage message)
        {
            context.Send((c =>
            {
                if (!Messages.Any(a => a.Id == message.Id))
                {
                    Messages.Add(message);
                }
            }), null);
        }

        public void RemoveMessage(int id)
        {
            context.Send((c =>
            {
                var message = Messages.FirstOrDefault(a => a.Id == id);
                if (message != null)
                {
                    Messages.Remove(message);
                }
            }), null);
        }

        public void RemoveMessage(NotificationMessage message)
        {
            context.Send((c => Messages.Remove(message)), null);            
        }

        public void ClearMessages()
        {
            context.Send((c => Messages.Clear()), null);
        }

        public void OpenFullScreen()
        {
            CloseView();
            App.CurrentApp.OpenFullscreenView();            
        }

        public void OpenView()
        {
            Window.Show(this);
            Window.BringToForeground();
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
            GlobalTaskHandler.CancelToken.Cancel();
            await GlobalTaskHandler.ProgressTask;
        }        

        public virtual void ClearFilters()
        {
            AppSettings.FilterSettings.ClearFilters();
        }

        public virtual void Dispose()
        {
            GamesView.Dispose();
            GamesStats.Dispose();
            AppSettings.PropertyChanged -= AppSettings_PropertyChanged;
            AppSettings.FilterSettings.PropertyChanged -= FilterSettings_PropertyChanged;
        }
    }
}
