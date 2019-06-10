using Playnite.API;
using Playnite.Commands;
using Playnite.Common;
using Playnite.Controls;
using Playnite.Database;
using Playnite.FullscreenApp.Controls;
using Playnite.FullscreenApp.Controls.Views;
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
                    SelectedGameDetails = new GameDetailsViewModel(value, Resources, GamesEditor, this);
                }

                selectedGame = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(GameDetailsButtonVisible));
                OnPropertyChanged(nameof(GameDetailsEntry));
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
                OnPropertyChanged(nameof(GameDetailsEntry));
            }
        }

        public GamesCollectionViewEntry GameDetailsEntry => GameDetailsVisible ? SelectedGame : null;
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
        public RelayCommand<object> OpenPatreonCommand { get; private set; }
        public RelayCommand<object> ClearFiltersCommand { get; private set; }
        public RelayCommand<object> OpenAdditionalFiltersCommand { get; private set; }
        public RelayCommand<object> CloseAdditionalFiltersCommand { get; private set; }
        public RelayCommand<object> InstallSelectedCommand { get; private set; }
        public RelayCommand<object> PlaySelectedCommand { get; private set; }
        public RelayCommand<object> OpenSearchCommand { get; private set; }
        public RelayCommand<object> NextFilterViewCommand { get; private set; }
        public RelayCommand<object> PrevFilterViewCommand { get; private set; }
        public RelayCommand<object> SelectPrevGameCommand { get; private set; }
        public RelayCommand<object> SelectNextGameCommand { get; private set; }
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
            SetViewSizeAndPosition(IsFullScreen);
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
                    settings.ViewSettings.SortingOrderDirection = SortOrderDirection.Descending;
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
                    settings.ViewSettings.SortingOrderDirection = SortOrderDirection.Descending;
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
                var cmdline = new CmdLineOptions() { SkipLibUpdate = true };
                ProcessStarter.StartProcess(PlaynitePaths.DesktopExecutablePath, cmdline.ToString());
                application.Quit();
            });

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

            RestartSystemCommand = new RelayCommand<object>((a) =>
            {
                if (!PlayniteEnvironment.IsDebuggerAttached)
                {
                    Computer.Restart();
                }
            });

            OpenPatreonCommand = new RelayCommand<object>((a) =>
            {
                ProcessStarter.StartUrl(@"https://www.patreon.com/playnite");
            });

            LoadSubFilterCommand = new RelayCommand<GameField>((gameField) =>
            {
                switch (gameField)
                {
                    case GameField.PluginId:
                        OpenSubFilter("Library", nameof(DatabaseFilter.Libraries), nameof(FilterSettings.Library), true);
                        break;
                    case GameField.Categories:
                        OpenSubFilter("Categories", nameof(DatabaseFilter.Categories), nameof(FilterSettings.Category), true);
                        break;
                    case GameField.Platform:
                        OpenSubFilter("Platforms", nameof(DatabaseFilter.Platforms), nameof(FilterSettings.Platform), true);
                        break;
                    case GameField.CompletionStatus:
                        OpenSubEnumFilter("Completion Status", typeof(CompletionStatus), nameof(FilterSettings.CompletionStatus));
                        break;
                    case GameField.ReleaseYear:
                        OpenSubStringFilter("Release Year", nameof(DatabaseFilter.ReleaseYears), nameof(FilterSettings.ReleaseYear));
                        break;
                    case GameField.Genres:
                        OpenSubFilter("Genres", nameof(DatabaseFilter.Genres), nameof(FilterSettings.Genre));
                        break;
                    case GameField.Developers:
                        OpenSubFilter("Developers", nameof(DatabaseFilter.Developers), nameof(FilterSettings.Developer));
                        break;
                    case GameField.Publishers:
                        OpenSubFilter("Publishers", nameof(DatabaseFilter.Publishers), nameof(FilterSettings.Publisher));
                        break;
                    case GameField.Tags:
                        OpenSubFilter("Tags", nameof(DatabaseFilter.Tags), nameof(FilterSettings.Tag));
                        break;
                    case GameField.Playtime:
                        OpenSubEnumFilter("Playtime", typeof(PlaytimeCategory), nameof(FilterSettings.PlayTime));
                        break;
                    case GameField.Series:
                        OpenSubFilter("Series", nameof(DatabaseFilter.Series), nameof(FilterSettings.Series));
                        break;
                    case GameField.Region:
                        OpenSubFilter("Region", nameof(DatabaseFilter.Regions), nameof(FilterSettings.Region));
                        break;
                    case GameField.Source:
                        OpenSubFilter("Source", nameof(DatabaseFilter.Sources), nameof(FilterSettings.Source));
                        break;
                    case GameField.AgeRating:
                        OpenSubFilter("AgeRating", nameof(DatabaseFilter.AgeRatings), nameof(FilterSettings.AgeRating));
                        break;
                    case GameField.UserScore:
                        OpenSubEnumFilter("UserScore", typeof(ScoreGroup), nameof(FilterSettings.UserScore));
                        break;
                    case GameField.CommunityScore:
                        OpenSubEnumFilter("CommunityScore", typeof(ScoreGroup), nameof(FilterSettings.CommunityScore));
                        break;
                    case GameField.CriticScore:
                        OpenSubEnumFilter("CriticScore", typeof(ScoreGroup), nameof(FilterSettings.CriticScore));
                        break;
                    case GameField.LastActivity:
                        OpenSubEnumFilter("LastActivity", typeof(PastTimeSegment), nameof(FilterSettings.LastActivity));
                        break;
                    case GameField.Added:
                        OpenSubEnumFilter("Added", typeof(PastTimeSegment), nameof(FilterSettings.Added));
                        break;
                    case GameField.Modified:
                        OpenSubEnumFilter("Modified", typeof(PastTimeSegment), nameof(FilterSettings.Modified));
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

            InstallSelectedCommand = new RelayCommand<object>((a) =>
            {
                GamesEditor.InstallGame(SelectedGame.Game);
            }, (a) => SelectedGame?.IsInstalled == false);

            PlaySelectedCommand = new RelayCommand<object>((a) =>
            {
                GamesEditor.PlayGame(SelectedGame.Game);
            }, (a) => SelectedGame?.IsInstalled == true);

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
            });

            PrevFilterViewCommand = new RelayCommand<object>((a) =>
            {
                var min = AppSettings.Fullscreen.ActiveView.GetMin();
                var prev = (int)AppSettings.Fullscreen.ActiveView - 1;
                if (prev >= 0)
                {
                    AppSettings.Fullscreen.ActiveView = (ActiveFullscreenView)prev;
                }
            });

            SelectPrevGameCommand = new RelayCommand<object>((a) =>
            {
                var currIndex = GamesView.CollectionView.IndexOf(SelectedGame);
                var prevIndex = currIndex - 1;
                if (prevIndex >= 0)
                {
                    SelectGameIndex(prevIndex);
                }
            });

            SelectNextGameCommand = new RelayCommand<object>((a) =>
            {
                var currIndex = GamesView.CollectionView.IndexOf(SelectedGame);
                var nextIndex = currIndex + 1;
                if (nextIndex <= GamesView.CollectionView.Count)
                {
                    SelectGameIndex(nextIndex);
                }
            });
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
                Title = title,
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
                Title = title
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
                Title = title
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
            var screens = Computer.GetMonitors();
            if (screenIndex + 1 > screens.Count)
            {
                screenIndex = 0;
            }

            var screen = screens[screenIndex];
            var ratio = Sizes.GetAspectRatio(screen.Bounds);
            ViewportWidth = ratio.GetWidth(ViewportHeight);
            if (fullscreen)
            {
                WindowWidth = screen.Bounds.Width;
                WindowHeight = screen.Bounds.Height;
                WindowLeft = screen.Bounds.X;
                WindowTop = screen.Bounds.Y;
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
                GlobalTaskHandler.CancelToken = new CancellationTokenSource();
                GlobalTaskHandler.ProgressTask = Task.Run(async () =>
                {
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
                        using (var downloader = new MetadataDownloader(Database, Extensions.LibraryPlugins))
                        {
                            downloader.DownloadMetadataGroupedAsync(addedGames, AppSettings.DefaultMetadataSettings,
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
                ProgressVisible = false;
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
        }
    }
}
