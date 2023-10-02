using Playnite.API;
using Playnite.Commands;
using Playnite.Common;
using Playnite.Controls;
using Playnite.Database;
using Playnite.FullscreenApp.Controls;
using Playnite.FullscreenApp.Controls.Views;
using Playnite.FullscreenApp.Windows;
using Playnite.Input;
using Playnite.Metadata;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Playnite.FullscreenApp.ViewModels
{
    public partial class FullscreenAppViewModel : MainViewModelBase, IDisposable, IMainViewModelBase
    {
        private static object gamesLock = new object();
        private readonly SynchronizationContext context;
        private bool isInitialized = false;
        protected bool ignoreCloseActions = false;

        public GamesEditor GamesEditor { get; }
        public bool IsFullScreen { get; private set; } = true;
        public ObservableTime CurrentTime { get; } = new ObservableTime();
        public ObservablePowerStatus PowerStatus { get; } = new ObservablePowerStatus();

        private GameStatusViewModel gameStatusView;
        public GameStatusViewModel GameStatusView
        {
            get => gameStatusView;
            set
            {
                gameStatusView = value;
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

        internal int LastValidSelectedGameIndex;

        public List<GamesCollectionViewEntry> SelectedGames
        {
            get => SelectedGame != null ? null : new List<GamesCollectionViewEntry>(1) { SelectedGame };
            set
            {
                if (SelectedGames.HasItems())
                {
                    SelectedGame = value[0];
                }
                else
                {
                    SelectedGame = null;
                }
            }
        }

        private GamesCollectionViewEntry selectedGame;
        public GamesCollectionViewEntry SelectedGame
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
                    LastValidSelectedGameIndex = GamesView.CollectionView.IndexOf(value);
                    SelectedGameDetails = new GameDetailsViewModel(value, Resources, GamesEditor, this, Dialogs);
                }

                selectedGame = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(GameDetailsButtonVisible));

                if (!IsDisposing)
                {
                    Extensions.InvokeOnGameSelected(
                        oldValue == null ? null : new List<Game> { oldValue.Game },
                        selectedGame == null ? null : new List<Game> { selectedGame.Game });
                }
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
            }
        }

        private bool gameStatusVisible = false;
        public bool GameStatusVisible
        {
            get => gameStatusVisible;
            set
            {
                gameStatusVisible = value;
                OnPropertyChanged();
            }
        }

        public bool GameDetailsButtonVisible => GameDetailsVisible == false && SelectedGame != null;

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

        private bool GenerateAudio { get; set; } = true;

        private bool childOpened = false;
        public bool ChildOpened
        {
            get => childOpened;
            set
            {
                childOpened = value;
                GenerateAudio = !value;
                if (value == false)
                {
                    // Super ugly hack to remove posibility of window dimming not showing up for a moment,
                    // when child window opens another child window and also closes itself.
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(10);
                        context.Send((_) => OnPropertyChanged(), null);
                    });
                }
                else
                {
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSearchActive
        {
            get => !AppSettings.Fullscreen.FilterSettings.Name.IsNullOrEmpty();
        }

        public bool IsExtraFilterActive
        {
            get
            {
                if (ActiveFilterPreset != null)
                {
                    return false;
                }

                if (!AppSettings.Fullscreen.FilterSettings.Name.IsNullOrEmpty())
                {
                    return false;
                }

                if (AppSettings.Fullscreen.FilterSettings.IsActive && ActiveFilterPreset == null)
                {
                    return true;
                }
                else if (ActiveFilterPreset != null)
                {
                    var preset = ActiveFilterPreset.Settings.GetClone();
                    preset.Name = null;
                    var current = AppSettings.Fullscreen.FilterSettings.GetClone();
                    current.Name = null;
                    return !preset.IsEqualJson(current);
                }

                return false;
            }
        }

        /// <summary>
        /// This constructor should be used on from <see cref="DesignMainViewModel"/> for Blend usage!
        /// </summary>
        public FullscreenAppViewModel(
            IGameDatabaseMain database,
            PlayniteApplication app,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            ExtensionFactory extensions) : base(database, app, dialogs, resources, extensions, null)
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
            ExtensionFactory extensions,
            PlayniteApplication app) : base(database, app, dialogs, resources, extensions, window)
        {
            context = SynchronizationContext.Current;
            GamesEditor = gamesEditor;
            AppSettings = settings;
            IsFullScreen = !PlayniteEnvironment.IsDebuggerAttached;
            settings.Fullscreen.PropertyChanged += Fullscreen_PropertyChanged;
            settings.Fullscreen.FilterSettings.FilterChanged += FilterSettings_FilterChanged;
            ThemeManager.ApplyFullscreenButtonPrompts(PlayniteApplication.CurrentNative, AppSettings.Fullscreen.ButtonPrompts);
            InitializeCommands();
            UpdateCursorSettings();
            EventManager.RegisterClassHandler(typeof(WindowBase), WindowBase.ClosedRoutedEvent, new RoutedEventHandler(WindowBaseCloseHandler));
            EventManager.RegisterClassHandler(typeof(WindowBase), WindowBase.LoadedRoutedEvent, new RoutedEventHandler(WindowBaseLoadedHandler));
            EventManager.RegisterClassHandler(typeof(CheckBox), CheckBox.CheckedEvent, new RoutedEventHandler(ElemestateChangedHander));
            EventManager.RegisterClassHandler(typeof(CheckBox), CheckBox.UncheckedEvent, new RoutedEventHandler(ElemestateChangedHander));
            EventManager.RegisterClassHandler(typeof(Slider), Slider.ValueChangedEvent, new RoutedEventHandler(ElemestateChangedHander));
            EventManager.RegisterClassHandler(typeof(UIElement), UIElement.GotFocusEvent, new RoutedEventHandler(ElementGotFocusHandler));
            app.Controllers.Started += Controllers_Started;
            app.Controllers.Starting += Controllers_Starting;
            app.Controllers.Stopped += Controllers_Stopped;
            app.Controllers.StartupCancelled += Controllers_StartupCancelled;
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            Logger.Info("Detected screen settings changes, adjusting window.");
            SetViewSizeAndPosition(IsFullScreen);
            AdjustGameItemsToScreenChanges();
        }

        private void XInputDevice_ButtonUp(object sender, XInputDevice.ButtonUpEventArgs e)
        {
            if (AppSettings.Fullscreen.EnableXinputProcessing &&
                AppSettings.Fullscreen.GuideButtonFocus &&
                e.Button == XInputButton.Guide)
            {
                WindowManager.LastActiveWindow?.RestoreWindow();
            }
        }

        private void Controllers_StartupCancelled(object sender, OnGameStartupCancelledEventArgs e)
        {
            Controllers_Stopped(sender, new GameStoppedEventArgs());
        }

        private void Controllers_Stopped(object sender, GameStoppedEventArgs e)
        {
            if (GameStatusVisible)
            {
                GameStatusVisible = false;
                if (GameDetailsVisible)
                {
                    GameDetailsFocused = true;
                }
                else
                {
                    GameListFocused = true;
                }
            }

            GameStatusView = null;
        }

        private void Controllers_Starting(object sender, OnGameStartingEventArgs e)
        {
            if (GameDetailsVisible)
            {
                GameDetailsFocused = false;
            }
            else
            {
                GameListFocused = false;
            }

            if (SelectedGame?.Game.Id == e.Game.Id)
            {
                GameStatusView = new GameStatusViewModel(SelectedGame);
            }
            else
            {
                GameStatusView = new GameStatusViewModel(new GamesCollectionViewEntry(
                    e.Game,
                    GamesView.GetLibraryPlugin(e.Game),
                    AppSettings));
            }

            GameStatusVisible = true;
            GameStatusView.GameStatusText = ResourceProvider.GetString(LOC.GameIsStarting).Format(e.Game.Name);
        }

        private void Controllers_Started(object sender, GameStartedEventArgs e)
        {
            if (GameStatusView != null)
            {
                GameStatusView.GameStatusText = ResourceProvider.GetString(LOC.GameIsRunning).Format(e.Source.Game.Name);
            }
        }

        private void ElementGotFocusHandler(object sender, RoutedEventArgs e)
        {
            // This prevents "double-click" sounds when using mouse to open child menus.
            // There's probably a better way how to detect if focus was caused by mouse input, but I haven't found it.
            var mouseInput = InputManager.Current?.PrimaryMouseDevice;
            if (mouseInput != null && mouseInput.LeftButton == MouseButtonState.Pressed)
            {
                if (e.OriginalSource is ListBoxItem listItem && listItem.DataContext is GamesCollectionViewEntry)
                {
                    // Click selecting a game on the main view is an exception
                }
                else
                {
                    return;
                }
            }

            if (sender is UIElement elem && elem.IsVisible)
            {
                switch (sender)
                {
                    case Button _:
                    case ListBoxItem _:
                    case CheckBox _:
                    case Slider _:
                    case ComboBox _:
                    case TextBox _:
                    case HtmlTextView _:
                        FullscreenApplication.PlayNavigateSound();
                        break;
                }
            }
        }

        private void ElemestateChangedHander(object sender, RoutedEventArgs e)
        {
            if (sender is UIElement check && check.IsFocused)
            {
                FullscreenApplication.PlayActivateSound();
            }
        }
        private void WindowBaseCloseHandler(object sender, RoutedEventArgs e)
        {
            ChildOpened = Window.Window.HasChildWindow;
        }

        private void WindowBaseLoadedHandler(object sender, RoutedEventArgs e)
        {
            ChildOpened = Window.Window.HasChildWindow;
        }

        private void Fullscreen_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FullscreenSettings.Monitor) || e.PropertyName == nameof(FullscreenSettings.UsePrimaryDisplay))
            {
                SetViewSizeAndPosition(IsFullScreen);
            }
            else if (e.PropertyName == nameof(FullscreenSettings.ButtonPrompts))
            {
                ThemeManager.ApplyFullscreenButtonPrompts(PlayniteApplication.CurrentNative, AppSettings.Fullscreen.ButtonPrompts);
            }
            else if (e.PropertyName == nameof(FullscreenSettings.HideMouserCursor))
            {
                UpdateCursorSettings();
            }
            else if (e.PropertyName == nameof(FullscreenSettings.EnableXinputProcessing))
            {
                if (App.XInputDevice != null)
                {
                    App.XInputDevice.StandardProcessingEnabled = AppSettings.Fullscreen.EnableXinputProcessing;
                }
            }
            else if (e.PropertyName == nameof(FullscreenSettings.BackgroundVolume))
            {
                if (AppSettings.Fullscreen.BackgroundVolume == 0)
                {
                    FullscreenApplication.StopBackgroundSound();
                }
                else
                {
                    FullscreenApplication.PlayBackgroundSound();
                }
            }
            else if (e.PropertyName == nameof(FullscreenSettings.IsMusicMuted))
            {
                FullscreenApplication.SetBackgroundSoundVolume(AppSettings.Fullscreen.IsMusicMuted ? 0f : AppSettings.Fullscreen.BackgroundVolume);
            }
            else if (e.PropertyName == nameof(FullscreenSettings.PrimaryControllerOnly))
            {
                if (App.XInputDevice != null)
                {
                    App.XInputDevice.PrimaryControllerOnly = AppSettings.Fullscreen.PrimaryControllerOnly;
                }
            }

            if (e.PropertyName == nameof(FullscreenSettings.HorizontalLayout) ||
                e.PropertyName == nameof(FullscreenSettings.Columns) ||
                e.PropertyName == nameof(FullscreenSettings.Rows) ||
                e.PropertyName == nameof(FullscreenSettings.ImageScalerMode) ||
                e.PropertyName == nameof(FullscreenSettings.UsePrimaryDisplay) ||
                e.PropertyName == nameof(FullscreenSettings.Monitor))
            {
                AdjustGameItemsToScreenChanges();
            }

            if (e.PropertyName == nameof(FullscreenSettings.SwapConfirmCancelButtons))
            {
                App.UpdateXInputBindings();
            }
        }

        private void FilterSettings_FilterChanged(object sender, FilterChangedEventArgs e)
        {
            if (!IgnoreFilterChanges)
            {
                ActiveFilterPreset = null;
            }

            OnPropertyChanged(nameof(IsSearchActive));
            OnPropertyChanged(nameof(IsExtraFilterActive));
        }

        private void AdjustGameItemsToScreenChanges()
        {
            var oldSettings = GamesCollectionViewEntry.FullscreenListCoverProperties;
            GamesCollectionViewEntry.InitItemViewProperties(App, AppSettings);
            if (oldSettings != GamesCollectionViewEntry.FullscreenListCoverProperties)
            {
                GamesView.NotifyItemPropertyChanges(
                    nameof(GamesCollectionViewEntry.FullscreenListItemCoverObject),
                    nameof(GamesCollectionViewEntry.DefaultFullscreenListItemCoverObject));
            }
        }

        private void UpdateCursorSettings()
        {
            Computer.SetMouseCursorVisibility(!AppSettings.Fullscreen.HideMouserCursor);
            WindowManager.SetEnableMouseInput(!AppSettings.Fullscreen.HideMouserCursor);
        }

        public void OpenMainMenu()
        {
            var vm = new MainMenuViewModel(new MainMenuWindowFactory(), this);
            vm.OpenView();
            GameListFocused = false;
            GameListFocused = true;
        }

        public void OpenNotificationsMenu()
        {
            var vm = new NotificationsViewModel(new NotificationsWindowFactory(), this);
            vm.OpenView();
            GameListFocused = false;
            GameListFocused = true;
        }

        public void OpenGameMenu()
        {
            var vm = new GameMenuViewModel(new GameMenuWindowFactory(), this, SelectedGame.Game, GamesEditor);
            vm.OpenView();
        }

        public void SwitchToDesktopMode()
        {
            Logger.Info("Switching to Desktop mode.");
            if (GlobalTaskHandler.IsActive)
            {
                if (Dialogs.ShowMessage(
                    Resources.GetString(LOC.BackgroundProgressCancelAskSwitchMode),
                    Resources.GetString(LOC.BackToDesktopMode),
                    MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }

                var dialogRes = Dialogs.ActivateGlobalProgress((_) =>
                    {
                        var waitRes = GlobalTaskHandler.CancelAndWait(30_000);
                        if (waitRes == false)
                        {
                            Logger.Error("Active global task failed to finish in time when switching to desktop mode.");
                        }
                    },
                    new GlobalProgressOptions(LOC.OpeningDesktopModeMessage));
                if (dialogRes.Error != null)
                {
                    Logger.Error(dialogRes.Error, "Cancelling global task when switching to desktop mode failed.");
                }
            }

            CloseView();
            App.QuitAndStart(
                PlaynitePaths.DesktopExecutablePath,
                new CmdLineOptions()
                {
                    SkipLibUpdate = true,
                    StartInDesktop = true,
                    MasterInstance = true,
                    SafeStartup = App.CmdLine.SafeStartup
                }.ToString());
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
                Title = title.StartsWith("LOC", StringComparison.Ordinal) ? ResourceProvider.GetString(title) : title,
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
                Title = title.StartsWith("LOC", StringComparison.Ordinal) ? ResourceProvider.GetString(title) : title
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
                Title = title.StartsWith("LOC", StringComparison.Ordinal) ? ResourceProvider.GetString(title) : title
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
            App.UpdateScreenInformation(Window.Window);
            Window.Window.LocationChanged += Window_LocationChanged;
            InitializeView();
        }

        public override void CloseView()
        {
            ignoreCloseActions = true;
            Window.Close();
            Dispose();
            ignoreCloseActions = false;
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
            ComputerScreen screen = null;
            var screens = Computer.GetScreens();
            if (AppSettings.Fullscreen.UsePrimaryDisplay)
            {
                screen = screens.FirstOrDefault(a => a.Primary);
            }
            else
            {
                var screenIndex = AppSettings.Fullscreen.Monitor;
                if (screenIndex + 1 > screens.Count || screenIndex < 0)
                {
                    screenIndex = 0;
                }

                screen = screens[screenIndex];
            }

            if (screen == null)
            {
                screen = screens[0];
            }

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
            if (App.XInputDevice != null)
            {
                App.XInputDevice.ButtonUp += XInputDevice_ButtonUp;
            }

            GamesCollectionViewEntry.InitItemViewProperties(App, AppSettings);
            DatabaseFilters = new DatabaseFilter(Database, Extensions, AppSettings, AppSettings.Fullscreen.FilterSettings);
            DatabaseExplorer = new DatabaseExplorer(Database, Extensions, AppSettings, this);
            var openProgress = new ProgressViewViewModel(
                new ProgressWindowFactory(),
                new GlobalProgressOptions(LOC.OpeningDatabase));

            if (openProgress.ActivateProgress(_ =>
            {
                if (!Database.IsOpen)
                {
                    Database.SetDatabasePath(AppSettings.DatabasePath);
                    Database.OpenDatabase();
                }
            }).Result != true)
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

            OnPropertyChanged(nameof(SortedFilterPresets));
            OnPropertyChanged(nameof(SortedFilterFullscreenPresets));
            if (AppSettings.Fullscreen.SelectedFilterPreset != Guid.Empty)
            {
                ActiveFilterPreset = Database.FilterPresets.FirstOrDefault(a => a.Id == AppSettings.Fullscreen.SelectedFilterPreset);
            }
        }

        public override void SelectGame(Guid id, bool restoreView = false)
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

        public override NotificationMessage GetAddonUpdatesFoundMessage(List<AddonUpdate> updates)
        {
            return new NotificationMessage("AddonUpdateAvailable", Resources.GetString(LOC.AddonUpdatesAvailable), NotificationType.Info, () =>
            {
                new AddonsViewModel(
                        new AddonsUpdateWindowFactory(),
                        Dialogs,
                        Resources,
                        App.ServicesClient,
                        Extensions,
                        AppSettings,
                        App,
                        updates).OpenView();
            });
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
                    }
                }
            }
        }

        public void RestoreWindow()
        {
            WindowManager.LastActiveWindow?.RestoreWindow();
        }

        public void MinimizeWindow()
        {
            Window.Window.WindowState = WindowState.Minimized;
        }

        public void Dispose()
        {
            IsDisposing = true;
            GamesView?.Dispose();
            Window.Window.LocationChanged -= Window_LocationChanged;
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            App.UpdateScreenInformation(Window.Window);
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
                default:
                    Logger.Warn($"Uknown URI command {command}");
                    break;
            }
        }

        public override void OpenSettings(int settingsPageIndex)
        {
        }

        public override void EditGame(Game game)
        {
        }

        public override void AssignCategories(Game game)
        {
        }

        private void SelectFilterPreset()
        {
            if (!Database.FilterPresets.HasItems())
            {
                return;
            }

            if (ItemSelector.SelectSingle<FilterPreset>(
                LOC.SettingsTopPanelFilterPresetsItem,
                "",
                Database.FilterPresets.OrderBy(a => a.Name).Select(a => new SelectableNamedObject<FilterPreset>(a, a.Name)).ToList(),
                out var selectedPreset))
            {
                ActiveFilterPreset = selectedPreset;
            }
        }
    }
}
