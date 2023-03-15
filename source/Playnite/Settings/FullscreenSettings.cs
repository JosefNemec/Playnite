using Newtonsoft.Json;
using Playnite.Audio;
using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Playnite
{
    public enum FullscreenButtonPrompts
    {
        Xbox,
        PlayStation
    }

    public enum ActiveFullscreenView : int
    {
        RecentlyPlayed = 0,
        Favorites = 1,
        MostPlayed = 2,
        All = 3
        //Explore
    }

    public class FullscreenViewSettings : ViewSettingsBase
    {
    }

    public class FullscreenFilterSettings : FilterSettings
    {
        public FullscreenFilterSettings() : base()
        {
            FilterChanged += (s, e) => OnPropertyChanged(nameof(IsSubAdditionalFilterActive));
        }

        [JsonIgnore]
        public bool IsSubAdditionalFilterActive
        {
            get
            {
                return
                    Series?.IsSet == true ||
                    Source?.IsSet == true ||
                    AgeRating?.IsSet == true ||
                    Region?.IsSet == true ||
                    Genre?.IsSet == true ||
                    Publisher?.IsSet == true ||
                    Developer?.IsSet == true ||
                    Tag?.IsSet == true ||
                    Feature?.IsSet == true ||
                    CompletionStatuses?.IsSet == true ||
                    UserScore?.IsSet == true ||
                    CriticScore?.IsSet == true ||
                    CommunityScore?.IsSet == true ||
                    LastActivity?.IsSet == true ||
                    Added?.IsSet == true ||
                    Modified?.IsSet == true ||
                    ReleaseYear?.IsSet == true ||
                    PlayTime?.IsSet == true;
            }
        }
    }

    public class FullscreenSettings : ObservableObject
    {
        [JsonIgnore]
        public List<ComputerScreen> AvailableScreens => Computer.GetScreens();

        [JsonIgnore]
        public List<ThemeManifest> AvailableThemes => ThemeManager.GetAvailableThemes(ApplicationMode.Fullscreen).OrderBy(a => a.Name).ToList();

        [JsonIgnore]
        public const FullscreenButtonPrompts DefaultButtonPrompts = FullscreenButtonPrompts.Xbox;

        private bool isMusicMuted = false;
        [JsonIgnore]
        public bool IsMusicMuted
        {
            get
            {
                return isMusicMuted;
            }

            set
            {
                if (isMusicMuted != value)
                {
                    isMusicMuted = value;
                    OnPropertyChanged();
                }
            }
        }

        private int monitor = Computer.GetGetPrimaryScreenIndex();
        public int Monitor
        {
            get
            {
                return monitor;
            }

            set
            {
                monitor = value;
                OnPropertyChanged();
            }
        }

        private string theme = ThemeManager.DefaultFullscreenThemeId;
        [RequiresRestart]
        public string Theme
        {
            get
            {
                return theme;
            }

            set
            {
                theme = value;
                OnPropertyChanged();
            }
        }

        private int rows = 2;
        public int Rows
        {
            get
            {
                return rows;
            }

            set
            {
                rows = value;
                OnPropertyChanged();
            }
        }

        private int columns = 4;
        public int Columns
        {
            get
            {
                return columns;
            }

            set
            {
                columns = value;
                OnPropertyChanged();
            }
        }

        private bool horizontalLayout = false;
        public bool HorizontalLayout
        {
            get
            {
                return horizontalLayout;
            }

            set
            {
                horizontalLayout = value;
                OnPropertyChanged();
            }
        }

        private bool showBattery = false;
        public bool ShowBattery
        {
            get
            {
                return showBattery;
            }

            set
            {
                showBattery = value;
                OnPropertyChanged();
            }
        }

        private bool showClock = true;
        public bool ShowClock
        {
            get
            {
                return showClock;
            }

            set
            {
                showClock = value;
                OnPropertyChanged();
            }
        }

        private bool showBatteryPercentage = false;
        public bool ShowBatteryPercentage
        {
            get
            {
                return showBatteryPercentage;
            }

            set
            {
                showBatteryPercentage = value;
                OnPropertyChanged();
            }
        }

        private bool showGameTitles = false;
        public bool ShowGameTitles
        {
            get
            {
                return showGameTitles;
            }

            set
            {
                showGameTitles = value;
                OnPropertyChanged();
            }
        }

        private FullscreenButtonPrompts buttonPrompts = DefaultButtonPrompts;
        public FullscreenButtonPrompts ButtonPrompts
        {
            get
            {
                return buttonPrompts;
            }

            set
            {
                buttonPrompts = value;
                OnPropertyChanged();
            }
        }

        private FullscreenFilterSettings filterSettings = new FullscreenFilterSettings();
        public FullscreenFilterSettings FilterSettings
        {
            get
            {
                return filterSettings;
            }

            set
            {
                filterSettings = value;
                OnPropertyChanged();
            }
        }

        private FullscreenViewSettings viewSettings = new FullscreenViewSettings();
        public FullscreenViewSettings ViewSettings
        {
            get
            {
                return viewSettings;
            }

            set
            {
                viewSettings = value;
                OnPropertyChanged();
            }
        }

        private bool darkenUninstalledGamesGrid = false;
        public bool DarkenUninstalledGamesGrid
        {
            get => darkenUninstalledGamesGrid;
            set
            {
                darkenUninstalledGamesGrid = value;
                OnPropertyChanged();
            }
        }

        private bool enableMainBackgroundImage = false;
        public bool EnableMainBackgroundImage
        {
            get
            {
                return enableMainBackgroundImage;
            }

            set
            {
                enableMainBackgroundImage = value;
                OnPropertyChanged();
            }
        }

        private int mainBackgroundImageBlurAmount = 0;
        public int MainBackgroundImageBlurAmount
        {
            get
            {
                return mainBackgroundImageBlurAmount;
            }

            set
            {
                mainBackgroundImageBlurAmount = value;
                OnPropertyChanged();
            }
        }

        private float mainBackgroundImageDarkAmount = 30;
        public float MainBackgroundImageDarkAmount
        {
            get
            {
                return mainBackgroundImageDarkAmount;
            }

            set
            {
                mainBackgroundImageDarkAmount = value;
                OnPropertyChanged();
            }
        }

        private bool usePrimaryDisplay = false;
        public bool UsePrimaryDisplay
        {
            get => usePrimaryDisplay;
            set
            {
                usePrimaryDisplay = value;
                OnPropertyChanged();
            }
        }

        private Guid selectedFilterPreset;
        public Guid SelectedFilterPreset
        {
            get => selectedFilterPreset;
            set
            {
                selectedFilterPreset = value;
                OnPropertyChanged();
            }
        }

        private bool hideMouserCursor = false;
        public bool HideMouserCursor
        {
            get => hideMouserCursor;
            set
            {
                hideMouserCursor = value;
                OnPropertyChanged();
            }
        }

        private bool asyncImageLoading = true;
        [RequiresRestart]
        public bool AsyncImageLoading
        {
            get
            {
                return asyncImageLoading;
            }

            set
            {
                asyncImageLoading = value;
                OnPropertyChanged();
            }
        }

        private bool minimizeAfterGameStartup = true;
        public bool MinimizeAfterGameStartup
        {
            get
            {
                return minimizeAfterGameStartup;
            }

            set
            {
                minimizeAfterGameStartup = value;
                OnPropertyChanged();
            }
        }

        private double fontSize = 22;
        [RequiresRestart]
        public double FontSize
        {
            get
            {
                return fontSize;
            }

            set
            {
                fontSize = value;
                OnPropertyChanged();
            }
        }

        private double fontSizeSmall = 18;
        [RequiresRestart]
        public double FontSizeSmall
        {
            get
            {
                return fontSizeSmall;
            }

            set
            {
                fontSizeSmall = value;
                OnPropertyChanged();
            }
        }

        private bool enableXinputProcessing = true;
        [RequiresRestart]
        public bool EnableXinputProcessing
        {
            get
            {
                return enableXinputProcessing;
            }

            set
            {
                enableXinputProcessing = value;
                OnPropertyChanged();
            }
        }

        private bool mainMenuShowRestart = true;
        public bool MainMenuShowRestart
        {
            get
            {
                return mainMenuShowRestart;
            }

            set
            {
                mainMenuShowRestart = value;
                OnPropertyChanged();
            }
        }

        private bool mainMenuShowShutdown = true;
        public bool MainMenuShowShutdown
        {
            get
            {
                return mainMenuShowShutdown;
            }

            set
            {
                mainMenuShowShutdown = value;
                OnPropertyChanged();
            }
        }

        private bool mainMenuShowSuspend = true;
        public bool MainMenuShowSuspend
        {
            get
            {
                return mainMenuShowSuspend;
            }

            set
            {
                mainMenuShowSuspend = value;
                OnPropertyChanged();
            }
        }

        private bool mainMenuShowHibernate = true;
        public bool MainMenuShowHibernate
        {
            get
            {
                return mainMenuShowHibernate;
            }

            set
            {
                mainMenuShowHibernate = value;
                OnPropertyChanged();
            }
        }

        private bool mainMenuShowMinimize = true;
        public bool MainMenuShowMinimize
        {
            get
            {
                return mainMenuShowMinimize;
            }

            set
            {
                mainMenuShowMinimize = value;
                OnPropertyChanged();
            }
        }

        private bool swapStartDetailsAction = false;
        public bool SwapStartDetailsAction
        {
            get
            {
                return swapStartDetailsAction;
            }

            set
            {
                swapStartDetailsAction = value;
                OnPropertyChanged();
            }
        }

        private bool swapConfirmCancelButtons = false;
        public bool SwapConfirmCancelButtons
        {
            get
            {
                return swapConfirmCancelButtons;
            }

            set
            {
                swapConfirmCancelButtons = value;
                OnPropertyChanged();
            }
        }

        private float interfaceVolume = 0.5f;
        public float InterfaceVolume
        {
            get
            {
                return interfaceVolume;
            }

            set
            {
                interfaceVolume = value;
                OnPropertyChanged();
            }
        }

        private float musicVolume = 0.3f;
        public float BackgroundVolume
        {
            get
            {
                return musicVolume;
            }

            set
            {
                musicVolume = value;
                OnPropertyChanged();
            }
        }

        private bool muteInBackground = true;
        public bool MuteInBackground
        {
            get
            {
                return muteInBackground;
            }

            set
            {
                muteInBackground = value;
                OnPropertyChanged();
            }
        }

        private bool primaryControllerOnly = true;
        public bool PrimaryControllerOnly
        {
            get
            {
                return primaryControllerOnly;
            }

            set
            {
                primaryControllerOnly = value;
                OnPropertyChanged();
            }
        }

        private bool guideButtonFocus = false;
        public bool GuideButtonFocus
        {
            get
            {
                return guideButtonFocus;
            }

            set
            {
                guideButtonFocus = value;
                OnPropertyChanged();
            }
        }

        private AudioInterfaceApi audioInterfaceApi = AudioInterfaceApi.WASAPI;
        [RequiresRestart]
        public AudioInterfaceApi AudioInterfaceApi
        {
            get
            {
                return audioInterfaceApi;
            }

            set
            {
                audioInterfaceApi = value;
                OnPropertyChanged();
            }
        }

        private ImageLoadScaling imageScalerMode = ImageLoadScaling.BitmapDotNet;
        public ImageLoadScaling ImageScalerMode
        {
            get => imageScalerMode;
            set
            {
                imageScalerMode = value;
                OnPropertyChanged();
            }
        }

        private bool smoothScrolling = true;
        public bool SmoothScrolling
        {
            get => smoothScrolling;
            set
            {
                smoothScrolling = value;
                OnPropertyChanged();
            }
        }
    }
}
