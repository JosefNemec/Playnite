using Playnite;
using Playnite.API;
using Playnite.App;
using Playnite.Common;
using Playnite.Common.System;
using Playnite.Database;
using Playnite.Metadata;
using Playnite.Plugins;
using Playnite.Scripting;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.Settings;
using PlayniteUI.Commands;
using PlayniteUI.Windows;
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

namespace PlayniteUI.ViewModels
{
    public class MainViewModel : ObservableObject, IDisposable
    {
        public static ILogger Logger = LogManager.GetLogger();
        private static object gamesLock = new object();
        protected bool ignoreCloseActions = false;
        private readonly SynchronizationContext context;

        public PlayniteAPI PlayniteApi { get; }
        public ExtensionFactory Extensions { get; }
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
                OnPropertyChanged();
            }
        }

        private GameViewEntry selectedGame;
        public GameViewEntry SelectedGame
        {
            get => selectedGame;
            set
            {
                if (value == selectedGame)
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
                    if (AppSettings.ViewSettings.GamesViewType == ViewType.List ||
                        (AppSettings.ViewSettings.GamesViewType == ViewType.Images && ShowGameSidebar))
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
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedGames));
            }
        }

        private GamesCollectionView gamesView;
        public GamesCollectionView GamesView
        {
            get => gamesView;
            set
            {
                gamesView = value;
                OnPropertyChanged();
            }
        }

        private List<ThirdPartyTool> thirdPartyTools;
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
        public string ProgressStatus
        {
            get => progressStatus;
            set
            {
                progressStatus = value;
                context.Post((a) => OnPropertyChanged(), null);
            }
        }

        private double progressValue;
        public double ProgressValue
        {
            get => progressValue;
            set
            {
                progressValue = value;
                context.Post((a) => OnPropertyChanged(), null);
            }
        }

        private double progressTotal;
        public double ProgressTotal
        {
            get => progressTotal;
            set
            {
                progressTotal = value;
                context.Post((a) => OnPropertyChanged(), null);
            }
        }

        private bool progressVisible = false;
        public bool ProgressVisible
        {
            get => progressVisible;
            set
            {
                progressVisible = value;
                context.Post((a) => OnPropertyChanged(), null);
            }
        }

        private bool showGameSidebar = false;
        public bool ShowGameSidebar
        {
            get => showGameSidebar;
            set
            {
                if (value == true && SelectedGameDetails == null)
                {
                    SelectedGameDetails = new GameDetailsViewModel(SelectedGame, AppSettings, GamesEditor, Dialogs, Resources);
                }                   

                showGameSidebar = value;
                OnPropertyChanged();
            }
        }

        private bool mainMenuOpened = false;
        public bool MainMenuOpened
        {
            get => mainMenuOpened;
            set
            {
                mainMenuOpened = value;
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
                }

                windowState = value;
                OnPropertyChanged();
            }
        }

        private PlayniteSettings appSettings;
        public PlayniteSettings AppSettings
        {
            get => appSettings;
            private set
            {
                appSettings = value;
                OnPropertyChanged();
            }
        }

        private DatabaseStats gamesStats;
        public DatabaseStats GamesStats
        {
            get => gamesStats;
            private set
            {
                gamesStats = value;
                OnPropertyChanged();
            }
        }

        public DatabaseFilter DatabaseFilters { get; set; }
        


        #region General Commands
        public RelayCommand<object> OpenFilterPanelCommand { get; private set; }
        public RelayCommand<object> CloseFilterPanelCommand { get; private set; }
        public RelayCommand<object> OpenMainMenuCommand { get; private set; }
        public RelayCommand<object> CloseMainMenuCommand { get; private set; }
        public RelayCommand<ThirdPartyTool> ThirdPartyToolOpenCommand { get; private set; }
        public RelayCommand<object> UpdateGamesCommand { get; private set; }
        public RelayCommand<object> OpenSteamFriendsCommand { get; private set; }
        public RelayCommand<object> ReportIssueCommand { get; private set; }
        public RelayCommand<object> ShutdownCommand { get; private set; }
        public RelayCommand<object> ShowWindowCommand { get; private set; }
        public RelayCommand<CancelEventArgs> WindowClosingCommand { get; private set; }
        public RelayCommand<DragEventArgs> FileDroppedCommand { get; private set; }
        public RelayCommand<object> OpenAboutCommand { get; private set; }
        public RelayCommand<object> OpenPlatformsCommand { get; private set; }
        public RelayCommand<object> OpenSettingsCommand { get; private set; }
        public RelayCommand<object> AddCustomGameCommand { get; private set; }
        public RelayCommand<object> AddInstalledGamesCommand { get; private set; }
        public RelayCommand<object> AddEmulatedGamesCommand { get; private set; }
        public RelayCommand<bool> OpenThemeTesterCommand { get; private set; }
        public RelayCommand<object> OpenFullScreenCommand { get; private set; }
        public RelayCommand<object> CancelProgressCommand { get; private set; }
        public RelayCommand<object> ClearMessagesCommand { get; private set; }
        public RelayCommand<object> DownloadMetadataCommand { get; private set; }
        public RelayCommand<object> ClearFiltersCommand { get; private set; }
        public RelayCommand<object> RemoveGameSelectionCommand { get; private set; }
        public RelayCommand<ExtensionFunction> InvokeExtensionFunctionCommand { get; private set; }
        public RelayCommand<object> ReloadScriptsCommand { get; private set; }
        public RelayCommand<GameViewEntry> ShowGameSideBarCommand { get; private set; }
        public RelayCommand<object> CloseGameSideBarCommand { get; private set; }
        public RelayCommand<object> OpenSearchCommand { get; private set; }
        public RelayCommand<object> ToggleFilterPanelCommand { get; private set; }
        public RelayCommand<object> CheckForUpdateCommand { get; private set; }
        public RelayCommand<ILibraryPlugin> UpdateLibraryCommand { get; private set; }
        #endregion

        #region Game Commands
        public RelayCommand<Game> StartGameCommand { get; private set; }
        public RelayCommand<Game> InstallGameCommand { get; private set; }
        public RelayCommand<Game> UninstallGameCommand { get; private set; }
        public RelayCommand<object> StartSelectedGameCommand { get; private set; }
        public RelayCommand<object> EditSelectedGamesCommand { get; private set; }
        public RelayCommand<object> RemoveSelectedGamesCommand { get; private set; }
        public RelayCommand<Game> EditGameCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> EditGamesCommand { get; private set; }
        public RelayCommand<Game> OpenGameLocationCommand { get; private set; }
        public RelayCommand<Game> CreateGameShortcutCommand { get; private set; }
        public RelayCommand<Game> ToggleFavoritesCommand { get; private set; }
        public RelayCommand<Game> ToggleVisibilityCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> SetAsFavoritesCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> RemoveAsFavoritesCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> SetAsHiddensCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> RemoveAsHiddensCommand { get; private set; }
        public RelayCommand<Game> AssignGameCategoryCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> AssignGamesCategoryCommand { get; private set; }
        public RelayCommand<Game> RemoveGameCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> RemoveGamesCommand { get; private set; }
        #endregion

        public MainViewModel(
            GameDatabase database,
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            PlayniteSettings settings,
            GamesEditor gamesEditor,
            PlayniteAPI playniteApi,
            ExtensionFactory extensions)
        {
            context = SynchronizationContext.Current;
            Window = window;
            Dialogs = dialogs;
            Resources = resources;
            Database = database;
            GamesEditor = gamesEditor;
            AppSettings = settings;
            PlayniteApi = playniteApi;
            Extensions = extensions;
            DatabaseFilters = new DatabaseFilter(database, extensions, AppSettings.FilterSettings);

            try
            {
                ThirdPartyTools = ThirdPartyToolsList.GetTools(Extensions.LibraryPlugins?.Select(a => a.Value.Plugin));
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                Logger.Error(e, "Failed to load third party tools.");
            }

            AppSettings.PropertyChanged += AppSettings_PropertyChanged;
            AppSettings.FilterSettings.PropertyChanged += FilterSettings_PropertyChanged;
            AppSettings.ViewSettings.PropertyChanged += ViewSettings_PropertyChanged;
            GamesStats = new DatabaseStats(database);
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            OpenSearchCommand = new RelayCommand<object>((game) =>
            {
                SearchOpened = true;
            }, new KeyGesture(Key.F, ModifierKeys.Control));

            ToggleFilterPanelCommand = new RelayCommand<object>((game) =>
            {
                AppSettings.FilterPanelVisible = !AppSettings.FilterPanelVisible;
            }, new KeyGesture(Key.G, ModifierKeys.Control));

            OpenFilterPanelCommand = new RelayCommand<object>((game) =>
            {
                AppSettings.FilterPanelVisible = true;
            });        

            CloseFilterPanelCommand = new RelayCommand<object>((game) =>
            {
                AppSettings.FilterPanelVisible = false;
            });
        
            OpenMainMenuCommand = new RelayCommand<object>((game) =>
            {
                MainMenuOpened = true;
            });

            CloseMainMenuCommand = new RelayCommand<object>((game) =>
            {
                MainMenuOpened = false;
            });

            ThirdPartyToolOpenCommand = new RelayCommand<ThirdPartyTool>((tool) =>
            {
                MainMenuOpened = false;
                StartThirdPartyTool(tool);
            });

            UpdateGamesCommand = new RelayCommand<object>((a) =>
            {
                MainMenuOpened = false;
                UpdateDatabase(true);
            }, (a) => GameAdditionAllowed || !Database.IsOpen,
            new KeyGesture(Key.F5));

            OpenSteamFriendsCommand = new RelayCommand<object>((a) =>
            {
                OpenSteamFriends();
            });

            ReportIssueCommand = new RelayCommand<object>((a) =>
            {
                MainMenuOpened = false;
                ReportIssue();
            });

            ShutdownCommand = new RelayCommand<object>((a) =>
            {
                MainMenuOpened = false;
                if (GlobalTaskHandler.IsActive)
                {
                    if (Dialogs.ShowMessage(
                        Resources.FindString("LOCBackgroundProgressCancelAskExit"),
                        Resources.FindString("LOCCrashClosePlaynite"),
                        MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                ignoreCloseActions = true;
                ShutdownApp();
            }, new KeyGesture(Key.Q, ModifierKeys.Alt));

            ShowWindowCommand = new RelayCommand<object>((a) =>
            {
                RestoreWindow();
            });

            WindowClosingCommand = new RelayCommand<CancelEventArgs>((args) =>
            {
                OnClosing(args);
            });

            FileDroppedCommand = new RelayCommand<DragEventArgs>((args) =>
            {
                OnFileDropped(args);
            });

            OpenAboutCommand = new RelayCommand<object>((a) =>
            {
                MainMenuOpened = false;
                OpenAboutWindow(new AboutViewModel(AboutWindowFactory.Instance, Dialogs, Resources));
            }, new KeyGesture(Key.F1));

            OpenPlatformsCommand = new RelayCommand<object>((a) =>
            {
                MainMenuOpened = false;
                ConfigurePlatforms(
                    new PlatformsViewModel(Database,
                    PlatformsWindowFactory.Instance,
                    Dialogs,
                    Resources));
            }, (a) => Database.IsOpen,
            new KeyGesture(Key.T, ModifierKeys.Control));

            AddCustomGameCommand = new RelayCommand<object>((a) =>
            {
                MainMenuOpened = false;
                AddCustomGame(GameEditWindowFactory.Instance);
            }, (a) => Database.IsOpen,
            new KeyGesture(Key.Insert));

            AddInstalledGamesCommand = new RelayCommand<object>((a) =>
            {
                MainMenuOpened = false;
                ImportInstalledGames(
                    new InstalledGamesViewModel(
                    InstalledGamesWindowFactory.Instance,
                    Dialogs), null);
            }, (a) => Database.IsOpen);

            AddEmulatedGamesCommand = new RelayCommand<object>((a) =>
            {
                MainMenuOpened = false;
                ImportEmulatedGames(
                    new EmulatorImportViewModel(Database,
                    EmulatorImportViewModel.DialogType.GameImport,
                    EmulatorImportWindowFactory.Instance,
                    Dialogs,
                    Resources));
            }, (a) => Database.IsOpen,
            new KeyGesture(Key.E, ModifierKeys.Control));

            OpenThemeTesterCommand = new RelayCommand<bool>((fullscreen) =>
            {
                var window = new ThemeTesterWindow();
                window.SkinType = fullscreen ? ThemeTesterWindow.SourceType.Fullscreen : ThemeTesterWindow.SourceType.Normal;
                window.Show();
            }, new KeyGesture(Key.F8));

            OpenFullScreenCommand = new RelayCommand<object>((a) =>
            {
                OpenFullScreen();
            }, new KeyGesture(Key.F11));

            CancelProgressCommand = new RelayCommand<object>((a) =>
            {
                CancelProgress();
            }, (a) => !GlobalTaskHandler.CancelToken.IsCancellationRequested);

            ClearMessagesCommand = new RelayCommand<object>((a) =>
            {
                ClearMessages();
            }, (a) => PlayniteApi.Notifications.Count > 0);

            DownloadMetadataCommand = new RelayCommand<object>((a) =>
            {
                MainMenuOpened = false;
                DownloadMetadata(new MetadataDownloadViewModel(MetadataDownloadWindowFactory.Instance));
            }, (a) => GameAdditionAllowed,
            new KeyGesture(Key.D, ModifierKeys.Control));

            ClearFiltersCommand = new RelayCommand<object>((a) =>
            {
                ClearFilters();
            });

            CheckForUpdateCommand = new RelayCommand<object>((a) =>
            {
                MainMenuOpened = false;
                CheckForUpdate();
            });

            UpdateLibraryCommand = new RelayCommand<ILibraryPlugin>((a) =>
            {
                MainMenuOpened = false;
                UpdateLibrary(a);
            }, (a) => GameAdditionAllowed);

            RemoveGameSelectionCommand = new RelayCommand<object>((a) =>
            {
                RemoveGameSelection();
            });

            InvokeExtensionFunctionCommand = new RelayCommand<ExtensionFunction>((f) =>
            {
                MainMenuOpened = false;
                if (!Extensions.InvokeExtension(f, out var error))
                {
                    Dialogs.ShowMessage(
                         Resources.FindString("LOCScriptExecutionError") + "\n\n" + error,
                         Resources.FindString("LOCScriptError"),
                         MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            ReloadScriptsCommand = new RelayCommand<object>((f) =>
            {
                MainMenuOpened = false;
                Extensions.LoadScripts(PlayniteApi, AppSettings.DisabledPlugins);
            }, new KeyGesture(Key.F12));

            ShowGameSideBarCommand = new RelayCommand<GameViewEntry>((f) =>
            {
                SelectedGame = f;
                ShowGameSidebar = true;
            });

            CloseGameSideBarCommand = new RelayCommand<object>((f) =>
            {
                ShowGameSidebar = false;
            });

            OpenSettingsCommand = new RelayCommand<object>((a) =>
            {
                MainMenuOpened = false;
                OpenSettings(
                    new SettingsViewModel(Database,
                    AppSettings,
                    SettingsWindowFactory.Instance,
                    Dialogs,
                    Resources,
                    Extensions));
            }, new KeyGesture(Key.F4));

            StartGameCommand = new RelayCommand<Game>((game) =>
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

            InstallGameCommand = new RelayCommand<Game>((game) =>
            {
                if (game != null)
                {
                    GamesEditor.InstallGame(game);
                }
                else if (SelectedGame != null)
                {
                    GamesEditor.InstallGame(SelectedGame.Game);
                }
            });

            UninstallGameCommand = new RelayCommand<Game>((game) =>
            {
                if (game != null)
                {
                    GamesEditor.UnInstallGame(game);
                }
                else if (SelectedGame != null)
                {
                    GamesEditor.UnInstallGame(SelectedGame.Game);
                }
            });

            EditSelectedGamesCommand = new RelayCommand<object>((a) =>
            {
                if (SelectedGames?.Count() > 1)
                {
                    GamesEditor.EditGames(SelectedGames.Select(g => g.Game).ToList());
                }
                else
                {
                    GamesEditor.EditGame(SelectedGame.Game);
                }
            },
            (a) => SelectedGame != null,
            new KeyGesture(Key.F3));

            StartSelectedGameCommand = new RelayCommand<object>((a) =>
            {
                GamesEditor.PlayGame(SelectedGame.Game);
            },
            (a) => SelectedGames?.Count() == 1,
            new KeyGesture(Key.Enter));

            RemoveSelectedGamesCommand = new RelayCommand<object>((a) =>
            {
                if (SelectedGames?.Count() > 1)
                {
                    GamesEditor.RemoveGames(SelectedGames.Select(g => g.Game).ToList());
                }
                else
                {
                    GamesEditor.RemoveGame(SelectedGame.Game);
                }
            },
            (a) => SelectedGame != null,
            new KeyGesture(Key.Delete));

            EditGameCommand = new RelayCommand<Game>((a) =>
            {
                if (GamesEditor.EditGame(a) == true)
                {
                    SelectedGame = GamesView.Items.FirstOrDefault(g => g.Id == a.Id);
                }
            });

            EditGamesCommand = new RelayCommand<IEnumerable<Game>>((a) =>
            {
                GamesEditor.EditGames(a.ToList());
            });

            OpenGameLocationCommand = new RelayCommand<Game>((a) =>
            {
                GamesEditor.OpenGameLocation(a);
            });

            CreateGameShortcutCommand = new RelayCommand<Game>((a) =>
            {
                GamesEditor.CreateShortcut(a);
            });

            ToggleFavoritesCommand = new RelayCommand<Game>((a) =>
            {
                GamesEditor.ToggleFavoriteGame(a);
            });

            ToggleVisibilityCommand = new RelayCommand<Game>((a) =>
            {
                GamesEditor.ToggleHideGame(a);
            });

            AssignGameCategoryCommand = new RelayCommand<Game>((a) =>
            {
                if (GamesEditor.SetGameCategories(a) == true)
                {
                    SelectedGame = GamesView.Items.FirstOrDefault(g => g.Id == a.Id);
                }
            });

            AssignGamesCategoryCommand = new RelayCommand<IEnumerable<Game>>((a) =>
            {
                GamesEditor.SetGamesCategories(a.ToList());
            });

            RemoveGameCommand = new RelayCommand<Game>((a) =>
            {
                GamesEditor.RemoveGame(a);
            },
            new KeyGesture(Key.Delete));

            RemoveGamesCommand = new RelayCommand<IEnumerable<Game>>((a) =>
            {
                GamesEditor.RemoveGames(a.ToList());
            },
            new KeyGesture(Key.Delete));

            SetAsFavoritesCommand = new RelayCommand<IEnumerable<Game>>((a) =>
            {
                GamesEditor.SetFavoriteGames(a.ToList(), true);
            });

            RemoveAsFavoritesCommand = new RelayCommand<IEnumerable<Game>>((a) =>
            {
                GamesEditor.SetFavoriteGames(a.ToList(), false);
            });

            SetAsHiddensCommand = new RelayCommand<IEnumerable<Game>>((a) =>
            {
                GamesEditor.SetHideGames(a.ToList(), true);
            });

            RemoveAsHiddensCommand = new RelayCommand<IEnumerable<Game>>((a) =>
            {
                GamesEditor.SetHideGames(a.ToList(), false);
            });
        }

        private void ViewSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppSettings.ViewSettings.GamesViewType) &&
                AppSettings.ViewSettings.GamesViewType == ViewType.Images &&
                ShowGameSidebar &&
                SelectedGameDetails == null)
            {
                SelectedGameDetails = new GameDetailsViewModel(SelectedGame, AppSettings, GamesEditor, Dialogs, Resources);
            }
        }

        private void FilterSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(AppSettings.FilterSettings.Active))
            {
                AppSettings.SaveSettings();

                if (e.PropertyName != nameof(AppSettings.FilterSettings.Name) && e.PropertyName != nameof(AppSettings.FilterSettings.SearchActive))
                {
                    AppSettings.FilterPanelVisible = true;
                }
            }
        }

        private void AppSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppSettings.Language))
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
                Dialogs.ShowErrorMessage(Resources.FindString("LOCAppStartupError") + "\n\n" + e.Message, Resources.FindString("LOCStartupError"));
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

        public void ReportIssue()
        {
            try
            {
                ProcessStarter.StartUrl(@"https://github.com/JosefNemec/Playnite/issues/new");
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                Logger.Error(e, "Failed to open report issue url.");
            }
        }

        public void ShutdownApp()
        {
            App.CurrentApp.Quit();
        }

        protected void InitializeView()
        {
            if (GameDatabase.GetMigrationRequired(AppSettings.DatabasePath))
            {
                var migrationProgress = new ProgressViewViewModel(new ProgressWindowFactory(),
                () =>
                {
                    if (AppSettings.DatabasePath.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
                    {
                        var newDbPath = GameDatabase.GetMigratedDbPath(AppSettings.DatabasePath);
                        var newResolvedDbPath = GameDatabase.GetFullDbPath(newDbPath);
                        if (Directory.Exists(newResolvedDbPath))
                        {
                            newDbPath += "_db";
                            newResolvedDbPath += "_db";
                        }

                        if (!File.Exists(AppSettings.DatabasePath))
                        {
                            AppSettings.DatabasePath = newDbPath;
                        }
                        else
                        {
                            var dbSize = new FileInfo(AppSettings.DatabasePath).Length;
                            if (FileSystem.GetFreeSpace(newResolvedDbPath) < dbSize)
                            {
                                throw new NoDiskSpaceException(dbSize);
                            }

                            GameDatabase.MigrateDatabase(AppSettings.DatabasePath);
                            GameDatabase.MigrateToNewFormat(AppSettings.DatabasePath, newResolvedDbPath);
                            FileSystem.DeleteFile(AppSettings.DatabasePath);
                            AppSettings.DatabasePath = newDbPath;
                        }
                    }
                    else
                    {
                        // Do migration of new format when needed
                    }
                }, Resources.FindString("LOCDBUpgradeProgress"));

                if (migrationProgress.ActivateProgress() != true)
                {
                    Logger.Error(migrationProgress.FailException, "Failed to migrate database to new version.");
                    var message = Resources.FindString("LOCDBUpgradeFail");
                    if (migrationProgress.FailException is NoDiskSpaceException exc)
                    {
                        message = string.Format(Resources.FindString("LOCDBUpgradeEmptySpaceFail"), Units.BytesToMegaBytes(exc.RequiredSpace));
                    }

                    Dialogs.ShowErrorMessage(message, "");
                    GameAdditionAllowed = false;
                    return;
                }
            }

            var openProgress = new ProgressViewViewModel(new ProgressWindowFactory(),
            () =>
            {
                if (!Database.IsOpen)
                {
                    Database.SetDatabasePath(AppSettings.DatabasePath);
                    Database.OpenDatabase();
                }
            }, Resources.FindString("LOCOpeningDatabase"));

            if (openProgress.ActivateProgress() != true)
            {
                Logger.Error(openProgress.FailException, "Failed to open library database.");
                var message = Resources.FindString("LOCDatabaseOpenError") + $"\n{openProgress.FailException.Message}";
                Dialogs.ShowErrorMessage(message, "");
                GameAdditionAllowed = false;
                return;
            }
                     
            GamesView = new GamesCollectionView(Database, AppSettings, IsFullscreenView, Extensions);         
            BindingOperations.EnableCollectionSynchronization(GamesView.Items, gamesLock);
            if (GamesView.CollectionView.Count > 0)
            {
                SelectGame((GamesView.CollectionView.GetItemAt(0) as GameViewEntry).Id);
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
            await UpdateDatabase(updateLibrary, true);
        }

        public async Task UpdateDatabase(bool updateLibrary, bool metaForNewGames)
        {
            if (!Database.IsOpen)
            {
                Logger.Error("Cannot load new games, database is not loaded.");
                Dialogs.ShowErrorMessage(Resources.FindString("LOCDatabaseNotOpenedError"), Resources.FindString("LOCDatabaseErroTitle"));
                return;
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
                GlobalTaskHandler.ProgressTask = Task.Run(async () =>
                {
                    var addedGames = new List<Game>();
                    ProgressVisible = true;
                    ProgressValue = 0;
                    ProgressTotal = 1;

                    foreach (var pluginId in Extensions.LibraryPlugins.Keys)
                    {
                        var plugin = Extensions.LibraryPlugins[pluginId];
                        Logger.Info($"Importing games from {plugin.Plugin.Name} plugin.");
                        ProgressStatus = string.Format(Resources.FindString("LOCProgressImportinGames"), plugin.Plugin.Name);

                        try
                        {
                            using (Database.BufferedUpdate())
                            {
                                addedGames.AddRange(GameLibrary.ImportGames(plugin.Plugin, Database, AppSettings.ForcePlayTimeSync));
                            }

                            RemoveMessage($"{plugin.Plugin.Id} - download");
                        }
                        catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                        {
                            Logger.Error(e, $"Failed to import games from plugin: {plugin.Plugin.Name}");
                            AddMessage(new NotificationMessage(
                                $"{plugin.Plugin.Id} - download",
                                string.Format(Resources.FindString("LOCLibraryImportError"), plugin.Plugin.Name) + $"\n{e.Message}",
                                NotificationType.Error,
                                null));
                        }
                    }

                    ProgressStatus = Resources.FindString("LOCProgressLibImportFinish");
                    await Task.Delay(500);
                                     
                    if (addedGames.Any() && metaForNewGames && AppSettings.DownloadMetadataOnImport)
                    {
                        Logger.Info($"Downloading metadata for {addedGames.Count} new games.");
                        ProgressValue = 0;
                        ProgressTotal = addedGames.Count;
                        ProgressStatus = Resources.FindString("LOCProgressMetadata");
                        var metaSettings = new MetadataDownloaderSettings();
                        metaSettings.ConfigureFields(MetadataSource.StoreOverIGDB, true);
                        metaSettings.CoverImage.Source = MetadataSource.IGDBOverStore;
                        metaSettings.Name = new MetadataFieldSettings(true, MetadataSource.Store);
                        using (var downloader = new MetadataDownloader(Database, Extensions.LibraryPlugins.Select(a => a.Value.Plugin)))
                        {
                            downloader.DownloadMetadataGroupedAsync(addedGames, metaSettings,
                                (g, i, t) =>
                                {
                                    ProgressValue = i + 1;
                                    ProgressStatus = Resources.FindString("LOCProgressMetadata") + $" [{ProgressValue}/{ProgressTotal}]";
                                },
                                GlobalTaskHandler.CancelToken).Wait();
                        }
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

                GlobalTaskHandler.CancelToken = new CancellationTokenSource();
                ProgressVisible = true;
                ProgressValue = 0;
                ProgressTotal = games.Count;
                ProgressStatus = Resources.FindString("LOCProgressMetadata");

                using (var downloader = new MetadataDownloader(Database, Extensions.LibraryPlugins.Select(a => a.Value.Plugin)))
                {
                    GlobalTaskHandler.ProgressTask =
                        downloader.DownloadMetadataGroupedAsync(games, settings,
                            (g, i, t) =>
                            {
                                ProgressValue = i + 1;
                                ProgressStatus = Resources.FindString("LOCProgressMetadata") + $" [{ProgressValue}/{ProgressTotal}]";
                            },
                            GlobalTaskHandler.CancelToken);
                    await GlobalTaskHandler.ProgressTask;
                }
            }
            finally
            {
                ProgressVisible = false;
                GameAdditionAllowed = true;
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
                games = GamesView.CollectionView.Cast<GameViewEntry>().Select(a => a.Game).Distinct().ToList();
            }

            await DownloadMetadata(settings, games);
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
                IsInstalled = true
            };

            Database.Games.Add(newGame);
            if (GamesEditor.EditGame(newGame) == true)
            {
                var viewEntry = GamesView.Items.First(a => a.Game.GameId == newGame.GameId);
                SelectedGame = viewEntry;
            }
            else
            {
                Database.Games.Remove(newGame);
            }
        }

        public async void ImportInstalledGames(InstalledGamesViewModel model, string path)
        {
            if (model.OpenView(path) == true && model.Games?.Any() == true)
            {
                var addedGames = InstalledGamesViewModel.AddImportableGamesToDb(model.Games, Database);
                if (AppSettings.DownloadMetadataOnImport)
                {
                    if (!GlobalTaskHandler.IsActive)
                    {
                        var settings = new MetadataDownloaderSettings();
                        settings.ConfigureFields(MetadataSource.IGDB, true);
                        await DownloadMetadata(settings, addedGames);
                    }
                    else
                    {
                        Logger.Warn("Skipping metadata download for manually added games, some global task is already in progress.");
                    }
                }
            }
        }

        public async void ImportEmulatedGames(EmulatorImportViewModel model)
        {
            if (model.OpenView() == true && model.ImportedGames?.Any() == true)
            {
                if (AppSettings.DownloadMetadataOnImport)
                {
                    if (!GlobalTaskHandler.IsActive)
                    {
                        var settings = new MetadataDownloaderSettings();
                        settings.ConfigureFields(MetadataSource.IGDB, true);
                        await DownloadMetadata(settings, model.ImportedGames);
                    }
                    else
                    {
                        Logger.Warn("Skipping metadata download for manually added emulated games, some global task is already in progress.");
                    }
                }
            }
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
            }
            else
            {
                if (GlobalTaskHandler.IsActive)
                {
                    if (Dialogs.ShowMessage(
                        Resources.FindString("LOCBackgroundProgressCancelAskExit"),
                        Resources.FindString("LOCCrashClosePlaynite"),
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
                    Window.BringToForeground();

                    var path = files[0];
                    if (File.Exists(path))
                    {
                        // Other file types to be added in #501
                        if (!(new List<string>() { ".exe", ".lnk" }).Contains(Path.GetExtension(path).ToLower()))
                        {
                            return;
                        }

                        var game = GameExtensions.GetGameFromExecutable(path);
                        var exePath = game.GetRawExecutablePath();
                        if (!string.IsNullOrEmpty(exePath))
                        {
                            var ico = IconExtension.ExtractIconFromExe(exePath, true);
                            if (ico != null)
                            {
                                var iconName = Guid.NewGuid().ToString() + ".png";
                                game.Icon = Database.AddFile(iconName, ico.ToByteArray(System.Drawing.Imaging.ImageFormat.Png), game.Id);
                            }                            
                        }

                        Database.Games.Add(game);
                        Database.AssignPcPlatform(game);
                        GamesEditor.EditGame(game);
                        SelectGame(game.Id);
                    }
                    else if (Directory.Exists(path))
                    {
                        var instModel = new InstalledGamesViewModel(
                           InstalledGamesWindowFactory.Instance,
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
        }

        public void CheckForUpdate()
        {
            try
            {
                var updater = new Updater(App.CurrentApp);
                if (updater.IsUpdateAvailable)
                {
                    var model = new UpdateViewModel(updater, UpdateWindowFactory.Instance, Resources, Dialogs);
                    model.OpenView();
                }
                else
                {
                    Dialogs.ShowMessage(Resources.FindString("LOCUpdateNoNewUpdateMessage"), string.Empty);                    
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                Logger.Error(e, "Failed to check for update.");
                Dialogs.ShowErrorMessage(Resources.FindString("LOCUpdateCheckFailMessage"), Resources.FindString("LOCUpdateError"));
            }
        }

        public async void UpdateLibrary(ILibraryPlugin library)
        {
            GameAdditionAllowed = false;

            try
            {
                GlobalTaskHandler.CancelToken = new CancellationTokenSource();
                GlobalTaskHandler.ProgressTask = Task.Run(async () =>
                {
                    var addedGames = new List<Game>();
                    ProgressVisible = true;
                    ProgressValue = 0;
                    ProgressTotal = 1;                  
                    ProgressStatus = string.Format(Resources.FindString("LOCProgressImportinGames"), library.Name);

                    try
                    {
                        using (Database.BufferedUpdate())
                        {
                            addedGames.AddRange(GameLibrary.ImportGames(library, Database, AppSettings.ForcePlayTimeSync));
                        }

                        RemoveMessage($"{library.Id} - download");
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        Logger.Error(e, $"Failed to import games from plugin: {library.Name}");
                        AddMessage(new NotificationMessage(
                            $"{library.Id} - download",
                            string.Format(Resources.FindString("LOCLibraryImportError"), library.Name) + $"\n{e.Message}",
                            NotificationType.Error,
                            null));
                    }

                    ProgressStatus = Resources.FindString("LOCProgressLibImportFinish");
                    await Task.Delay(500);

                    if (addedGames.Any() && AppSettings.DownloadMetadataOnImport)
                    {
                        Logger.Info($"Downloading metadata for {addedGames.Count} new games.");
                        ProgressValue = 0;
                        ProgressTotal = addedGames.Count;
                        ProgressStatus = Resources.FindString("LOCProgressMetadata");
                        var metaSettings = new MetadataDownloaderSettings();
                        metaSettings.ConfigureFields(MetadataSource.StoreOverIGDB, true);
                        metaSettings.CoverImage.Source = MetadataSource.IGDBOverStore;
                        metaSettings.Name = new MetadataFieldSettings(true, MetadataSource.Store);
                        using (var downloader = new MetadataDownloader(Database, Extensions.LibraryPlugins.Select(a => a.Value.Plugin)))
                        {
                            downloader.DownloadMetadataGroupedAsync(addedGames, metaSettings,
                                (g, i, t) =>
                                {
                                    ProgressValue = i + 1;
                                    ProgressStatus = Resources.FindString("LOCProgressMetadata") + $" [{ProgressValue}/{ProgressTotal}]";
                                },
                                GlobalTaskHandler.CancelToken).Wait();
                        }
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

        public void OpenFullScreen()
        {
            if (GlobalTaskHandler.IsActive)
            {
                ProgressViewViewModel.ActivateProgress(() => GlobalTaskHandler.CancelAndWait(), Resources.FindString("LOCOpeningFullscreenModeMessage"));
            }

            CloseView();
            App.CurrentApp.OpenFullscreenView(false);            
        }

        public void OpenView()
        {
            Window.Show(this);
            if (AppSettings.StartMinimized)
            {
                WindowState = WindowState.Minimized;
            }
            else
            {
                Window.BringToForeground();
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
        }

        public virtual void Dispose()
        {
            GamesView?.Dispose();
            GamesStats?.Dispose();
            AppSettings.PropertyChanged -= AppSettings_PropertyChanged;
            AppSettings.FilterSettings.PropertyChanged -= FilterSettings_PropertyChanged;
        }
    }
}
