using Playnite.API;
using Playnite.Commands;
using Playnite.Common;
using Playnite.Controls;
using Playnite.Database;
using Playnite.FullscreenApp.Controls;
using Playnite.FullscreenApp.Controls.Views;
using Playnite.FullscreenApp.Windows;
using Playnite.Metadata;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.ViewModels;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Playnite.FullscreenApp.ViewModels
{
    public class FullscreenAppViewModel : MainViewModelBase, IDisposable
    {
        public static ILogger Logger = LogManager.GetLogger();
        private static object gamesLock = new object();
        private readonly SynchronizationContext context;
        private PlayniteApplication application;
        private bool isInitialized = false;
        private string oldTheme;
        protected bool ignoreCloseActions = false;

        public PlayniteAPI PlayniteApi { get; set; }
        public ExtensionFactory Extensions { get; }
        public IWindowFactory Window { get; }
        public IDialogsFactory Dialogs { get; }
        public IResourceProvider Resources { get; }
        public GameDatabase Database { get; }
        public GamesEditor GamesEditor { get; }
        public PlayniteSettings AppSettings { get; set; }
        public bool IsFullScreen { get; private set; } = true;
        public ObservableTime CurrentTime { get; } = new ObservableTime();
        public ObservablePowerStatus PowerStatus { get; } = new ObservablePowerStatus();

        private bool databaseUpdateRunning = false;
        public bool DatabaseUpdateRunning
        {
            get => databaseUpdateRunning;
            set
            {
                databaseUpdateRunning = value;
                OnPropertyChanged();
            }
        }

        private double windowLeft = 0;
        public double WindowLeft
        {
            get => windowLeft;
            set
            {
                windowLeft = value;
                OnPropertyChanged();
            }
        }

        private double windowTop = 0;
        public double WindowTop
        {
            get => windowTop;
            set
            {
                windowTop = value;
                OnPropertyChanged();
            }
        }

        private double windowWidth = 1920;
        public double WindowWidth
        {
            get => windowWidth;
            set
            {
                windowWidth = value;
                OnPropertyChanged();
            }
        }

        private double windowHeight = 1080;
        public double WindowHeight
        {
            get => windowHeight;
            set
            {
                windowHeight = value;
                OnPropertyChanged();
            }
        }

        private double viewportWidth = 1920;
        public double ViewportWidth
        {
            get => viewportWidth;
            set
            {
                viewportWidth = value;
                OnPropertyChanged();
            }
        }

        private double viewportHeight = 1080;
        public double ViewportHeight
        {
            get => viewportHeight;
            set
            {
                viewportHeight = value;
                OnPropertyChanged();
            }
        }

        private FullscreenCollectionView gamesView;
        public new FullscreenCollectionView GamesView
        {
            get => gamesView;
            set
            {
                gamesView = value;
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
                var oldValue = selectedGame;
                // TODO completely rework and decouple selected game from main view and game details
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
                    SelectedGameDetails = new GameDetailsViewModel(value, Resources, GamesEditor, this, Dialogs);
                }

                selectedGame = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(GameDetailsButtonVisible));

                if (GameDetailsVisible && (value == null || GameDetailsEntry != value))
                {
                    var selected = SelectClosestGameDetails();
                    if (selected != null)
                    {
                        selectedGame = selected;
                        OnPropertyChanged();
                        OnPropertyChanged(nameof(GameDetailsButtonVisible));
                    }
                }

                Extensions.InvokeOnGameSelected(
                    oldValue == null ? null : new List <Game> { oldValue.Game },
                    selectedGame == null ? null : new List<Game> { selectedGame.Game });
            }
        }

        private int lastGameDetailsIndex = -1;

        private GamesCollectionViewEntry gameDetailsEntry;
        public GamesCollectionViewEntry GameDetailsEntry
        {
            get => gameDetailsEntry;
            set
            {
                // TODO completely rework and decouple selected game from main view and game details
                SelectedGameDetails?.Dispose();
                if (value == null)
                {
                    if (SelectedGame != null)
                    {
                        SelectedGameDetails = new GameDetailsViewModel(SelectedGame, Resources, GamesEditor, this, Dialogs);
                    }
                    else
                    {
                        SelectedGameDetails = null;
                    }

                    lastGameDetailsIndex = -1;
                }
                else
                {
                    SelectedGameDetails = new GameDetailsViewModel(value, Resources, GamesEditor, this, Dialogs);
                    lastGameDetailsIndex = GamesView.CollectionView.IndexOf(value);
                }

                gameDetailsEntry = value;
                OnPropertyChanged();
            }
        }

        private bool mainMenuVisible = false;
        public bool MainMenuVisible
        {
            get => mainMenuVisible;
            set
            {
                mainMenuVisible = value;
                OnPropertyChanged();
            }
        }

        private bool gameMenuVisible = false;
        public bool GameMenuVisible
        {
            get => gameMenuVisible;
            set
            {
                gameMenuVisible = value;
                OnPropertyChanged();
            }
        }

        private bool settingsMenuVisible = false;
        public bool SettingsMenuVisible
        {
            get => settingsMenuVisible;
            set
            {
                settingsMenuVisible = value;
                OnPropertyChanged();
            }
        }

        private bool gameListFocused = false;
        public bool GameListFocused
        {
            get => gameListFocused;
            set
            {
                gameListFocused = value;
                OnPropertyChanged();
            }
        }

        private bool mainMenuFocused = false;
        public bool MainMenuFocused
        {
            get => mainMenuFocused;
            set
            {
                mainMenuFocused = value;
                OnPropertyChanged();
            }
        }

        private bool gameListVisible = true;
        public bool GameListVisible
        {
            get => gameListVisible;
            set
            {
                gameListVisible = value;
                OnPropertyChanged();
            }
        }

        private bool gameDetailsFocused = false;
        public bool GameDetailsFocused
        {
            get => gameDetailsFocused;
            set
            {
                gameDetailsFocused = value;
                OnPropertyChanged();
            }
        }

        private bool gameDetailsVisible = false;
        public bool GameDetailsVisible
        {
            get => gameDetailsVisible;
            set
            {
                gameDetailsVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(GameDetailsButtonVisible));

                if (value == true)
                {
                    GameDetailsEntry = SelectedGame;
                }
                else
                {
                    GameDetailsEntry = null;
                }
            }
        }

        public bool GameDetailsButtonVisible => GameDetailsVisible == false && SelectedGame != null;

        private DatabaseFilter databaseFilters;
        public DatabaseFilter DatabaseFilters
        {
            get => databaseFilters;
            private set
            {
                databaseFilters = value;
                OnPropertyChanged();
            }
        }

        private DatabaseExplorer databaseExplorer;
        public DatabaseExplorer DatabaseExplorer
        {
            get => databaseExplorer;
            private set
            {
                databaseExplorer = value;
                OnPropertyChanged();
            }
        }

        private System.Windows.Controls.Control subFilterControl;
        public System.Windows.Controls.Control SubFilterControl
        {
            get => subFilterControl;
            set
            {
                subFilterControl = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SubFilterVisible));
            }
        }

        public bool SubFilterVisible
        {
            get => SubFilterControl != null;
        }

        private bool filterPanelVisible = false;
        public bool FilterPanelVisible
        {
            get => filterPanelVisible;
            set
            {
                filterPanelVisible = value;
                OnPropertyChanged();
            }
        }

        private bool filterAdditionalPanelVisible = false;
        public bool FilterAdditionalPanelVisible
        {
            get => filterAdditionalPanelVisible;
            set
            {
                filterAdditionalPanelVisible = value;
                OnPropertyChanged();
            }
        }

        private bool notificationsVisible = false;
        public bool NotificationsVisible
        {
            get => notificationsVisible;
            set
            {
                notificationsVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsExtraFilterActive
        {
            get => !IsSearchActive && GetIsExtraFilterActive(AppSettings.Fullscreen);
        }

        public bool IsSearchActive
        {
            get => !AppSettings.Fullscreen.FilterSettings.Name.IsNullOrEmpty();
        }

        #region Commands
        public RelayCommand<CancelEventArgs> WindowClosingCommand { get; private set; }
        public RelayCommand<EventArgs> WindowGotFocusCommand { get; private set; }
        public RelayCommand<object> ExitCommand { get; private set; }
        public RelayCommand<object> SwitchToDesktopCommand { get; private set; }
        public RelayCommand<object> ToggleFullscreenCommand { get; private set; }
        public RelayCommand<object> ToggleMainMenuCommand { get; private set; }
        public RelayCommand<object> ToggleGameOptionsCommand { get; private set; }
        public RelayCommand<object> ToggleSettingsMenuCommand { get; private set; }
        public RelayCommand<object> ToggleGameDetailsCommand { get; private set; }
        public RelayCommand<object> ToggleFiltersCommand { get; private set; }
        public RelayCommand<object> ToggleNotificationsCommand { get; private set; }
        public RelayCommand<object> ClearNotificationsCommand { get; private set; }
        public RelayCommand<GameField> LoadSubFilterCommand { get; private set; }
        public RelayCommand<object> CloseSubFilterCommand { get; private set; }
        public RelayCommand<object> CloseAdditionalFilterCommand { get; private set; }
        public RelayCommand<object> ShutdownSystemCommand { get; private set; }
        public RelayCommand<object> RestartSystemCommand { get; private set; }
        public RelayCommand<object> HibernateSystemCommand { get; private set; }
        public RelayCommand<object> SleepSystemCommand { get; private set; }
        public RelayCommand<object> ClearFiltersCommand { get; private set; }
        public RelayCommand<object> OpenAdditionalFiltersCommand { get; private set; }
        public RelayCommand<object> CloseAdditionalFiltersCommand { get; private set; }
        public RelayCommand<object> ActivateSelectedCommand { get; private set; }
        public RelayCommand<object> OpenSearchCommand { get; private set; }
        public RelayCommand<object> NextFilterViewCommand { get; private set; }
        public RelayCommand<object> PrevFilterViewCommand { get; private set; }
        public RelayCommand<object> SelectPrevGameCommand { get; private set; }
        public RelayCommand<object> SelectNextGameCommand { get; private set; }
        public RelayCommand<DragEventArgs> FileDroppedCommand { get; private set; }
        public RelayCommand<object> SelectRandomGameCommand { get; private set; }
        public RelayCommand<object> UpdateGamesCommand { get; private set; }
        #endregion Commands

        public FullscreenAppViewModel()
        {
            InitializeCommands();
        }

        public FullscreenAppViewModel(
            GameDatabase database,
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            PlayniteSettings settings,
            GamesEditor gamesEditor,
            PlayniteAPI playniteApi,
            ExtensionFactory extensions,
            PlayniteApplication app) : this()
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
            ((NotificationsAPI)PlayniteApi.Notifications).ActivationRequested += FullscreenAppViewModel_ActivationRequested;
            IsFullScreen = !PlayniteEnvironment.IsDebuggerAttached;
            settings.Fullscreen.PropertyChanged += Fullscreen_PropertyChanged;
            settings.Fullscreen.FilterSettings.FilterChanged += FilterSettings_FilterChanged;
            ThemeManager.ApplyFullscreenButtonPrompts(PlayniteApplication.CurrentNative, AppSettings.Fullscreen.ButtonPrompts);
        }

        private void FullscreenAppViewModel_ActivationRequested(object sender, NotificationsAPI.ActivationRequestEventArgs e)
        {
            PlayniteApi.Notifications.Remove(e.Message.Id);
            NotificationsVisible = false;
            GameListFocused = true;
            e.Message.ActivationAction();
        }

        private void Fullscreen_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FullscreenSettings.ActiveView))
            {
                SetQuickFilter(AppSettings.Fullscreen);
                SelectGameIndex(0);
            }
            else if (e.PropertyName == nameof(FullscreenSettings.Monitor))
            {
                SetViewSizeAndPosition(IsFullScreen);
            }
            else if (e.PropertyName == nameof(FullscreenSettings.InstalledOnlyInQuickFilters))
            {
                // TODO
                //if (AppSettings.Fullscreen.ActiveView != ActiveFullscreenView.Explore && AppSettings.Fullscreen.ActiveView != ActiveFullscreenView.All)
                if (AppSettings.Fullscreen.ActiveView != ActiveFullscreenView.All)
                {
                    AppSettings.Fullscreen.FilterSettings.IsInstalled = AppSettings.Fullscreen.InstalledOnlyInQuickFilters;
                }
            }
            else if (e.PropertyName == nameof(FullscreenSettings.ButtonPrompts))
            {
                ThemeManager.ApplyFullscreenButtonPrompts(PlayniteApplication.CurrentNative, AppSettings.Fullscreen.ButtonPrompts);
            }
        }

        private void FilterSettings_FilterChanged(object sender, FilterChangedEventArgs e)
        {
            OnPropertyChanged(nameof(IsExtraFilterActive));
            OnPropertyChanged(nameof(IsSearchActive));
        }

        internal void SetQuickFilter(FullscreenSettings settings)
        {
            settings.FilterSettings.ClearFilters();
            switch (settings.ActiveView)
            {
                case ActiveFullscreenView.RecentlyPlayed:
                    // TODO buffer
                    settings.FilterSettings.IsInstalled = settings.InstalledOnlyInQuickFilters;
                    settings.FilterSettings.Favorite = false;
                    settings.ViewSettings.SortingOrder = SortOrder.LastActivity;
                    settings.ViewSettings.SortingOrderDirection = SortOrderDirection.Descending;
                    break;
                case ActiveFullscreenView.Favorites:
                    settings.FilterSettings.IsInstalled = settings.InstalledOnlyInQuickFilters;
                    settings.FilterSettings.Favorite = true;
                    settings.ViewSettings.SortingOrder = SortOrder.Name;
                    settings.ViewSettings.SortingOrderDirection = SortOrderDirection.Ascending;
                    break;
                case ActiveFullscreenView.MostPlayed:
                    settings.FilterSettings.IsInstalled = settings.InstalledOnlyInQuickFilters;
                    settings.FilterSettings.Favorite = false;
                    settings.ViewSettings.SortingOrder = SortOrder.Playtime;
                    settings.ViewSettings.SortingOrderDirection = SortOrderDirection.Descending;
                    break;
                case ActiveFullscreenView.All:
                    settings.FilterSettings.IsInstalled = false;
                    settings.FilterSettings.Favorite = false;
                    settings.ViewSettings.SortingOrder = SortOrder.Name;
                    settings.ViewSettings.SortingOrderDirection = SortOrderDirection.Ascending;
                    break;
                //case ActiveFullscreenView.Explore:
                //    break;
            }
        }

        internal bool GetIsExtraFilterActive(FullscreenSettings settings)
        {
            var tempSettings = new FullscreenSettings
            {
                ActiveView = settings.ActiveView,
                InstalledOnlyInQuickFilters = settings.InstalledOnlyInQuickFilters
            };

            SetQuickFilter(tempSettings);
            return !tempSettings.FilterSettings.IsEqualJson(settings.FilterSettings);
        }

        private void InitializeCommands()
        {
            WindowClosingCommand = new RelayCommand<CancelEventArgs>((a) =>
            {
                if (!ignoreCloseActions)
                {
                    Dispose();
                    application.Quit();
                }
            });

            WindowGotFocusCommand = new RelayCommand<EventArgs>((a) =>
            {
                if (Keyboard.FocusedElement == Window.Window && isInitialized && !ignoreCloseActions)
                {
                    Logger.Warn("Lost keyboard focus from known controls, trying to focus something.");
                    foreach (var child in ElementTreeHelper.FindVisualChildren<FrameworkElement>(Window.Window))
                    {
                        if (child.Focusable && child.IsVisible)
                        {
                            Logger.Debug($"Focusing {child}");
                            child.Focus();
                            return;
                        }
                    }
                }
            });

            ExitCommand = new RelayCommand<object>((a) =>
            {
                Shutdown();
            });

            ToggleFullscreenCommand = new RelayCommand<object>((a) =>
            {
                ToggleFullscreen();
            });

            ToggleMainMenuCommand = new RelayCommand<object>((a) =>
            {
                if (MainMenuVisible)
                {
                    GameListFocused = true;
                }
                else
                {
                    GameListFocused = false;
                }

                MainMenuVisible = !MainMenuVisible;
                if (!MainMenuVisible)
                {
                    MainMenuFocused = false;
                }
                else
                {
                    MainMenuFocused = true;
                }
            });

            ToggleGameOptionsCommand = new RelayCommand<object>((a) =>
            {
                if (GameMenuVisible)
                {
                    if (GameDetailsVisible)
                    {
                        GameDetailsFocused = true;
                    }
                    else
                    {
                        GameListFocused = true;
                    }
                }
                else
                {
                    if (GameDetailsVisible)
                    {
                        GameDetailsFocused = false;
                    }
                    else
                    {
                        GameListFocused = false;
                    }
                }

                GameMenuVisible = !GameMenuVisible;
            }, (a) => SelectedGame != null);

            ToggleSettingsMenuCommand = new RelayCommand<object>((a) =>
            {
                if (SettingsMenuVisible)
                {
                    if (oldTheme != AppSettings.Fullscreen.Theme)
                    {
                        if (Dialogs.ShowMessage(
                            ResourceProvider.GetString("LOCSettingsRestartAskMessage"),
                            ResourceProvider.GetString("LOCSettingsRestartTitle"),
                            MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            application.Restart(new CmdLineOptions() { SkipLibUpdate = true });
                        }
                    }

                    MainMenuFocused = true;
                }
                else
                {
                    MainMenuFocused = false;
                    oldTheme = AppSettings.Fullscreen.Theme;
                }

                SettingsMenuVisible = !SettingsMenuVisible;
            });

            ToggleGameDetailsCommand = new RelayCommand<object>((a) =>
            {
                var oldEntr = GameDetailsEntry;
                if (GameDetailsVisible)
                {
                    SelectedGame = oldEntr;
                }

                GameDetailsVisible = !GameDetailsVisible;
                GameListVisible = !GameListVisible;

                if (!GameDetailsVisible)
                {
                    GameDetailsFocused = false;
                    GameListFocused = true;
                }
                else
                {
                    GameDetailsFocused = true;
                    GameListFocused = false;
                }
            }, (a) => SelectedGame != null);

            ToggleNotificationsCommand = new RelayCommand<object>((a) =>
            {
                if (NotificationsVisible)
                {
                    GameListFocused = true;
                }
                else
                {
                    GameListFocused = false;
                }

                NotificationsVisible = !NotificationsVisible;
            });

            SwitchToDesktopCommand = new RelayCommand<object>((a) =>
            {
                if (GlobalTaskHandler.IsActive)
                {
                    ProgressViewViewModel.ActivateProgress(() => GlobalTaskHandler.CancelAndWait(), Resources.GetString("LOCOpeningDesktopModeMessage"));
                }

                CloseView();
                application.Quit();
                var cmdline = new CmdLineOptions()
                {
                    SkipLibUpdate = true,
                    StartInDesktop = true
                };

                ProcessStarter.StartProcess(PlaynitePaths.DesktopExecutablePath, cmdline.ToString());
            }, new KeyGesture(Key.F11));

            ShutdownSystemCommand = new RelayCommand<object>((a) =>
            {
                if (!PlayniteEnvironment.IsDebuggerAttached)
                {
                    Computer.Shutdown();
                }
            });

            HibernateSystemCommand = new RelayCommand<object>((a) =>
            {
                if (!PlayniteEnvironment.IsDebuggerAttached)
                {
                    Computer.Hibernate();
                }
            });

            SleepSystemCommand = new RelayCommand<object>((a) =>
            {
                if (!PlayniteEnvironment.IsDebuggerAttached)
                {
                    Computer.Sleep();
                }
            });

            RestartSystemCommand = new RelayCommand<object>((a) =>
            {
                if (!PlayniteEnvironment.IsDebuggerAttached)
                {
                    Computer.Restart();
                }
            });

            LoadSubFilterCommand = new RelayCommand<GameField>((gameField) =>
            {
                switch (gameField)
                {
                    case GameField.PluginId:
                        OpenSubFilter("LOCLibrary", nameof(DatabaseFilter.Libraries), nameof(FilterSettings.Library), true);
                        break;
                    case GameField.Categories:
                        OpenSubFilter("LOCCategoryLabel", nameof(DatabaseFilter.Categories), nameof(FilterSettings.Category), true);
                        break;
                    case GameField.Platform:
                        OpenSubFilter("LOCPlatformTitle", nameof(DatabaseFilter.Platforms), nameof(FilterSettings.Platform), true);
                        break;
                    case GameField.CompletionStatus:
                        OpenSubEnumFilter("LOCCompletionStatus", typeof(CompletionStatus), nameof(FilterSettings.CompletionStatus));
                        break;
                    case GameField.ReleaseYear:
                        OpenSubStringFilter("LOCGameReleaseYearTitle", nameof(DatabaseFilter.ReleaseYears), nameof(FilterSettings.ReleaseYear));
                        break;
                    case GameField.Genres:
                        OpenSubFilter("LOCGenreLabel", nameof(DatabaseFilter.Genres), nameof(FilterSettings.Genre));
                        break;
                    case GameField.Developers:
                        OpenSubFilter("LOCDeveloperLabel", nameof(DatabaseFilter.Developers), nameof(FilterSettings.Developer));
                        break;
                    case GameField.Publishers:
                        OpenSubFilter("LOCPublisherLabel", nameof(DatabaseFilter.Publishers), nameof(FilterSettings.Publisher));
                        break;
                    case GameField.Features:
                        OpenSubFilter("LOCFeatureLabel", nameof(DatabaseFilter.Features), nameof(FilterSettings.Feature));
                        break;
                    case GameField.Tags:
                        OpenSubFilter("LOCTagLabel", nameof(DatabaseFilter.Tags), nameof(FilterSettings.Tag));
                        break;
                    case GameField.Playtime:
                        OpenSubEnumFilter("LOCTimePlayed", typeof(PlaytimeCategory), nameof(FilterSettings.PlayTime));
                        break;
                    case GameField.Series:
                        OpenSubFilter("LOCSeriesLabel", nameof(DatabaseFilter.Series), nameof(FilterSettings.Series));
                        break;
                    case GameField.Region:
                        OpenSubFilter("LOCRegionLabel", nameof(DatabaseFilter.Regions), nameof(FilterSettings.Region));
                        break;
                    case GameField.Source:
                        OpenSubFilter("LOCSourceLabel", nameof(DatabaseFilter.Sources), nameof(FilterSettings.Source));
                        break;
                    case GameField.AgeRating:
                        OpenSubFilter("LOCAgeRatingLabel", nameof(DatabaseFilter.AgeRatings), nameof(FilterSettings.AgeRating));
                        break;
                    case GameField.UserScore:
                        OpenSubEnumFilter("LOCUserScore", typeof(ScoreGroup), nameof(FilterSettings.UserScore));
                        break;
                    case GameField.CommunityScore:
                        OpenSubEnumFilter("LOCCommunityScore", typeof(ScoreGroup), nameof(FilterSettings.CommunityScore));
                        break;
                    case GameField.CriticScore:
                        OpenSubEnumFilter("LOCCriticScore", typeof(ScoreGroup), nameof(FilterSettings.CriticScore));
                        break;
                    case GameField.LastActivity:
                        OpenSubEnumFilter("LOCGameLastActivityTitle", typeof(PastTimeSegment), nameof(FilterSettings.LastActivity));
                        break;
                    case GameField.Added:
                        OpenSubEnumFilter("LOCAddedLabel", typeof(PastTimeSegment), nameof(FilterSettings.Added));
                        break;
                    case GameField.Modified:
                        OpenSubEnumFilter("LOCModifiedLabel", typeof(PastTimeSegment), nameof(FilterSettings.Modified));
                        break;
                }
            });

            OpenAdditionalFiltersCommand = new RelayCommand<object>((a) =>
            {
                FilterPanelVisible = false;
                FilterAdditionalPanelVisible = true;
            });

            CloseAdditionalFiltersCommand = new RelayCommand<object>((a) =>
            {
                FilterAdditionalPanelVisible = false;
                FilterPanelVisible = true;
            });

            CloseAdditionalFilterCommand = new RelayCommand<object>((a) =>
            {
                ((IDisposable)SubFilterControl).Dispose();
                SubFilterControl = null;
                FilterAdditionalPanelVisible = true;
            });

            CloseSubFilterCommand = new RelayCommand<object>((a) =>
            {
                if (SubFilterControl != null)
                {
                    FilterPanelVisible = true;
                    ((IDisposable)SubFilterControl).Dispose();
                    SubFilterControl = null;
                }
            });

            ToggleFiltersCommand = new RelayCommand<object>((a) =>
            {
                if (SubFilterVisible)
                {
                    ((IDisposable)SubFilterControl).Dispose();
                    SubFilterControl = null;
                    FilterPanelVisible = false;
                }
                else if (FilterAdditionalPanelVisible)
                {
                    FilterAdditionalPanelVisible = false;
                }
                else
                {
                    FilterPanelVisible = !FilterPanelVisible;
                }

                if (FilterPanelVisible)
                {
                    GameListFocused = false;
                }
                else
                {
                    GameListFocused = true;
                }
            });

            ClearFiltersCommand = new RelayCommand<object>((a) =>
            {
                SetQuickFilter(AppSettings.Fullscreen);
            });

            ClearNotificationsCommand = new RelayCommand<object>((a) =>
            {
                PlayniteApi.Notifications.RemoveAll();
                NotificationsVisible = false;
                GameListFocused = true;
            });

            ActivateSelectedCommand = new RelayCommand<object>((a) =>
            {
                if (GameDetailsVisible)
                {
                    if (GameDetailsEntry?.IsInstalled == true)
                    {
                        GamesEditor.PlayGame(GameDetailsEntry.Game);
                    }
                    else if (GameDetailsEntry?.IsInstalled == false)
                    {
                        GamesEditor.InstallGame(GameDetailsEntry.Game);
                    }
                }
                else
                {
                    if (SelectedGame?.IsInstalled == true)
                    {
                        GamesEditor.PlayGame(SelectedGame.Game);
                    }
                    else if (SelectedGame?.IsInstalled == false)
                    {
                        GamesEditor.InstallGame(SelectedGame.Game);
                    }
                }
            }, (a) => Database?.IsOpen == true);

            OpenSearchCommand = new RelayCommand<object>((a) =>
            {
                GameListFocused = false;
                var test = new Windows.TextInputWindow();
                test.PropertyChanged += SearchText_PropertyChanged;
                test.ShowInput(WindowManager.CurrentWindow, "", "", AppSettings.Fullscreen.FilterSettings.Name);
                test.PropertyChanged -= SearchText_PropertyChanged;
                GameListFocused = true;
            });

            NextFilterViewCommand = new RelayCommand<object>((a) =>
            {
                var max = AppSettings.Fullscreen.ActiveView.GetMax();
                var next = (int)AppSettings.Fullscreen.ActiveView + 1;
                if (next <= max)
                {
                    AppSettings.Fullscreen.ActiveView = (ActiveFullscreenView)next;
                }
            }, (a) => Database?.IsOpen == true);

            PrevFilterViewCommand = new RelayCommand<object>((a) =>
            {
                var min = AppSettings.Fullscreen.ActiveView.GetMin();
                var prev = (int)AppSettings.Fullscreen.ActiveView - 1;
                if (prev >= 0)
                {
                    AppSettings.Fullscreen.ActiveView = (ActiveFullscreenView)prev;
                }
            }, (a) => Database?.IsOpen == true);

            SelectPrevGameCommand = new RelayCommand<object>((a) =>
            {
                var currIndex = GamesView.CollectionView.IndexOf(GameDetailsEntry);
                var prevIndex = currIndex - 1;
                if (prevIndex >= 0)
                {
                    GameDetailsFocused = false;
                    GameDetailsEntry = GamesView.CollectionView.GetItemAt(prevIndex) as GamesCollectionViewEntry;
                    GameDetailsFocused = true;
                }
            }, (a) => Database?.IsOpen == true);

            SelectNextGameCommand = new RelayCommand<object>((a) =>
            {
                var currIndex = GamesView.CollectionView.IndexOf(GameDetailsEntry);
                var nextIndex = currIndex + 1;
                if (nextIndex < GamesView.CollectionView.Count)
                {
                    GameDetailsFocused = false;
                    GameDetailsEntry = GamesView.CollectionView.GetItemAt(nextIndex) as GamesCollectionViewEntry;
                    GameDetailsFocused = true;
                }
            }, (a) => Database?.IsOpen == true);

            FileDroppedCommand = new RelayCommand<DragEventArgs>((args) =>
            {
                OnFileDropped(args);
            });

            SelectRandomGameCommand = new RelayCommand<object>((a) =>
            {
                PlayRandomGame();
            }, (a) => Database?.IsOpen == true,
            new KeyGesture(Key.F6));

            UpdateGamesCommand = new RelayCommand<object>(async (a) =>
            {
                if (MainMenuVisible)
                {
                    ToggleMainMenuCommand.Execute(null);
                }

                await UpdateDatabase(AppSettings.DownloadMetadataOnImport);
            }, (a) => !DatabaseUpdateRunning);
        }

        private GamesCollectionViewEntry SelectClosestGameDetails()
        {
            var focusIndex = -1;
            if (lastGameDetailsIndex == 0 && GamesView.CollectionView.Count > 0)
            {
                focusIndex = 0;
            }
            else if (lastGameDetailsIndex > 0 && GamesView.CollectionView.Count < lastGameDetailsIndex && GamesView.CollectionView.Count > 0)
            {
                focusIndex = GamesView.CollectionView.Count + 1;
            }
            else
            {
                focusIndex = lastGameDetailsIndex - 1;
            }

            if (focusIndex > -1)
            {
                GameDetailsFocused = false;
                GameDetailsEntry = GamesView.CollectionView.GetItemAt(focusIndex) as GamesCollectionViewEntry;
                GameDetailsFocused = true;
                return GameDetailsEntry;
            }
            else
            {
                ToggleGameDetailsCommand.Execute(null);
                return null;
            }
        }

        private void SearchText_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Windows.TextInputWindow.InputText))
            {
                var input = sender as Windows.TextInputWindow;
                if (AppSettings.Fullscreen.FilterSettings.Name != input.InputText)
                {
                    if (AppSettings.Fullscreen.FilterSettings.Name.IsNullOrEmpty() && input.InputText.IsNullOrEmpty())
                    {
                        return;
                    }

                    AppSettings.Fullscreen.FilterSettings.Name = input.InputText;
                }
            }
        }

        private void OpenSubEnumFilter(string title, Type enumType, string filterPropertiesPath, bool isPrimaryFilter = false)
        {
            SubFilterControl = new FilterEnumListSelection(this, isPrimaryFilter)
            {
                Title = title.StartsWith("LOC") ? ResourceProvider.GetString(title) : title,
                EnumType = enumType
            };

            BindingOperations.SetBinding(SubFilterControl, FilterEnumListSelection.FilterPropertiesProperty, new Binding()
            {
                Source = AppSettings.Fullscreen.FilterSettings,
                Path = new PropertyPath(filterPropertiesPath),
                Mode = BindingMode.TwoWay
            });

            FilterAdditionalPanelVisible = false;
            FilterPanelVisible = false;
        }

        private void OpenSubStringFilter(string title, string itemsListPath, string filterPropertiesPath, bool isPrimaryFilter = false)
        {
            SubFilterControl = new FilterStringListSelection(this, isPrimaryFilter)
            {
                Title = title.StartsWith("LOC") ? ResourceProvider.GetString(title) : title
            };

            BindingOperations.SetBinding(SubFilterControl, FilterStringListSelection.ItemsListProperty, new Binding()
            {
                Source = DatabaseFilters,
                Path = new PropertyPath(itemsListPath)
            });

            BindingOperations.SetBinding(SubFilterControl, FilterStringListSelection.FilterPropertiesProperty, new Binding()
            {
                Source = AppSettings.Fullscreen.FilterSettings,
                Path = new PropertyPath(filterPropertiesPath),
                Mode = BindingMode.TwoWay
            });

            FilterAdditionalPanelVisible = false;
            FilterPanelVisible = false;
        }

        private void OpenSubFilter(string title, string itemsListPath, string filterPropertiesPath, bool isPrimaryFilter = false)
        {
            SubFilterControl = new FilterDbItemtSelection(this, isPrimaryFilter)
            {
                Title = title.StartsWith("LOC") ? ResourceProvider.GetString(title) : title
            };

            BindingOperations.SetBinding(SubFilterControl, FilterDbItemtSelection.ItemsListProperty, new Binding()
            {
                Source = DatabaseFilters,
                Path = new PropertyPath(itemsListPath)
            });

            BindingOperations.SetBinding(SubFilterControl, FilterDbItemtSelection.FilterPropertiesProperty, new Binding()
            {
                Source = AppSettings.Fullscreen.FilterSettings,
                Path = new PropertyPath(filterPropertiesPath),
                Mode = BindingMode.TwoWay
            });

            FilterAdditionalPanelVisible = false;
            FilterPanelVisible = false;
        }

        public void OpenView()
        {
            Window.Show(this);
            SetViewSizeAndPosition(IsFullScreen);
            application.UpdateScreenInformation(Window.Window);
            Window.Window.LocationChanged += Window_LocationChanged;
            InitializeView();
        }

        public void CloseView()
        {
            ignoreCloseActions = true;
            Window.Close();
            Dispose();
            ignoreCloseActions = false;
        }

        public void Shutdown()
        {
            CloseView();
            application.Quit();
        }

        public void ToggleFullscreen()
        {
            if (IsFullScreen)
            {
                IsFullScreen = false;
                SetViewSizeAndPosition(IsFullScreen);
            }
            else
            {
                IsFullScreen = true;
                SetViewSizeAndPosition(IsFullScreen);
            }
        }

        public void SetViewSizeAndPosition(bool fullscreen)
        {
            var screenIndex = AppSettings.Fullscreen.Monitor;
            var screens = Computer.GetScreens();
            if (screenIndex + 1 > screens.Count || screenIndex < 0)
            {
                screenIndex = 0;
            }

            var screen = screens[screenIndex];
            var ratio = Sizes.GetAspectRatio(screen.Bounds);
            ViewportWidth = ratio.GetWidth(ViewportHeight);
            var dpi = VisualTreeHelper.GetDpi(Window.Window);
            if (fullscreen)
            {
                WindowLeft = screen.Bounds.X / dpi.DpiScaleX;
                WindowTop = screen.Bounds.Y / dpi.DpiScaleY;
                WindowWidth = screen.Bounds.Width / dpi.DpiScaleX;
                WindowHeight = screen.Bounds.Height / dpi.DpiScaleY;
            }
            else
            {
                WindowWidth = screen.Bounds.Width / 1.5;
                WindowHeight = screen.Bounds.Height / 1.5;
                WindowLeft = screen.Bounds.X + ((screen.Bounds.Width - WindowWidth) / 2);
                WindowTop = screen.Bounds.Y + ((screen.Bounds.Height - WindowHeight) / 2);
            }
        }

        protected void InitializeView()
        {
            DatabaseFilters = new DatabaseFilter(Database, Extensions, AppSettings.Fullscreen.FilterSettings);
            DatabaseExplorer = new DatabaseExplorer(Database, Extensions, AppSettings);
            var openProgress = new ProgressViewViewModel(new ProgressWindowFactory(),
            () =>
            {
                if (!Database.IsOpen)
                {
                    Database.SetDatabasePath(AppSettings.DatabasePath);
                    Database.OpenDatabase();
                }
            }, Resources.GetString("LOCOpeningDatabase"));

            if (openProgress.ActivateProgress() != true)
            {
                Logger.Error(openProgress.FailException, "Failed to open library database.");
                var message = Resources.GetString("LOCDatabaseOpenError") + $"\n{openProgress.FailException.Message}";
                Dialogs.ShowErrorMessage(message, "");
                return;
            }

            GamesView = new FullscreenCollectionView(Database, AppSettings, Extensions);
            BindingOperations.EnableCollectionSynchronization(GamesView.Items, gamesLock);
            if (GamesView.CollectionView.Count > 0)
            {
                SelectGameIndex(0);
            }
            else
            {
                SelectedGame = null;
            }

            GameListFocused = true;
            isInitialized = true;
            Extensions.NotifiyOnApplicationStarted();
        }

        public void SelectGame(Guid id)
        {
            var viewEntry = GamesView.Items.FirstOrDefault(a => a.Game.Id == id);
            SelectedGame = viewEntry;
        }

        public void SelectGameIndex(int index)
        {
            if (!Database.IsOpen)
            {
                return;
            }

            if (GamesView.CollectionView.Count > index)
            {
                var viewEntry = GamesView.CollectionView.GetItemAt(index) as GamesCollectionViewEntry;
                SelectedGame = viewEntry;
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

            try
            {
                DatabaseUpdateRunning = true;
                GlobalTaskHandler.CancelToken = new CancellationTokenSource();
                GlobalTaskHandler.ProgressTask = Task.Run(async () =>
                {
                    DatabaseFilters.IgnoreDatabaseUpdates = true;
                    var addedGames = new List<Game>();
                    ProgressVisible = true;
                    ProgressValue = 0;
                    ProgressTotal = 1;

                    foreach (var plugin in Extensions.LibraryPlugins)
                    {
                        Logger.Info($"Importing games from {plugin.Name} plugin.");
                        ProgressStatus = string.Format(Resources.GetString("LOCProgressImportinGames"), plugin.Name);

                        try
                        {
                            using (Database.BufferedUpdate())
                            {
                                addedGames.AddRange(Database.ImportGames(plugin, AppSettings.ForcePlayTimeSync));
                            }

                            PlayniteApi.Notifications.Remove($"{plugin.Id} - download");
                        }
                        catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                        {
                            Logger.Error(e, $"Failed to import games from plugin: {plugin.Name}");
                            PlayniteApi.Notifications.Add(new NotificationMessage(
                                $"{plugin.Id} - download",
                                string.Format(Resources.GetString("LOCLibraryImportError"), plugin.Name) + $"\n{e.Message}",
                                NotificationType.Error));
                        }
                    }

                    ProgressStatus = Resources.GetString("LOCProgressLibImportFinish");
                    await Task.Delay(500);

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
                DatabaseUpdateRunning = false;
                ProgressVisible = false;
                DatabaseFilters.IgnoreDatabaseUpdates = false;
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
                            try
                            {
                                var desc = ThemeManager.GetDescriptionFromPackedFile(path);
                                if (desc == null)
                                {
                                    throw new FileNotFoundException("Theme manifest not found.");
                                }

                                if (new Version(desc.ThemeApiVersion).Major != ThemeManager.GetApiVersion(desc.Mode).Major)
                                {
                                    throw new Exception(Resources.GetString("LOCGeneralExtensionInstallApiVersionFails"));
                                }

                                if (Dialogs.ShowMessage(
                                        string.Format(Resources.GetString("LOCThemeInstallPrompt"),
                                            desc.Name, desc.Author, desc.Version),
                                        Resources.GetString("LOCGeneralExtensionInstallTitle"),
                                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                {
                                    ExtensionInstaller.QueueExetnsionInstall(path);
                                    if (Dialogs.ShowMessage(
                                        Resources.GetString("LOCExtInstallationRestartNotif"),
                                        Resources.GetString("LOCSettingsRestartTitle"),
                                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                    {
                                        application.Restart(new CmdLineOptions()
                                        {
                                            SkipLibUpdate = true,
                                        });
                                    };
                                }
                            }
                            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                            {
                                Logger.Error(e, "Failed to install theme.");
                                Dialogs.ShowErrorMessage(
                                    string.Format(Resources.GetString("LOCThemeInstallFail"), e.Message), "");
                            }
                        }
                    }
                }
            }
        }

        public void PlayRandomGame()
        {
            if (MainMenuVisible)
            {
                ToggleMainMenuCommand.Execute(null);
            }

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
                    GamesEditor.PlayGame(selection.Game);
                }
            }
        }

        public void RestoreWindow()
        {
            Window.RestoreWindow();
        }

        public void MinimizeWindow()
        {
            Window.Window.WindowState = WindowState.Minimized;
        }

        public void Dispose()
        {
            GamesView?.Dispose();
            Window.Window.LocationChanged -= Window_LocationChanged;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            application.UpdateScreenInformation(Window.Window);
        }
    }
}
