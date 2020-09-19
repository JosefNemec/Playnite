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

namespace Playnite.DesktopApp.ViewModels
{
    public class SideBarItem : ObservableObject
    {
        public ApplicationView ViewSource { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public RelayCommand<SideBarItem> Command { get; set; }
        public object CommandParameter { get; set; }
        public object ImageObject
        {
            get
            {
                if (Image.IsNullOrEmpty())
                {
                    return null;
                }
                else
                {
                    return MenuHelpers.GetIcon(Image, default, default);
                }
            }
        }

        private bool selected;
        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                OnPropertyChanged();
            }
        }
    }

    public class DesktopAppViewModel : MainViewModelBase, IDisposable
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

        private ObservableCollection<SideBarItem> appViewItems;
        public ObservableCollection<SideBarItem> AppViewItems
        {
            get => appViewItems;
            set
            {
                appViewItems = value;
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
                Extensions.InvokeOnGameSelected(
                    oldValue?.Select(a => a.Game).ToList(),
                    SelectedGames?.Select(a => a.Game).ToList());
            }
        }

        private DesktopCollectionView gamesView;
        public new DesktopCollectionView GamesView
        {
            get => gamesView;
            set
            {
                gamesView = value;
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

        #region General Commands
        public RelayCommand<object> ToggleExplorerPanelCommand { get; private set; }
        public RelayCommand<object> ToggleFilterPanelCommand { get; private set; }
        public RelayCommand<object> OpenFilterPanelCommand { get; private set; }
        public RelayCommand<object> CloseFilterPanelCommand { get; private set; }
        public RelayCommand<object> CloseNotificationPanelCommand { get; private set; }
        public RelayCommand<ThirdPartyTool> ThirdPartyToolOpenCommand { get; private set; }
        public RelayCommand<object> UpdateGamesCommand { get; private set; }
        public RelayCommand<object> OpenSteamFriendsCommand { get; private set; }
        public RelayCommand<object> ReportIssueCommand { get; private set; }
        public RelayCommand<object> ShutdownCommand { get; private set; }
        public RelayCommand<object> ShowWindowCommand { get; private set; }
        public RelayCommand<CancelEventArgs> WindowClosingCommand { get; private set; }
        public RelayCommand<DragEventArgs> FileDroppedCommand { get; private set; }
        public RelayCommand<object> OpenAboutCommand { get; private set; }
        public RelayCommand<object> OpenEmulatorsCommand { get; private set; }
        public RelayCommand<object> OpenSettingsCommand { get; private set; }
        public RelayCommand<object> AddCustomGameCommand { get; private set; }
        public RelayCommand<object> AddInstalledGamesCommand { get; private set; }
        public RelayCommand<object> AddEmulatedGamesCommand { get; private set; }
        public RelayCommand<object> AddWindowsStoreGamesCommand { get; private set; }
        public RelayCommand<object> OpenFullScreenCommand { get; private set; }
        public RelayCommand<object> OpenFullScreenFromControllerCommand { get; private set; }
        public RelayCommand<object> CancelProgressCommand { get; private set; }
        public RelayCommand<object> ClearMessagesCommand { get; private set; }
        public RelayCommand<object> DownloadMetadataCommand { get; private set; }
        public RelayCommand<object> OpenSoftwareToolsCommand { get; private set; }
        public RelayCommand<object> ClearFiltersCommand { get; private set; }
        public RelayCommand<object> RemoveGameSelectionCommand { get; private set; }
        public RelayCommand<ExtensionFunction> InvokeExtensionFunctionCommand { get; private set; }
        public RelayCommand<object> ReloadScriptsCommand { get; private set; }
        public RelayCommand<GamesCollectionViewEntry> ShowGameSideBarCommand { get; private set; }
        public RelayCommand<object> CloseGameSideBarCommand { get; private set; }
        public RelayCommand<object> OpenSearchCommand { get; private set; }
        public RelayCommand<object> CheckForUpdateCommand { get; private set; }
        public RelayCommand<object> OpenDbFieldsManagerCommand { get; private set; }
        public RelayCommand<LibraryPlugin> UpdateLibraryCommand { get; private set; }
        public RelayCommand<SideBarItem> ChangeAppViewCommand { get; private set; }
        #endregion

        #region Game Commands
        public RelayCommand<Game> StartGameCommand { get; private set; }
        public RelayCommand<AppSoftware> StartSoftwareToolCommand { get; private set; }
        public RelayCommand<Game> InstallGameCommand { get; private set; }
        public RelayCommand<Game> UninstallGameCommand { get; private set; }
        public RelayCommand<object> StartSelectedGameCommand { get; private set; }
        public RelayCommand<object> EditSelectedGamesCommand { get; private set; }
        public RelayCommand<object> RemoveSelectedGamesCommand { get; private set; }
        public RelayCommand<Game> EditGameCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> EditGamesCommand { get; private set; }
        public RelayCommand<Game> OpenGameLocationCommand { get; private set; }
        public RelayCommand<Game> CreateDesktopShortcutCommand { get; private set; }
        public RelayCommand<Game> OpenManualCommand { get; private set; }
        public RelayCommand<Game> ToggleFavoritesCommand { get; private set; }
        public RelayCommand<Game> ToggleVisibilityCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> SetAsFavoritesCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> RemoveAsFavoritesCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> SetAsHiddensCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> RemoveAsHiddensCommand { get; private set; }
        public RelayCommand<Game> AssignGameCategoryCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> AssignGamesCategoryCommand { get; private set; }
        public RelayCommand<Tuple<Game, CompletionStatus>> SetGameCompletionStatusCommand { get; private set; }
        public RelayCommand<Tuple<IEnumerable<Game>, CompletionStatus>> SetGamesCompletionStatusCommand { get; private set; }
        public RelayCommand<Game> RemoveGameCommand { get; private set; }
        public RelayCommand<IEnumerable<Game>> RemoveGamesCommand { get; private set; }
        public RelayCommand<object> SelectRandomGameCommand { get; private set; }
        #endregion

        public DesktopAppViewModel()
        {
            InitializeCommands();
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
            PlayniteApplication app)
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

        private void InitializeCommands()
        {
            OpenSearchCommand = new RelayCommand<object>((game) =>
            {
                if (SearchOpened)
                {
                    // The binding sometimes breaks when main window is restored from minimized state.
                    // This fixes it.
                    SearchOpened = false;
                }

                SearchOpened = true;
            }, new KeyGesture(Key.F, ModifierKeys.Control));

            ToggleExplorerPanelCommand = new RelayCommand<object>((game) =>
            {
                AppSettings.ExplorerPanelVisible = !AppSettings.ExplorerPanelVisible;
            }, new KeyGesture(Key.E, ModifierKeys.Control));

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

            CloseNotificationPanelCommand = new RelayCommand<object>((game) =>
            {
                AppSettings.NotificationPanelVisible = false;
            });

            ThirdPartyToolOpenCommand = new RelayCommand<ThirdPartyTool>((tool) =>
            {
                StartThirdPartyTool(tool);
            });

            UpdateGamesCommand = new RelayCommand<object>((a) =>
            {
#pragma warning disable CS4014
                UpdateDatabase(AppSettings.DownloadMetadataOnImport);
#pragma warning restore CS4014
            }, (a) => GameAdditionAllowed,
            new KeyGesture(Key.F5));

            OpenSteamFriendsCommand = new RelayCommand<object>((a) =>
            {
                OpenSteamFriends();
            });

            ReportIssueCommand = new RelayCommand<object>((a) =>
            {
                ReportIssue();
            });

            ShutdownCommand = new RelayCommand<object>((a) =>
            {
                if (GlobalTaskHandler.IsActive)
                {
                    if (Dialogs.ShowMessage(
                        Resources.GetString("LOCBackgroundProgressCancelAskExit"),
                        Resources.GetString("LOCCrashClosePlaynite"),
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
                OpenAboutWindow(new AboutViewModel(new AboutWindowFactory(), Dialogs, Resources));
            }, new KeyGesture(Key.F1));

            OpenEmulatorsCommand = new RelayCommand<object>((a) =>
            {
                ConfigureEmulators(
                    new EmulatorsViewModel(Database,
                    new EmulatorsWindowFactory(),
                    Dialogs,
                    Resources));
            }, (a) => Database?.IsOpen == true,
            new KeyGesture(Key.T, ModifierKeys.Control));

            OpenSoftwareToolsCommand = new RelayCommand<object>((a) =>
            {
                ConfigureSoftwareTools(new ToolsConfigViewModel(
                    Database,
                    new ToolsConfigWindowFactory(),
                    Dialogs,
                    Resources));
            }, (a) => Database?.IsOpen == true);

            AddCustomGameCommand = new RelayCommand<object>((a) =>
            {
                AddCustomGame(new GameEditWindowFactory());
            }, (a) => Database?.IsOpen == true,
            new KeyGesture(Key.Insert));

            AddInstalledGamesCommand = new RelayCommand<object>((a) =>
            {
                ImportInstalledGames(
                    new InstalledGamesViewModel(
                    new InstalledGamesWindowFactory(),
                    Dialogs), null);
            }, (a) => Database?.IsOpen == true);

            AddEmulatedGamesCommand = new RelayCommand<object>((a) =>
            {
                ImportEmulatedGames(
                    new EmulatorImportViewModel(Database,
                    EmulatorImportViewModel.DialogType.GameImport,
                    new EmulatorImportWindowFactory(),
                    Dialogs,
                    Resources));
            }, (a) => Database?.IsOpen == true,
            new KeyGesture(Key.Q, ModifierKeys.Control));

            AddWindowsStoreGamesCommand = new RelayCommand<object>((a) =>
            {
                ImportWindowsStoreGames(
                    new InstalledGamesViewModel(
                    new InstalledGamesWindowFactory(),
                    Dialogs));
            }, (a) => Database?.IsOpen == true);

            OpenFullScreenCommand = new RelayCommand<object>((a) =>
            {
                SwitchToFullscreenMode();
            }, new KeyGesture(Key.F11));

            OpenFullScreenFromControllerCommand = new RelayCommand<object>((a) =>
            {
                if (AppSettings.GuideButtonOpensFullscreen)
                {
                    SwitchToFullscreenMode();
                }
            }, new KeyGesture(Key.F11));

            CancelProgressCommand = new RelayCommand<object>((a) =>
            {
                CancelProgress();
            }, (a) => GlobalTaskHandler.CancelToken?.IsCancellationRequested == false);

            ClearMessagesCommand = new RelayCommand<object>((a) =>
            {
                ClearMessages();
            }, (a) => PlayniteApi?.Notifications?.Count > 0);

            DownloadMetadataCommand = new RelayCommand<object>((a) =>
            {
                DownloadMetadata(new MetadataDownloadViewModel(new MetadataDownloadWindowFactory()));
            }, (a) => GameAdditionAllowed,
            new KeyGesture(Key.D, ModifierKeys.Control));

            ClearFiltersCommand = new RelayCommand<object>((a) =>
            {
                ClearFilters();
            });

            CheckForUpdateCommand = new RelayCommand<object>((a) =>
            {
                CheckForUpdate();
            });

            OpenDbFieldsManagerCommand = new RelayCommand<object>((a) =>
            {
                ConfigureDatabaseFields(
                        new DatabaseFieldsManagerViewModel(
                            Database,
                            new DatabaseFieldsManagerWindowFactory(),
                            Dialogs,
                            Resources));
            }, (a) => GameAdditionAllowed,
            new KeyGesture(Key.W, ModifierKeys.Control));

            UpdateLibraryCommand = new RelayCommand<LibraryPlugin>((a) =>
            {
                UpdateLibrary(a);
            }, (a) => GameAdditionAllowed);

            RemoveGameSelectionCommand = new RelayCommand<object>((a) =>
            {
                RemoveGameSelection();
            });

            InvokeExtensionFunctionCommand = new RelayCommand<ExtensionFunction>((f) =>
            {
                if (!Extensions.InvokeExtension(f, out var error))
                {
                    var message = error.Message;
                    if (error is ScriptRuntimeException err)
                    {
                        message = err.Message + "\n\n" + err.ScriptStackTrace;
                    }

                    Dialogs.ShowMessage(
                         message,
                         Resources.GetString("LOCScriptError"),
                         MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            ReloadScriptsCommand = new RelayCommand<object>((f) =>
            {
                Extensions.LoadScripts(PlayniteApi, AppSettings.DisabledPlugins, application.CmdLine.SafeStartup);
            }, new KeyGesture(Key.F12));

            ShowGameSideBarCommand = new RelayCommand<GamesCollectionViewEntry>((f) =>
            {
                AppSettings.GridViewSideBarVisible = true;
                SelectedGame = f;
            });

            CloseGameSideBarCommand = new RelayCommand<object>((f) =>
            {
                AppSettings.GridViewSideBarVisible = false;
            });

            OpenSettingsCommand = new RelayCommand<object>((a) =>
            {
                OpenSettings(
                    new SettingsViewModel(Database,
                    AppSettings,
                    new SettingsWindowFactory(),
                    Dialogs,
                    Resources,
                    Extensions,
                    application));
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

            StartSoftwareToolCommand = new RelayCommand<AppSoftware>((app) =>
            {
                StartSoftwareTool(app);
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
                    ignoreSelectionChanges = true;
                    try
                    {
                        GamesEditor.EditGames(SelectedGames.Select(g => g.Game).ToList());
                    }
                    finally
                    {
                        ignoreSelectionChanges = false;
                    }
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
                ignoreSelectionChanges = true;
                try
                {
                    GamesEditor.EditGames(a.ToList());
                }
                finally
                {
                    ignoreSelectionChanges = false;
                }
            });

            OpenGameLocationCommand = new RelayCommand<Game>((a) =>
            {
                GamesEditor.OpenGameLocation(a);
            });

            CreateDesktopShortcutCommand = new RelayCommand<Game>((a) =>
            {
                GamesEditor.CreateDesktopShortcut(a);
            });

            OpenManualCommand = new RelayCommand<Game>((a) =>
            {
                GamesEditor.OpenManual(a);
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

            SetGameCompletionStatusCommand = new RelayCommand<Tuple<Game, CompletionStatus>>((a) =>
            {
                GamesEditor.SetCompletionStatus(a.Item1, a.Item2);
            });

            SetGamesCompletionStatusCommand = new RelayCommand<Tuple<IEnumerable<Game>, CompletionStatus>>((a) =>
            {
                GamesEditor.SetCompletionStatus(a.Item1.ToList(), a.Item2);
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

            SelectRandomGameCommand = new RelayCommand<object>((a) =>
            {
                PlayRandomGame();
            }, (a) => Database?.IsOpen == true,
            new KeyGesture(Key.F6));

            ChangeAppViewCommand = new RelayCommand<SideBarItem>((item) =>
            {
                AppSettings.CurrentApplicationView = item.ViewSource;
            });
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
            if (e.PropertyName == nameof(PlayniteSettings.CurrentApplicationView))
            {
                AppViewItems.ForEach(a => a.Selected = false);

                if (AppSettings.CurrentApplicationView == ApplicationView.Statistics)
                {
                    AppViewItems[1].Selected = true;
                    ActiveView = statsView;
                    LibraryStats.Calculate();
                }
                else if (AppSettings.CurrentApplicationView == ApplicationView.Library)
                {
                    AppViewItems[0].Selected = true;
                    ActiveView = libraryView;
                }
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

        protected void InitializeView()
        {
            LibraryStats = new StatisticsViewModel(Database, Extensions, AppSettings, (g) =>
            {
                appSettings.CurrentApplicationView = ApplicationView.Library;
                SelectedGame = GamesView.Items.FirstOrDefault(a => g.Id == a.Id);
            });

            libraryView = new Controls.Views.Library(this);
            statsView = new Controls.LibraryStatistics(LibraryStats);

            AppViewItems = new ObservableCollection<SideBarItem>()
            {
                new SideBarItem
                {
                    Image = "SidebarLibraryIcon",
                    Name = Resources.GetString(LOC.Library),
                    Selected = true,
                    ViewSource = ApplicationView.Library
                },
                new SideBarItem
                {
                    Image = "SidebarStatisticsIcon",
                    Name = Resources.GetString(LOC.Statistics),
                    Selected = false,
                    ViewSource = ApplicationView.Statistics
                }
            };

            AppViewItems.ForEach(a =>
            {
                a.CommandParameter = a;
                a.Command = ChangeAppViewCommand;
            });

            AppSettings.CurrentApplicationView = ApplicationView.Library;
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
        }

        public async Task UpdateDatabase(bool metaForNewGames)
        {
            if (!Database.IsOpen)
            {
                Logger.Error("Cannot load new games, database is not loaded.");
                Dialogs.ShowErrorMessage(Resources.GetString("LOCDatabaseNotOpenedError"), Resources.GetString("LOCDatabaseErroTitle"));
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
                GlobalTaskHandler.CancelToken = new CancellationTokenSource();
                GlobalTaskHandler.ProgressTask = Task.Run(() =>
                {
                    DatabaseFilters.IgnoreDatabaseUpdates = true;
                    var addedGames = new List<Game>();
                    ProgressVisible = true;
                    ProgressValue = 0;
                    ProgressTotal = 1;

                    foreach (var plugin in Extensions.LibraryPlugins)
                    {
                        if (GlobalTaskHandler.CancelToken.IsCancellationRequested)
                        {
                            return;
                        }

                        Logger.Info($"Importing games from {plugin.Name} plugin.");
                        ProgressStatus = string.Format(Resources.GetString("LOCProgressImportinGames"), plugin.Name);

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
                                string.Format(Resources.GetString("LOCLibraryImportError"), plugin.Name) + $"\n{e.Message}",
                                NotificationType.Error));
                        }
                    }

                    ProgressStatus = Resources.GetString("LOCProgressLibImportFinish");
                    Thread.Sleep(500);

                    if (addedGames.Any() && metaForNewGames)
                    {
                        Logger.Info($"Downloading metadata for {addedGames.Count} new games.");
                        ProgressValue = 0;
                        ProgressTotal = addedGames.Count;
                        ProgressStatus = Resources.GetString("LOCProgressMetadata");
                        using (var downloader = new MetadataDownloader(Database, Extensions.MetadataPlugins, Extensions.LibraryPlugins))
                        {
                            downloader.DownloadMetadataAsync(addedGames, AppSettings.MetadataSettings, AppSettings,
                                (g, i, t) =>
                                {
                                    ProgressValue = i + 1;
                                    ProgressStatus = Resources.GetString("LOCProgressMetadata") + $" [{ProgressValue}/{ProgressTotal}]";
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

        public async void UpdateLibrary(LibraryPlugin library)
        {
            GameAdditionAllowed = false;

            try
            {
                GlobalTaskHandler.CancelToken = new CancellationTokenSource();
                GlobalTaskHandler.ProgressTask = Task.Run(() =>
                {
                    DatabaseFilters.IgnoreDatabaseUpdates = true;
                    var addedGames = new List<Game>();
                    ProgressVisible = true;
                    ProgressValue = 0;
                    ProgressTotal = 1;
                    ProgressStatus = string.Format(Resources.GetString("LOCProgressImportinGames"), library.Name);

                    try
                    {
                        addedGames.AddRange(Database.ImportGames(library, AppSettings.ForcePlayTimeSync, AppSettings.ImportExclusionList.Items));
                        RemoveMessage($"{library.Id} - download");
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        Logger.Error(e, $"Failed to import games from plugin: {library.Name}");
                        AddMessage(new NotificationMessage(
                            $"{library.Id} - download",
                            string.Format(Resources.GetString("LOCLibraryImportError"), library.Name) + $"\n{e.Message}",
                            NotificationType.Error));
                    }

                    ProgressStatus = Resources.GetString("LOCProgressLibImportFinish");
                    Thread.Sleep(500);

                    if (addedGames.Any() && AppSettings.DownloadMetadataOnImport)
                    {
                        Logger.Info($"Downloading metadata for {addedGames.Count} new games.");
                        ProgressValue = 0;
                        ProgressTotal = addedGames.Count;
                        ProgressStatus = Resources.GetString("LOCProgressMetadata");
                        using (var downloader = new MetadataDownloader(Database, Extensions.MetadataPlugins, Extensions.LibraryPlugins))
                        {
                            downloader.DownloadMetadataAsync(addedGames, AppSettings.MetadataSettings, AppSettings,
                                (g, i, t) =>
                                {
                                    ProgressValue = i + 1;
                                    ProgressStatus = Resources.GetString("LOCProgressMetadata") + $" [{ProgressValue}/{ProgressTotal}]";
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
                DatabaseFilters.IgnoreDatabaseUpdates = false;
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

        public async void ImportEmulatedGames(EmulatorImportViewModel model)
        {
            if (model.OpenView() == true && model.ImportedGames?.Any() == true)
            {
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
        }

        public void OpenAboutWindow(AboutViewModel model)
        {
            model.OpenView();
        }

        public void OpenSettings(SettingsViewModel model)
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
            application.Quit();
            var cmdline = new CmdLineOptions()
            {
                SkipLibUpdate = true,
                StartInFullscreen = true
            };

            ProcessStarter.StartProcess(PlaynitePaths.FullscreenExecutablePath, cmdline.ToString());
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
        }

        public virtual void Dispose()
        {
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
    }
}
