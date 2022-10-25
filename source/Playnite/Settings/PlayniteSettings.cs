using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.Configuration;
using Playnite.Common;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Newtonsoft.Json.Serialization;
using System.Runtime.Serialization;
using Playnite.Metadata;
using Playnite.SDK;
using Microsoft.Win32;
using Playnite.SDK.Models;
using System.Collections.ObjectModel;
using Playnite.SDK.Plugins;

namespace Playnite
{
    public enum DesktopSettingsPage
    {
        General = 0,
        AppearanceGeneral = 1,
        AppearanceAdvanced = 2,
        AppearanceDetailsView = 3,
        AppearanceGridView = 4,
        AppearanceLayout = 5,
        GeneralAdvanced = 6,
        Input = 7,
        Metadata = 9,
        Scripting = 11,
        ClientShutdown = 12,
        Performance = 13,
        ImportExlusionList = 14,
        Development = 19,
        AppearanceTopPanel = 20,
        Sorting = 21,
        Updates = 22,
        AppearanceListView = 23,
        Search = 24,
        Backup = 25
    }

    public enum GameSearchItemAction
    {
        [Description(LOC.GameSearchItemActionPlay)]
        Play,
        [Description(LOC.GameSearchItemActionSwitchTo)]
        SwitchTo,
        [Description(LOC.GameSearchItemActionOpenMenu)]
        OpenMenu,
        [Description(LOC.GameSearchItemActionEdit)]
        Edit,
        [Description(LOC.None)]
        None
    }

    public enum AfterLaunchOptions
    {
        None,
        Minimize,
        Close
    }

    public enum AfterGameCloseOptions
    {
        None,
        Restore
    }

    public enum ApplicationView
    {
        Library,
        Statistics
    }

    public enum ImageLoadScaling
    {
        [Description(LOC.SettingsImageScalingQuality)]
        None,
        [Description(LOC.SettingsImageScalingBalanced)]
        BitmapDotNet,
        [Description(LOC.SettingsImageScalingAlternative)]
        Custom
    }

    public enum TrayIconType
    {
        [Description("TrayIcon")]
        Default,
        [Description("TrayIconWhite")]
        Bright,
        [Description("TrayIconBlack")]
        Dark
    }

    public enum DefaultIconSourceOptions
    {
        [Description("LOCGameProviderTitle")]
        Library,
        [Description("LOCPlatformTitle")]
        Platform,
        [Description("Playnite")]
        General,
        [Description("LOCNone")]
        None
    }

    public enum DefaultCoverSourceOptions
    {
        [Description("LOCPlatformTitle")]
        Platform,
        [Description("Playnite")]
        General,
        [Description("LOCNone")]
        None
    }

    public enum DefaultBackgroundSourceOptions
    {
        [Description("LOCGameProviderTitle")]
        Library,
        [Description("LOCPlatformTitle")]
        Platform,
        [Description("LOCGameCoverTitle")]
        Cover,
        [Description("LOCNone")]
        None
    }

    public enum TextRenderingModeOptions
    {
        [Description("LOCSettingsTextRenderingModeOptionAuto")]
        Auto = 0,
        [Description("LOCSettingsTextRenderingModeOptionAliased")]
        Aliased = 1,
        [Description("LOCSettingsTextRenderingModeOptionGrayscale")]
        Grayscale = 2,
        [Description("LOCSettingsTextRenderingModeOptionClearType")]
        ClearType = 3
    }

    public enum TextFormattingModeOptions
    {
        [Description("LOCSettingsTextFormattingModeOptionIdeal")]
        Ideal = 0,
        [Description("LOCSettingsTextFormattingModeOptionDisplay")]
        Display = 1
    }

    public enum UpdateCheckFrequency
    {
        [Description(LOC.OptionOnEveryStartup)]
        OnEveryStartup = 0,
        [Description(LOC.OptionOnceADay)]
        OnceADay = 1,
        [Description(LOC.OptionOnceAWeek)]
        OnceAWeek = 2
    }

    public enum LibraryUpdateCheckFrequency
    {
        [Description(LOC.OptionOnlyManually)]
        Manually = 0,
        [Description(LOC.OptionOnEveryStartup)]
        OnEveryStartup = 1,
        [Description(LOC.OptionOnceADay)]
        OnceADay = 2,
        [Description(LOC.OptionOnceAWeek)]
        OnceAWeek = 3
    }

    public enum AutoBackupFrequency
    {
        [Description(LOC.OptionOnceADay)]
        OnceADay = 1,
        [Description(LOC.OptionOnceAWeek)]
        OnceAWeek = 2
    }

    public class DateFormattingOptions : ObservableObject
    {
        private string format;
        private bool pastWeekRelativeFormat;

        public string Format { get => format; set => SetValue(ref format, value); }
        public bool PastWeekRelativeFormat { get => pastWeekRelativeFormat; set => SetValue(ref pastWeekRelativeFormat, value); }

        public DateFormattingOptions()
        {
        }

        public DateFormattingOptions(string format, bool pastWeekRelativeFormat)
        {
            Format = format;
            PastWeekRelativeFormat = pastWeekRelativeFormat;
        }
    }

    public class ReleaseDateFormattingOptions : DateFormattingOptions
    {
        private string partialFormat = Constants.DefaultPartialReleaseDateTimeFormat;
        public string PartialFormat { get => partialFormat; set => SetValue(ref partialFormat, value); }

        public ReleaseDateFormattingOptions() : base()
        {
        }

        public ReleaseDateFormattingOptions(string format, bool pastWeekRelativeFormat) : base(format, pastWeekRelativeFormat)
        {
        }
    }

    public class PlayniteSettings : ObservableObject
    {
        private static SDK.ILogger logger = SDK.LogManager.GetLogger();

        public int Version
        {
            get; set;
        } = 7;

        private DetailsVisibilitySettings detailsVisibility = new DetailsVisibilitySettings();
        public DetailsVisibilitySettings DetailsVisibility
        {
            get
            {
                return detailsVisibility;
            }

            set
            {
                detailsVisibility = value;
                OnPropertyChanged();
            }
        }

        private DefaultIconSourceOptions defaultIconSource = DefaultIconSourceOptions.General;
        public DefaultIconSourceOptions DefaultIconSource
        {
            get
            {
                return defaultIconSource;
            }

            set
            {
                defaultIconSource = value;
                OnPropertyChanged();
            }
        }

        private DefaultCoverSourceOptions defaultCoverSource = DefaultCoverSourceOptions.General;
        public DefaultCoverSourceOptions DefaultCoverSource
        {
            get
            {
                return defaultCoverSource;
            }

            set
            {
                defaultCoverSource = value;
                OnPropertyChanged();
            }
        }

        private DefaultBackgroundSourceOptions defaultBackgroundSource = DefaultBackgroundSourceOptions.None;
        public DefaultBackgroundSourceOptions DefaultBackgroundSource
        {
            get
            {
                return defaultBackgroundSource;
            }

            set
            {
                defaultBackgroundSource = value;
                OnPropertyChanged();
            }
        }

        private bool indentGameDetails = true;
        public bool IndentGameDetails
        {
            get
            {
                return indentGameDetails;
            }

            set
            {
                indentGameDetails = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CalculatedGameDetailsIndentation));
            }
        }

        public double CalculatedGameDetailsIndentation
        {
            get
            {
                return IndentGameDetails ? GameDetailsIndentation : Double.NaN;
            }
        }

        private int gameDetailsIndentation = 400;
        public int GameDetailsIndentation
        {
            get
            {
                return gameDetailsIndentation;
            }

            set
            {
                gameDetailsIndentation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CalculatedGameDetailsIndentation));
            }
        }

        private Dock gridViewDetailsPosition = Dock.Right;
        public Dock GridViewDetailsPosition
        {
            get
            {
                return gridViewDetailsPosition;
            }

            set
            {
                gridViewDetailsPosition = value;
                OnPropertyChanged();
            }
        }

        private Dock filterPanelPosition = Dock.Right;
        public Dock FilterPanelPosition
        {
            get
            {
                return filterPanelPosition;
            }

            set
            {
                filterPanelPosition = value;
                OnPropertyChanged();
            }
        }

        private Dock explorerPanelPosition = Dock.Left;
        public Dock ExplorerPanelPosition
        {
            get
            {
                return explorerPanelPosition;
            }

            set
            {
                explorerPanelPosition = value;
                OnPropertyChanged();
            }
        }

        private Dock detailsListPosition = Dock.Left;
        public Dock DetailsListPosition
        {
            get
            {
                return detailsListPosition;
            }

            set
            {
                detailsListPosition = value;
                OnPropertyChanged();
            }
        }

        private bool explorerPanelVisible = false;
        public bool ExplorerPanelVisible
        {
            get
            {
                return explorerPanelVisible;
            }

            set
            {
                explorerPanelVisible = value;
                OnPropertyChanged();
            }
        }

        private double filterPanelWitdh = 240;
        public double FilterPanelWitdh
        {
            get
            {
                return filterPanelWitdh;
            }

            set
            {
                filterPanelWitdh = value;
                OnPropertyChanged();
            }
        }

        private double explorerPanelWitdh = 280;
        public double ExplorerPanelWitdh
        {
            get
            {
                return explorerPanelWitdh;
            }

            set
            {
                explorerPanelWitdh = value;
                OnPropertyChanged();
            }
        }

        private double grdiDetailsWitdh = 350;
        public double GrdiDetailsWitdh
        {
            get
            {
                return grdiDetailsWitdh;
            }

            set
            {
                grdiDetailsWitdh = value;
                OnPropertyChanged();
            }
        }

        private double detailsListWitdh = 350;
        public double DetailsListWitdh
        {
            get
            {
                return detailsListWitdh;
            }

            set
            {
                detailsListWitdh = value;
                OnPropertyChanged();
            }
        }

        private bool showGridItemBackground = true;
        public bool ShowGridItemBackground
        {
            get
            {
                return showGridItemBackground;
            }

            set
            {
                showGridItemBackground = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public double GridItemHeight
        {
            get; private set;
        }

        private double gridItemWidth = ViewSettings.DefaultGridItemWidth;
        public double GridItemWidth
        {
            get
            {
                return gridItemWidth;
            }

            set
            {
                gridItemWidth = Math.Round(value);
                OnPropertyChanged();
                UpdateGridItemHeight();
            }
        }

        [JsonIgnore]
        public AspectRatio CoverAspectRatio => new AspectRatio(GridItemWidthRatio, GridItemHeightRatio);

        private int gridItemWidthRatio = 3;
        public int GridItemWidthRatio
        {
            get
            {
                return gridItemWidthRatio;
            }

            set
            {
                gridItemWidthRatio = value;
                OnPropertyChanged();
                UpdateGridItemHeight();
                OnPropertyChanged(nameof(CoverAspectRatio));
            }
        }

        private int gridItemHeightRatio = 4;
        public int GridItemHeightRatio
        {
            get
            {
                return gridItemHeightRatio;
            }

            set
            {
                gridItemHeightRatio = value;
                OnPropertyChanged();
                UpdateGridItemHeight();
                OnPropertyChanged(nameof(CoverAspectRatio));
            }
        }

        private Stretch coverArtStretch = Stretch.UniformToFill;
        public Stretch CoverArtStretch
        {
            get
            {
                return coverArtStretch;
            }

            set
            {
                coverArtStretch = value;
                OnPropertyChanged();
            }
        }

        private int gridItemSpacing = 8;
        public int GridItemSpacing
        {
            get
            {
                return gridItemSpacing;
            }

            set
            {
                gridItemSpacing = value;
                OnPropertyChanged();
                ItemSpacingMargin = GetItemSpacingMargin();
                OnPropertyChanged(nameof(ItemSpacingMargin));
            }
        }

        private int gridItemMargin = 2;
        public int GridItemMargin
        {
            get
            {
                return gridItemMargin;
            }

            set
            {
                gridItemMargin = value;
                OnPropertyChanged();
            }
        }

        private int fullscreenItemSpacing = 20;
        public int FullscreenItemSpacing
        {
            get
            {
                return fullscreenItemSpacing;
            }

            set
            {
                fullscreenItemSpacing = value;
                OnPropertyChanged();
                FullscreenItemSpacingMargin = GetFullscreenItemSpacingMargin();
                OnPropertyChanged(nameof(FullscreenItemSpacingMargin));
            }
        }

        [JsonIgnore]
        public Thickness ItemSpacingMargin
        {
            get; private set;
        }

        [JsonIgnore]
        public Thickness FullscreenItemSpacingMargin
        {
            get; private set;
        }

        private bool firstTimeWizardComplete;
        public bool FirstTimeWizardComplete
        {
            get
            {
                return firstTimeWizardComplete;
            }

            set
            {
                firstTimeWizardComplete = value;
                OnPropertyChanged();
            }
        }

        private bool disableHwAcceleration = false;
        [RequiresRestart]
        public bool DisableHwAcceleration
        {
            get
            {
                return disableHwAcceleration;
            }

            set
            {
                disableHwAcceleration = value;
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

        private bool showNameEmptyCover = true;
        public bool ShowNameEmptyCover
        {
            get
            {
                return showNameEmptyCover;
            }

            set
            {
                showNameEmptyCover = value;
                OnPropertyChanged();
            }
        }

        private bool showNamesUnderCovers = false;
        public bool ShowNamesUnderCovers
        {
            get
            {
                return showNamesUnderCovers;
            }

            set
            {
                showNamesUnderCovers = value;
                OnPropertyChanged();
            }
        }

        private bool showBackgroundImageOnWindow = true;
        public bool ShowBackgroundImageOnWindow
        {
            get
            {
                return showBackgroundImageOnWindow;
            }

            set
            {
                showBackgroundImageOnWindow = value;
                OnPropertyChanged();
            }
        }

        private bool highQualityBackgroundBlur = true;
        public bool HighQualityBackgroundBlur
        {
            get
            {
                return highQualityBackgroundBlur;
            }

            set
            {
                highQualityBackgroundBlur = value;
                OnPropertyChanged();
            }
        }

        private bool blurWindowBackgroundImage = true;
        public bool BlurWindowBackgroundImage
        {
            get
            {
                return blurWindowBackgroundImage;
            }

            set
            {
                blurWindowBackgroundImage = value;
                OnPropertyChanged();
            }
        }

        private double backgroundImageBlurAmount = 60;
        public double BackgroundImageBlurAmount
        {
            get
            {
                return backgroundImageBlurAmount;
            }

            set
            {
                backgroundImageBlurAmount = value;
                OnPropertyChanged();
            }
        }

        private bool darkenWindowBackgroundImage = true;
        public bool DarkenWindowBackgroundImage
        {
            get
            {
                return darkenWindowBackgroundImage;
            }

            set
            {
                darkenWindowBackgroundImage = value;
                OnPropertyChanged();
            }
        }

        private float backgroundImageDarkAmount = 0.7f;
        public float BackgroundImageDarkAmount
        {
            get
            {
                return backgroundImageDarkAmount;
            }

            set
            {
                backgroundImageDarkAmount = value;
                OnPropertyChanged();
            }
        }

        private bool showBackImageOnGridView = false;
        public bool ShowBackImageOnGridView
        {
            get
            {
                return showBackImageOnGridView;
            }

            set
            {
                showBackImageOnGridView = value;
                OnPropertyChanged();
            }
        }

        private bool downloadMetadataOnImport = true;
        public bool DownloadMetadataOnImport
        {
            get
            {
                return downloadMetadataOnImport;
            }

            set
            {
                downloadMetadataOnImport = value;
                OnPropertyChanged();
            }
        }

        private bool showIconsOnList = true;
        public bool ShowIconsOnList
        {
            get
            {
                return showIconsOnList;
            }

            set
            {
                showIconsOnList = value;
                OnPropertyChanged();
            }
        }

        private bool showGroupCount = true;
        public bool ShowGroupCount
        {
            get
            {
                return showGroupCount;
            }

            set
            {
                showGroupCount = value;
                OnPropertyChanged();
            }
        }

        private bool startInFullscreen = false;
        public bool StartInFullscreen
        {
            get
            {
                return startInFullscreen;
            }

            set
            {
                startInFullscreen = value;
                OnPropertyChanged();
            }
        }

        private string databasePath;
        [RequiresRestart]
        public string DatabasePath
        {
            get
            {
                return databasePath;
            }

            set
            {
                databasePath = value;
                OnPropertyChanged();
            }
        }

        private FilterSettings filterSettings = new FilterSettings();
        public FilterSettings FilterSettings
        {
            get
            {
                return filterSettings;
            }

            set
            {
                filterSettings = value;
            }
        }

        private ViewSettings desktopViewSettings = new ViewSettings();
        public ViewSettings ViewSettings
        {
            get
            {
                return desktopViewSettings;
            }

            set
            {
                desktopViewSettings = value;
            }
        }

        private bool gridViewSideBarVisible = false;
        public bool GridViewSideBarVisible
        {
            get
            {
                return gridViewSideBarVisible;
            }

            set
            {
                gridViewSideBarVisible = value;
                OnPropertyChanged();
            }
        }

        private bool filterPanelVisible = false;
        public bool FilterPanelVisible
        {
            get
            {
                return filterPanelVisible;
            }

            set
            {
                filterPanelVisible = value;
                OnPropertyChanged();
            }
        }

        private bool notificationPanelVisible = false;
        [JsonIgnore]
        public bool NotificationPanelVisible
        {
            get
            {
                return notificationPanelVisible;
            }

            set
            {
                notificationPanelVisible = value;
                OnPropertyChanged();
            }
        }

        private bool showSidebar = true;
        public bool ShowSidebar
        {
            get
            {
                return showSidebar;
            }

            set
            {
                showSidebar = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowMainMenuOnTopPanel));
            }
        }

        private Dock sidebarPosition = Dock.Left;
        public Dock SidebarPosition
        {
            get
            {
                return sidebarPosition;
            }

            set
            {
                sidebarPosition = value;
                OnPropertyChanged();
            }
        }

        private bool mainMenuButtonSidebarMove = true;
        public bool MainMenuButtonSidebarMove
        {
            get
            {
                return mainMenuButtonSidebarMove;
            }

            set
            {
                mainMenuButtonSidebarMove = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowMainMenuOnTopPanel));
            }
        }

        [JsonIgnore]
        public bool ShowMainMenuOnTopPanel => !ShowSidebar || (ShowSidebar && !MainMenuButtonSidebarMove);

        private bool minimizeToTray = false;
        public bool MinimizeToTray
        {
            get
            {
                return minimizeToTray;
            }

            set
            {
                minimizeToTray = value;
                OnPropertyChanged();
            }
        }

        private bool closeToTray = true;
        public bool CloseToTray
        {
            get
            {
                return closeToTray;
            }

            set
            {
                closeToTray = value;
                OnPropertyChanged();
            }
        }

        private bool enableTray = true;
        [RequiresRestart]
        public bool EnableTray
        {
            get
            {
                return enableTray;
            }

            set
            {
                enableTray = value;
                OnPropertyChanged();
            }
        }

        private string language = "english";
        [RequiresRestart]
        public string Language
        {
            get
            {
                return language;
            }

            set
            {
                language = value;
                OnPropertyChanged();
            }
        }

        private AfterLaunchOptions afterLaunch = AfterLaunchOptions.Minimize;
        public AfterLaunchOptions AfterLaunch
        {
            get
            {
                return afterLaunch;
            }

            set
            {
                afterLaunch = value;
                OnPropertyChanged();
            }
        }

        private AfterGameCloseOptions afterGameClose = AfterGameCloseOptions.Restore;
        public AfterGameCloseOptions AfterGameClose
        {
            get
            {
                return afterGameClose;
            }

            set
            {
                afterGameClose = value;
                OnPropertyChanged();
            }
        }

        private string theme = ThemeManager.DefaultDesktopThemeId;
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

        private TrayIconType trayIcon = TrayIconType.Default;
        [RequiresRestart]
        public TrayIconType TrayIcon
        {
            get
            {
                return trayIcon;
            }

            set
            {
                trayIcon = value;
                OnPropertyChanged();
            }
        }

        public string InstallInstanceId
        {
            get; set;
        }

        private List<string> disabledPlugins = new List<string>();
        [RequiresRestart]
        public List<string> DisabledPlugins
        {
            get
            {
                return disabledPlugins;
            }

            set
            {
                disabledPlugins = value;
                OnPropertyChanged();
            }
        }

        private bool startMinimized = false;
        public bool StartMinimized
        {
            get
            {
                return startMinimized;
            }

            set
            {
                startMinimized = value;
                OnPropertyChanged();
            }
        }

        private bool startOnBoot = false;
        public bool StartOnBoot
        {
            get
            {
                return startOnBoot;
            }

            set
            {
                startOnBoot = value;
                OnPropertyChanged();
            }
        }

        private bool startOnBootClosedToTray = false;
        public bool StartOnBootClosedToTray
        {
            get
            {
                return startOnBootClosedToTray;
            }

            set
            {
                startOnBootClosedToTray = value;
                OnPropertyChanged();
            }
        }

        private bool enableControolerInDesktop = false;
        [RequiresRestart]
        public bool EnableControllerInDesktop
        {
            get
            {
                return enableControolerInDesktop;
            }

            set
            {
                enableControolerInDesktop = value;
                OnPropertyChanged();
            }
        }

        private bool guideButtonOpensFullscreen = false;
        public bool GuideButtonOpensFullscreen
        {
            get
            {
                return guideButtonOpensFullscreen;
            }

            set
            {
                guideButtonOpensFullscreen = value;
                OnPropertyChanged();
            }
        }

        private bool showPanelSeparators = true;
        public bool ShowPanelSeparators
        {
            get
            {
                return showPanelSeparators;
            }

            set
            {
                showPanelSeparators = value;
                OnPropertyChanged();
            }
        }

        private double gameDetailsCoverHeight = 170;
        public double GameDetailsCoverHeight
        {
            get
            {
                return gameDetailsCoverHeight;
            }

            set
            {
                gameDetailsCoverHeight = value;
                OnPropertyChanged();
            }
        }

        private string fontFamilyName = "Trebuchet MS";
        [RequiresRestart]
        public string FontFamilyName
        {
            get
            {
                return fontFamilyName;
            }

            set
            {
                fontFamilyName = value;
                OnPropertyChanged();
            }
        }

        private string monospaceFontFamilyName = "Consolas";
        [RequiresRestart]
        public string MonospaceFontFamilyName
        {
            get
            {
                return monospaceFontFamilyName;
            }

            set
            {
                monospaceFontFamilyName = value;
                OnPropertyChanged();
            }
        }

        private double fontSize = 14;
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

        private double fontSizeSmall = 12;
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

        private double fontSizeLarge = 15;
        [RequiresRestart]
        public double FontSizeLarge
        {
            get
            {
                return fontSizeLarge;
            }

            set
            {
                fontSizeLarge = value;
                OnPropertyChanged();
            }
        }

        private double fontSizeLarger = 20;
        [RequiresRestart]
        public double FontSizeLarger
        {
            get
            {
                return fontSizeLarger;
            }

            set
            {
                fontSizeLarger = value;
                OnPropertyChanged();
            }
        }

        private double fontSizeLargest = 29;
        [RequiresRestart]
        public double FontSizeLargest
        {
            get
            {
                return fontSizeLargest;
            }

            set
            {
                fontSizeLargest = value;
                OnPropertyChanged();
            }
        }

        private double detailsViewListIconSize = 26;
        public double DetailsViewListIconSize
        {
            get
            {
                return detailsViewListIconSize;
            }

            set
            {
                detailsViewListIconSize = value;
                OnPropertyChanged();
            }
        }

        private TextFormattingModeOptions textFormattingMode = TextFormattingModeOptions.Ideal;
        [RequiresRestart]
        public TextFormattingModeOptions TextFormattingMode
        {
            get
            {
                return textFormattingMode;
            }

            set
            {
                textFormattingMode = value;
                OnPropertyChanged();
            }
        }

        private TextRenderingModeOptions textRenderingMode = TextRenderingModeOptions.Auto;
        [RequiresRestart]
        public TextRenderingModeOptions TextRenderingMode
        {
            get
            {
                return textRenderingMode;
            }

            set
            {
                textRenderingMode = value;
                OnPropertyChanged();
            }
        }

        private MetadataDownloaderSettings metadataSettings;
        public MetadataDownloaderSettings MetadataSettings
        {
            get
            {
                return metadataSettings;
            }

            set
            {
                metadataSettings = value;
                OnPropertyChanged();
            }
        }

        private string preScript;
        public string PreScript
        {
            get => preScript;
            set
            {
                preScript = value;
                OnPropertyChanged();
            }
        }

        private string postScript;
        public string PostScript
        {
            get => postScript;
            set
            {
                postScript = value;
                OnPropertyChanged();
            }
        }

        private string gameStartedScript;
        public string GameStartedScript
        {
            get => gameStartedScript;
            set
            {
                gameStartedScript = value;
                OnPropertyChanged();
            }
        }

        private string appStartupScript;
        public string AppStartupScript
        {
            get => appStartupScript;
            set
            {
                appStartupScript = value;
                OnPropertyChanged();
            }
        }

        private string appShutdownScript;
        public string AppShutdownScript
        {
            get => appShutdownScript;
            set
            {
                appShutdownScript = value;
                OnPropertyChanged();
            }
        }

        private bool downloadBackgroundsImmediately = true;
        public bool DownloadBackgroundsImmediately
        {
            get => downloadBackgroundsImmediately;
            set
            {
                downloadBackgroundsImmediately = value;
                OnPropertyChanged();
            }
        }

        private bool showImagePerformanceWarning = true;
        public bool ShowImagePerformanceWarning
        {
            get => showImagePerformanceWarning;
            set
            {
                showImagePerformanceWarning = value;
                OnPropertyChanged();
            }
        }

        private bool backgroundImageAnimation = true;
        public bool BackgroundImageAnimation
        {
            get => backgroundImageAnimation;
            set
            {
                backgroundImageAnimation = value;
                OnPropertyChanged();
            }
        }

        private AutoClientShutdownSettings clientAutoShutdown = new AutoClientShutdownSettings();
        public AutoClientShutdownSettings ClientAutoShutdown
        {
            get => clientAutoShutdown;
            set
            {
                clientAutoShutdown = value;
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

        private bool usedFieldsOnlyOnFilterLists = true;
        public bool UsedFieldsOnlyOnFilterLists
        {
            get => usedFieldsOnlyOnFilterLists;
            set
            {
                usedFieldsOnlyOnFilterLists = value;
                OnPropertyChanged();
            }
        }

        private bool discordPresenceEnabled = false;
        public bool DiscordPresenceEnabled
        {
            get => discordPresenceEnabled;
            set
            {
                discordPresenceEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool showHiddenInQuickLaunch = true;
        public bool ShowHiddenInQuickLaunch
        {
            get => showHiddenInQuickLaunch;
            set
            {
                showHiddenInQuickLaunch = value;
                OnPropertyChanged();
            }
        }

        private int quickLaunchItems = 10;
        public int QuickLaunchItems
        {
            get => quickLaunchItems;
            set
            {
                quickLaunchItems = value;
                OnPropertyChanged();
            }
        }

        private string directoryOpenCommand;
        public string DirectoryOpenCommand
        {
            get => directoryOpenCommand;
            set
            {
                directoryOpenCommand = value;
                OnPropertyChanged();
            }
        }

        private AgeRatingOrg ageRatingOrgPriority = AgeRatingOrg.PEGI;
        public AgeRatingOrg AgeRatingOrgPriority
        {
            get => ageRatingOrgPriority;
            set
            {
                ageRatingOrgPriority = value;
                OnPropertyChanged();
            }
        }

        private bool traceLogEnabled = false;
        public bool TraceLogEnabled
        {
            get => traceLogEnabled;
            set
            {
                traceLogEnabled = value;
                OnPropertyChanged();
            }
        }

        private double gridViewScrollSensitivity = 1.5;
        public double GridViewScrollSensitivity
        {
            get => gridViewScrollSensitivity;
            set
            {
                gridViewScrollSensitivity = value;
                OnPropertyChanged();
            }
        }

        private long gridViewScrollSpeed = 250 * TimeSpan.TicksPerMillisecond;
        public long GridViewScrollSpeed
        {
            get => gridViewScrollSpeed;
            set
            {
                gridViewScrollSpeed = value;
                OnPropertyChanged();
            }
        }

        private bool gridViewSmoothScrollEnabled = false;
        public bool GridViewSmoothScrollEnabled
        {
            get => gridViewSmoothScrollEnabled;
            set
            {
                gridViewSmoothScrollEnabled = value;
                OnPropertyChanged();
            }
        }

        private double detailsViewScrollSensitivity = 1.5;
        public double DetailsViewScrollSensitivity
        {
            get => detailsViewScrollSensitivity;
            set
            {
                detailsViewScrollSensitivity = value;
                OnPropertyChanged();
            }
        }

        private long detailsViewScrollSpeed = 250 * TimeSpan.TicksPerMillisecond;
        public long DetailsViewScrollSpeed
        {
            get => detailsViewScrollSpeed;
            set
            {
                detailsViewScrollSpeed = value;
                OnPropertyChanged();
            }
        }

        private bool detailsViewSmoothScrollEnabled = false;
        public bool DetailsViewSmoothScrollEnabled
        {
            get => detailsViewSmoothScrollEnabled;
            set
            {
                detailsViewSmoothScrollEnabled = value;
                OnPropertyChanged();
            }
        }

        private double listViewScrollSensitivity = 1.5;
        public double ListViewScrollSensitivity
        {
            get => listViewScrollSensitivity;
            set
            {
                listViewScrollSensitivity = value;
                OnPropertyChanged();
            }
        }

        private long listViewScrollSpeed = 250 * TimeSpan.TicksPerMillisecond;
        public long ListViewScrollSpeed
        {
            get => listViewScrollSpeed;
            set
            {
                listViewScrollSpeed = value;
                OnPropertyChanged();
            }
        }

        private bool listViewSmoothScrollEnabled = false;
        public bool ListViewSmoothScrollEnabled
        {
            get => listViewSmoothScrollEnabled;
            set
            {
                listViewSmoothScrollEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool showTopPanelGeneralViewItem = false;
        public bool ShowTopPanelGeneralViewItem
        {
            get => showTopPanelGeneralViewItem;
            set
            {
                showTopPanelGeneralViewItem = value;
                OnPropertyChanged();
            }
        }

        private bool showTopPanelGroupingItem = true;
        public bool ShowTopPanelGroupingItem
        {
            get => showTopPanelGroupingItem;
            set
            {
                showTopPanelGroupingItem = value;
                OnPropertyChanged();
            }
        }

        private bool showTopPanelSortingItem = true;
        public bool ShowTopPanelSortingItem
        {
            get => showTopPanelSortingItem;
            set
            {
                showTopPanelSortingItem = value;
                OnPropertyChanged();
            }
        }

        private bool showTopPanelFilterPresetsItem = true;
        public bool ShowTopPanelFilterPresetsItem
        {
            get => showTopPanelFilterPresetsItem;
            set
            {
                showTopPanelFilterPresetsItem = value;
                OnPropertyChanged();
            }
        }

        private bool showTopPanelDetailsViewSwitch = true;
        public bool ShowTopPanelDetailsViewSwitch
        {
            get => showTopPanelDetailsViewSwitch;
            set
            {
                showTopPanelDetailsViewSwitch = value;
                OnPropertyChanged();
            }
        }

        private bool showTopPanelGridViewSwitch = true;
        public bool ShowTopPanelGridViewSwitch
        {
            get => showTopPanelGridViewSwitch;
            set
            {
                showTopPanelGridViewSwitch = value;
                OnPropertyChanged();
            }
        }

        private bool showTopPanelListViewSwitch = true;
        public bool ShowTopPanelListViewSwitch
        {
            get => showTopPanelListViewSwitch;
            set
            {
                showTopPanelListViewSwitch = value;
                OnPropertyChanged();
            }
        }

        private bool showTopPanelExplorerSwitch = true;
        public bool ShowTopPanelExplorerSwitch
        {
            get => showTopPanelExplorerSwitch;
            set
            {
                showTopPanelExplorerSwitch = value;
                OnPropertyChanged();
            }
        }

        private bool showTopPanelSelectRandomGameButton = false;
        public bool ShowTopPanelSelectRandomGameButton
        {
            get => showTopPanelSelectRandomGameButton;
            set
            {
                showTopPanelSelectRandomGameButton = value;
                OnPropertyChanged();
            }
        }

        private bool showTopPanelViewSelectRandomGameButton = true;
        public bool ShowTopPanelViewSelectRandomGameButton
        {
            get => showTopPanelViewSelectRandomGameButton;
            set
            {
                showTopPanelViewSelectRandomGameButton = value;
                OnPropertyChanged();
            }
        }

        private bool showTopPanelSearchButton = true;
        public bool ShowTopPanelSearchButton
        {
            get => showTopPanelSearchButton;
            set
            {
                showTopPanelSearchButton = value;
                OnPropertyChanged();
            }
        }

        private bool showTopPanelSearchBox = true;
        public bool ShowTopPanelSearchBox
        {
            get => showTopPanelSearchBox;
            set
            {
                showTopPanelSearchBox = value;
                OnPropertyChanged();
            }
        }

        private double topPanelSectionSeparatorWidth = 15;
        public double TopPanelSectionSeparatorWidth
        {
            get
            {
                return topPanelSectionSeparatorWidth;
            }

            set
            {
                topPanelSectionSeparatorWidth = value;
                OnPropertyChanged();
            }
        }

        private Dock pluginTopPanelAlignment = Dock.Right;
        public Dock PluginTopPanelAlignment
        {
            get => pluginTopPanelAlignment;
            set
            {
                pluginTopPanelAlignment = value;
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

        private PlaytimeImportMode playtimeImportMode = PlaytimeImportMode.NewImportsOnly;
        public PlaytimeImportMode PlaytimeImportMode
        {
            get => playtimeImportMode;
            set
            {
                playtimeImportMode = value;
                OnPropertyChanged();
            }
        }

        private bool useCompositionWebViewRenderer = false;
        public bool UseCompositionWebViewRenderer
        {
            get => useCompositionWebViewRenderer;
            set
            {
                useCompositionWebViewRenderer = value;
                OnPropertyChanged();
            }
        }

        private bool addonsPerfNoticeShown = false;
        public bool AddonsPerfNoticeShown
        {
            get => addonsPerfNoticeShown;
            set
            {
                addonsPerfNoticeShown = value;
                OnPropertyChanged();
            }
        }

        private bool gameSortingNameAutofill = true;
        public bool GameSortingNameAutofill
        {
            get => gameSortingNameAutofill;
            set
            {
                gameSortingNameAutofill = value;
                OnPropertyChanged();
            }
        }

        private List<string> gameSortingNameRemovedArticles = new List<string> { "The", "A", "An" };
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<string> GameSortingNameRemovedArticles
        {
            get => gameSortingNameRemovedArticles;
            set
            {
                gameSortingNameRemovedArticles = value;
                OnPropertyChanged();
            }
        }

        private bool showNahimicServiceWarning = true;
        public bool ShowNahimicServiceWarning
        {
            get => showNahimicServiceWarning;
            set
            {
                showNahimicServiceWarning = value;
                OnPropertyChanged();
            }
        }

        private bool showElevatedRightsWarning = true;
        public bool ShowElevatedRightsWarning
        {
            get => showElevatedRightsWarning;
            set
            {
                showElevatedRightsWarning = value;
                OnPropertyChanged();
            }
        }

        private DateFormattingOptions dateTimeFormatAdded = new DateFormattingOptions(Constants.DefaultDateTimeFormat, false);
        [RequiresRestart]
        public DateFormattingOptions DateTimeFormatAdded
        {
            get => dateTimeFormatAdded;
            set
            {
                dateTimeFormatAdded = value;
                OnPropertyChanged();
            }
        }

        private DateFormattingOptions dateTimeFormatModified = new DateFormattingOptions(Constants.DefaultDateTimeFormat, false);
        [RequiresRestart]
        public DateFormattingOptions DateTimeFormatModified
        {
            get => dateTimeFormatModified;
            set
            {
                dateTimeFormatModified = value;
                OnPropertyChanged();
            }
        }

        private DateFormattingOptions dateTimeFormatRecentActivity = new DateFormattingOptions(Constants.DefaultDateTimeFormat, true);
        [RequiresRestart]
        public DateFormattingOptions DateTimeFormatRecentActivity
        {
            get => dateTimeFormatRecentActivity;
            set
            {
                dateTimeFormatRecentActivity = value;
                OnPropertyChanged();
            }
        }

        private ReleaseDateFormattingOptions dateTimeFormatReleaseDate = new ReleaseDateFormattingOptions(Constants.DefaultDateTimeFormat, false);
        [RequiresRestart]
        public ReleaseDateFormattingOptions DateTimeFormatReleaseDate
        {
            get => dateTimeFormatReleaseDate;
            set
            {
                dateTimeFormatReleaseDate = value;
                OnPropertyChanged();
            }
        }

        private DateFormattingOptions dateTimeFormatLastPlayed = new DateFormattingOptions(Constants.DefaultDateTimeFormat, true);
        [RequiresRestart]
        public DateFormattingOptions DateTimeFormatLastPlayed
        {
            get => dateTimeFormatLastPlayed;
            set
            {
                dateTimeFormatLastPlayed = value;
                OnPropertyChanged();
            }
        }

        private bool installSizeScanUseSizeOnDisk = true;
        public bool InstallSizeScanUseSizeOnDisk
        {
            get => installSizeScanUseSizeOnDisk;
            set
            {
                installSizeScanUseSizeOnDisk = value;
                OnPropertyChanged();
            }
        }

        private bool scanLibInstallSizeOnLibUpdate = true;
        public bool ScanLibInstallSizeOnLibUpdate
        {
            get => scanLibInstallSizeOnLibUpdate;
            set
            {
                scanLibInstallSizeOnLibUpdate = value;
                OnPropertyChanged();
            }
        }

        private UpdateCheckFrequency checkForProgramUpdates = UpdateCheckFrequency.OnEveryStartup;
        public UpdateCheckFrequency CheckForProgramUpdates
        {
            get => checkForProgramUpdates;
            set
            {
                checkForProgramUpdates = value;
                OnPropertyChanged();
            }
        }

        private UpdateCheckFrequency checkForAddonUpdates = UpdateCheckFrequency.OnEveryStartup;
        public UpdateCheckFrequency CheckForAddonUpdates
        {
            get => checkForAddonUpdates;
            set
            {
                checkForAddonUpdates = value;
                OnPropertyChanged();
            }
        }

        private LibraryUpdateCheckFrequency checkForLibraryUpdates = LibraryUpdateCheckFrequency.OnEveryStartup;
        public LibraryUpdateCheckFrequency CheckForLibraryUpdates
        {
            get => checkForLibraryUpdates;
            set
            {
                checkForLibraryUpdates = value;
                OnPropertyChanged();
            }
        }

        private LibraryUpdateCheckFrequency checkForEmulatedLibraryUpdates = LibraryUpdateCheckFrequency.OnEveryStartup;
        public LibraryUpdateCheckFrequency CheckForEmulatedLibraryUpdates
        {
            get => checkForEmulatedLibraryUpdates;
            set
            {
                checkForEmulatedLibraryUpdates = value;
                OnPropertyChanged();
            }
        }

        public DateTime LastProgramUpdateCheck { get; set; }
        public DateTime LastAddonUpdateCheck { get; set; }
        public DateTime LastLibraryUpdateCheck { get; set; }
        public DateTime LastEmuLibraryUpdateCheck { get; set; }
        public DateTime LastAutoBackup { get; set; }

        private GameSearchItemAction primaryGameSearchItemAction = GameSearchItemAction.SwitchTo;
        public GameSearchItemAction PrimaryGameSearchItemAction { get => primaryGameSearchItemAction; set => SetValue(ref primaryGameSearchItemAction, value); }

        private GameSearchItemAction secondaryGameSearchItemAction = GameSearchItemAction.Play;
        public GameSearchItemAction SecondaryGameSearchItemAction { get => secondaryGameSearchItemAction; set => SetValue(ref secondaryGameSearchItemAction, value); }

        private bool globalSearchOpenWithLegacySearch = true;
        public bool GlobalSearchOpenWithLegacySearch { get => globalSearchOpenWithLegacySearch; set => SetValue(ref globalSearchOpenWithLegacySearch, value); }

        private bool saveGlobalSearchFilterSettings = true;
        public bool SaveGlobalSearchFilterSettings { get => saveGlobalSearchFilterSettings; set => SetValue(ref saveGlobalSearchFilterSettings, value); }

        private Dictionary<string, string> customSearchKeywrods = new Dictionary<string, string>();
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public Dictionary<string, string> CustomSearchKeywrods { get => customSearchKeywrods; set => SetValue(ref customSearchKeywrods, value); }

        private GameSearchFilterSettings gameSearchFilterSettings = new GameSearchFilterSettings();
        public GameSearchFilterSettings GameSearchFilterSettings { get => gameSearchFilterSettings; set => SetValue(ref gameSearchFilterSettings, value); }

        private HotKey systemSearchHotkey;
        public HotKey SystemSearchHotkey { get => systemSearchHotkey; set => SetValue(ref systemSearchHotkey, value); }

        private bool includeCommandsInDefaultSearch = true;
        public bool IncludeCommandsInDefaultSearch { get => includeCommandsInDefaultSearch; set => SetValue(ref includeCommandsInDefaultSearch, value); }

        private bool autoBackupEnabled = false;
        public bool AutoBackupEnabled { get => autoBackupEnabled; set => SetValue(ref autoBackupEnabled, value); }

        private AutoBackupFrequency autoBackupFrequency = AutoBackupFrequency.OnceAWeek;
        public AutoBackupFrequency AutoBackupFrequency { get => autoBackupFrequency; set => SetValue(ref autoBackupFrequency, value); }

        private string autoBackupDir;
        public string AutoBackupDir { get => autoBackupDir; set => SetValue(ref autoBackupDir, value); }

        private int rotatingBackups;
        public int RotatingBackups { get => rotatingBackups; set => SetValue(ref rotatingBackups, value); }

        private bool autoBackupIncludeLibFiles = true;
        public bool AutoBackupIncludeLibFiles { get => autoBackupIncludeLibFiles; set => SetValue(ref autoBackupIncludeLibFiles, value); }

        private bool autoBackupIncludeExtensionsData = true;
        public bool AutoBackupIncludeExtensionsData { get => autoBackupIncludeExtensionsData; set => SetValue(ref autoBackupIncludeExtensionsData, value); }

        private bool autoBackupIncludeExtensions = false;
        public bool AutoBackupIncludeExtensions { get => autoBackupIncludeExtensions; set => SetValue(ref autoBackupIncludeExtensions, value); }

        private bool autoBackupIncludeThemes = false;
        public bool AutoBackupIncludeThemes { get => autoBackupIncludeThemes; set => SetValue(ref autoBackupIncludeThemes, value); }

        private bool updateNotificationOnPatchesOnly = false;
        public bool UpdateNotificationOnPatchesOnly { get => updateNotificationOnPatchesOnly; set => SetValue(ref updateNotificationOnPatchesOnly, value); }

        private string webImageSarchIconTerm = "{Name} icon";
        public string WebImageSarchIconTerm { get => webImageSarchIconTerm; set => SetValue(ref webImageSarchIconTerm, value); }

        private string webImageSarchCoverTerm = "{Name} cover";
        public string WebImageSarchCoverTerm { get => webImageSarchCoverTerm; set => SetValue(ref webImageSarchCoverTerm, value); }

        private string webImageSarchBackgroundTerm = "{Name} wallpaper";
        public string WebImageSarchBackgroundTerm { get => webImageSarchBackgroundTerm; set => SetValue(ref webImageSarchBackgroundTerm, value); }

        [JsonIgnore]
        public static bool IsPortable
        {
            get
            {
                return PlaynitePaths.IsPortable;
            }
        }

        [JsonIgnore]
        public WindowPositions WindowPositions
        {
            get; private set;
        } = new WindowPositions();

        [JsonIgnore]
        public FullscreenSettings Fullscreen
        {
            get; private set;
        } = new FullscreenSettings();

        private List<SelectableItem<string>> develExtenions = new List<SelectableItem<string>>();
        public List<SelectableItem<string>> DevelExtenions
        {
            get
            {
                return develExtenions;
            }

            set
            {
                develExtenions = value;
                OnPropertyChanged();
            }
        }

        private SearchWindowVisibilitySettings searchWindowVisibility = new SearchWindowVisibilitySettings();
        public SearchWindowVisibilitySettings SearchWindowVisibility
        {
            get
            {
                return searchWindowVisibility;
            }

            set
            {
                searchWindowVisibility = value;
                OnPropertyChanged();
            }
        }

        public PlayniteSettings()
        {
            var gpus = Computer.GetGpuVendors();
            if (gpus.Contains(HwCompany.Intel) || gpus.Contains(HwCompany.VMware))
            {
                BackgroundImageAnimation = false;
            }

            InstallInstanceId = Guid.NewGuid().ToString();
            ItemSpacingMargin = GetItemSpacingMargin();
            FullscreenItemSpacingMargin = GetFullscreenItemSpacingMargin();
            UpdateGridItemHeight();
        }

        private static T LoadSettingFile<T>(string path) where T : class
        {
            try
            {
                if (File.Exists(path))
                {
                    return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, $"Failed to load {path} setting file.");
            }

            return null;
        }

        private static void SaveSettingFile(object settings, string path)
        {
            FileSystem.WriteStringToFile(path, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        public static PlayniteSettings GetDefaultSettings()
        {
            var settings = new PlayniteSettings();
            settings.ViewSettings.ListViewColumsOrder = new List<GameField>
                {
                    GameField.Icon,
                    GameField.Name,
                    GameField.ReleaseDate,
                    GameField.Genres,
                    GameField.LastActivity,
                    GameField.Playtime,
                    GameField.PluginId
                };

            var columns = new ListViewColumnsProperties();
            columns.Icon.Visible = true;
            columns.Name.Visible = true;
            columns.ReleaseDate.Visible = true;
            columns.Genres.Visible = true;
            columns.LastActivity.Visible = true;
            columns.Playtime.Visible = true;
            columns.PluginId.Visible = true;
            settings.ViewSettings.ListViewColumns = columns;
            settings.MetadataSettings = MetadataDownloaderSettings.GetDefaultSettings();
            return settings;
        }

        public static PlayniteSettings LoadSettings()
        {
            var settings = LoadSettingFile<PlayniteSettings>(PlaynitePaths.ConfigFilePath);
            if (settings == null)
            {
                logger.Warn("No existing settings found.");
                settings = LoadSettingFile<PlayniteSettings>(PlaynitePaths.BackupConfigFilePath);
                if (settings == null)
                {
                    logger.Warn("No settings backup found, creating default ones.");
                    settings = new PlayniteSettings();
                }
            }

            if (settings.ViewSettings.ListViewColumsOrder == null)
            {
                settings.ViewSettings.ListViewColumsOrder = new List<GameField>
                {
                    GameField.Icon,
                    GameField.Name,
                    GameField.ReleaseDate,
                    GameField.Genres,
                    GameField.LastActivity,
                    GameField.Playtime,
                    GameField.PluginId
                };
            }

            if (settings.ViewSettings.ListViewColumns == null)
            {
                var columns = new ListViewColumnsProperties();
                columns.Icon.Visible = true;
                columns.Name.Visible = true;
                columns.ReleaseDate.Visible = true;
                columns.Genres.Visible = true;
                columns.LastActivity.Visible = true;
                columns.Playtime.Visible = true;
                columns.PluginId.Visible = true;
                settings.ViewSettings.ListViewColumns = columns;
            }

            if (settings.MetadataSettings == null)
            {
                settings.MetadataSettings = MetadataDownloaderSettings.GetDefaultSettings();
            }

            if (settings.Version == 1)
            {
                settings.BackgroundImageBlurAmount = 17;
                settings.Version = 2;
            }

            if (settings.Version == 2)
            {
                settings.BackgroundImageBlurAmount = 60;
                settings.Version = 3;
            }

            if (settings.Version == 3)
            {
                settings.MetadataSettings.Feature = new MetadataFieldSettings(
                    true, new List<Guid> { Guid.Empty, BuiltinExtensions.GetIdFromExtension(BuiltinExtension.IgdbMetadata) });
                settings.Version = 4;
            }

            if (settings.Version == 4)
            {
                settings.MetadataSettings.AgeRating = new MetadataFieldSettings(
                    true, new List<Guid> { Guid.Empty, BuiltinExtensions.GetIdFromExtension(BuiltinExtension.IgdbMetadata) });
                settings.MetadataSettings.Series = new MetadataFieldSettings(
                    true, new List<Guid> { Guid.Empty, BuiltinExtensions.GetIdFromExtension(BuiltinExtension.IgdbMetadata) });
                settings.MetadataSettings.Platform = new MetadataFieldSettings(
                    true, new List<Guid> { Guid.Empty });
                settings.MetadataSettings.Region = new MetadataFieldSettings(
                    true, new List<Guid> { Guid.Empty });
                settings.Version = 5;
            }

            if (settings.Version == 5)
            {
                if (settings.DisabledPlugins.HasItems())
                {
                    // P9 saves disabled list based on add-on IDs, not directory names.
                    var idsMigration = new Dictionary<string, string>
                    {
                        { "AmazonGamesLibrary", "AmazonLibrary_Builtin" },
                        { "BattleNetLibrary", "BattlenetLibrary_Builtin" },
                        { "BethesdaLibrary", "BethesdaLibrary_Builtin" },
                        { "EpicLibrary", "EpicGamesLibrary_Builtin" },
                        { "GogLibrary", "GogLibrary_Builtin" },
                        { "HumbleLibrary", "HumbleLibrary_Builtin" },
                        { "IGDBMetadata", "IGDBMetadata_Builtin" },
                        { "ItchioLibrary", "ItchioLibrary_Builtin" },
                        { "LibraryExporter", "LibraryExporterPS_Builtin" },
                        { "OriginLibrary", "OriginLibrary_Builtin" },
                        { "PSNLibrary", "PlayStationLibrary_Builtin" },
                        { "SteamLibrary", "SteamLibrary_Builtin" },
                        { "TwitchLibrary", "TwitchLibrary_Builtin" },
                        { "UplayLibrary", "UplayLibrary_Builtin" },
                        { "XboxLibrary", "XboxLibrary_Builtin" }
                    };

                    for (int i = 0; i < settings.DisabledPlugins.Count; i++)
                    {
                        if (idsMigration.TryGetValue(settings.DisabledPlugins[i], out var newValue))
                        {
                            settings.DisabledPlugins[i] = newValue;
                        }
                    }
                }

                settings.ViewSettings.ListViewColumns.AgeRating.Field = GameField.AgeRatings;
                settings.ViewSettings.ListViewColumns.Platform.Field = GameField.Platforms;
                settings.ViewSettings.ListViewColumns.Series.Field = GameField.Series;
                settings.ViewSettings.ListViewColumns.Region.Field = GameField.Regions;
                settings.Version = 6;
            }

            if (settings.Version == 6)
            {
                var oldSettings = LoadSettingFile<Dictionary<string, object>>(PlaynitePaths.ConfigFilePath);
                if (oldSettings != null)
                {
                    if (oldSettings.TryGetValue("UpdateLibStartup", out var oldUpdateLibStartup) && (bool)oldUpdateLibStartup == false)
                    {
                        settings.CheckForLibraryUpdates = LibraryUpdateCheckFrequency.Manually;
                    }

                    if (oldSettings.TryGetValue("UpdateEmulatedLibStartup", out var oldUpdateEmulatedLibStartup) && (bool)oldUpdateEmulatedLibStartup == false)
                    {
                        settings.CheckForEmulatedLibraryUpdates = LibraryUpdateCheckFrequency.Manually;
                    }

                    if (oldSettings.TryGetValue("ForcePlayTimeSync", out var oldForcePlayTimeSync) && (bool)oldForcePlayTimeSync == true)
                    {
                        settings.PlaytimeImportMode = PlaytimeImportMode.Always;
                    }
                }

                settings.Version = 7;
            }

            settings.WindowPositions = LoadExternalConfig<WindowPositions>(PlaynitePaths.WindowPositionsPath, PlaynitePaths.BackupWindowPositionsPath);
            settings.Fullscreen = LoadExternalConfig<FullscreenSettings>(PlaynitePaths.FullscreenConfigFilePath, PlaynitePaths.BackupFullscreenConfigFilePath);
            settings.BackupSettings();
            return settings;
        }

        private static T LoadExternalConfig<T>(string origPath, string backupPath, bool generateDefault = true) where T : class, new()
        {
            var name = Path.GetFileName(origPath);
            var config = LoadSettingFile<T>(origPath);
            if (config == null)
            {
                logger.Warn($"No existing {name} settings found.");
                config = LoadSettingFile<T>(backupPath);
                if (config == null)
                {
                    logger.Warn($"No {name} settings backup found, creating default ones.");
                    if (generateDefault)
                    {
                        config = new T();
                    }
                }
            }

            return config;
        }

        public void SaveSettings()
        {
            try
            {
                FileSystem.CreateDirectory(PlaynitePaths.ConfigRootPath);
                SaveSettingFile(this, PlaynitePaths.ConfigFilePath);
                SaveSettingFile(WindowPositions, PlaynitePaths.WindowPositionsPath);
                SaveSettingFile(Fullscreen, PlaynitePaths.FullscreenConfigFilePath);
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to save application settings.");
            }
        }

        public void BackupSettings()
        {
            try
            {
                FileSystem.CreateDirectory(PlaynitePaths.ConfigRootPath);
                SaveSettingFile(this, PlaynitePaths.BackupConfigFilePath);
                SaveSettingFile(WindowPositions, PlaynitePaths.BackupWindowPositionsPath);
                SaveSettingFile(Fullscreen, PlaynitePaths.BackupFullscreenConfigFilePath);
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to backup application settings.");
            }
        }

        public static void ConfigureLogger()
        {
            var config = new LoggingConfiguration();
#if DEBUG
            var consoleTarget = new ColoredConsoleTarget()
            {
                Layout = @"${level:uppercase=true:padding=-5}|${logger}:${message}${onexception:${newline}${exception}}"
            };

            config.AddTarget("console", consoleTarget);

            var rule1 = new LoggingRule("*", LogLevel.Trace, consoleTarget);
            config.LoggingRules.Add(rule1);
#endif
            var coreFileTarget = new FileTarget()
            {
                FileName = Path.Combine(PlaynitePaths.ConfigRootPath, "playnite.log"),
                Layout = "${date:format=dd-MM HH\\:mm\\:ss.fff}|${level:uppercase=true:padding=-5}|${logger}:${message}${onexception:${newline}${exception:format=toString}}",
                KeepFileOpen = false,
                ArchiveFileName = Path.Combine(PlaynitePaths.ConfigRootPath, "playnite.{#####}.log"),
                ArchiveAboveSize = 4096000,
                ArchiveNumbering = ArchiveNumberingMode.Sequence,
                MaxArchiveFiles = 2,
                Encoding = Encoding.UTF8
            };

            var extensionFileTarget = new FileTarget()
            {
                FileName = Path.Combine(PlaynitePaths.ConfigRootPath, "extensions.log"),
                Layout = "${date:format=dd-MM HH\\:mm\\:ss.fff}|${level:uppercase=true:padding=-5}|${logger}:${message}${onexception:${newline}${exception:format=toString}}",
                KeepFileOpen = false,
                ArchiveFileName = Path.Combine(PlaynitePaths.ConfigRootPath, "extensions.{#####}.log"),
                ArchiveAboveSize = 4096000,
                ArchiveNumbering = ArchiveNumberingMode.Sequence,
                MaxArchiveFiles = 2,
                Encoding = Encoding.UTF8
            };

            var allRule = new LoggingRule("*", LogLevel.Trace, coreFileTarget);
            allRule.Filters.Add(new NLog.Filters.ConditionBasedFilter()
            {
                Condition = "contains('${logger}', '#')",
                Action = NLog.Filters.FilterResult.Ignore
            });

            config.LoggingRules.Add(allRule);
            config.LoggingRules.Add(new LoggingRule("*#*", LogLevel.Trace, extensionFileTarget));

            NLog.LogManager.Configuration = config;
            SDK.LogManager.Init(new NLogLogProvider());
            logger = SDK.LogManager.GetLogger();
        }

        public static string GetAppConfigValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public static bool GetAppConfigBoolValue(string key)
        {
            if (bool.TryParse(ConfigurationManager.AppSettings[key], out var result))
            {
                return result;
            }
            else
            {
                return false;
            }
        }

        public static void MigrateSettingsConfig()
        {
        }

        private Thickness GetItemSpacingMargin()
        {
            return new Thickness(GridItemSpacing / 2, GridItemSpacing / 2, GridItemSpacing / 2, GridItemSpacing / 2);;
        }

        private Thickness GetFullscreenItemSpacingMargin()
        {
            double marginX = FullscreenItemSpacing / 2;
            double marginY = CoverAspectRatio.GetWidth(FullscreenItemSpacing) / 2;
            return new Thickness(marginY / 2, marginX / 2, marginY / 2, marginX / 2);
        }

        private void UpdateGridItemHeight()
        {
            if (GridItemWidth != 0)
            {
                GridItemHeight = Math.Round(GridItemWidth * ((double)gridItemHeightRatio / GridItemWidthRatio));
            }
            else
            {
                GridItemHeight = 0;
            }

            OnPropertyChanged(nameof(GridItemHeight));
        }

        public bool ShouldCheckProgramUpdatePeriodic()
        {
            switch (CheckForProgramUpdates)
            {
                case UpdateCheckFrequency.OnceADay:
                    return DateTimes.Now.Date != LastProgramUpdateCheck.Date;
                case UpdateCheckFrequency.OnceAWeek:
                    return (DateTimes.Now - LastProgramUpdateCheck).TotalDays > 6;
                case UpdateCheckFrequency.OnEveryStartup:
                default:
                    return false;
            }
        }

        public bool ShouldCheckAddonUpdatePeriodic()
        {
            switch (CheckForAddonUpdates)
            {
                case UpdateCheckFrequency.OnceADay:
                    return DateTimes.Now.Date != LastAddonUpdateCheck.Date;
                case UpdateCheckFrequency.OnceAWeek:
                    return (DateTimes.Now - LastAddonUpdateCheck).TotalDays > 6;
                case UpdateCheckFrequency.OnEveryStartup:
                default:
                    return false;
            }
        }

        public bool ShouldCheckProgramUpdateStartup()
        {
            switch (CheckForProgramUpdates)
            {
                case UpdateCheckFrequency.OnceADay:
                    return DateTimes.Now.Date != LastProgramUpdateCheck.Date;
                case UpdateCheckFrequency.OnceAWeek:
                    return (DateTimes.Now - LastProgramUpdateCheck).TotalDays > 6;
                case UpdateCheckFrequency.OnEveryStartup:
                default:
                    return true;
            }
        }

        public bool ShouldCheckAddonUpdateStartup()
        {
            switch (CheckForAddonUpdates)
            {
                case UpdateCheckFrequency.OnceADay:
                    return DateTimes.Now.Date != LastAddonUpdateCheck.Date;
                case UpdateCheckFrequency.OnceAWeek:
                    return (DateTimes.Now - LastAddonUpdateCheck).TotalDays > 6;
                case UpdateCheckFrequency.OnEveryStartup:
                default:
                    return true;
            }
        }

        public bool ShouldCheckLibraryOnStartup()
        {
            switch (CheckForLibraryUpdates)
            {
                case LibraryUpdateCheckFrequency.OnceADay:
                    return DateTimes.Now.Date != LastLibraryUpdateCheck.Date;
                case LibraryUpdateCheckFrequency.OnceAWeek:
                    return (DateTimes.Now - LastLibraryUpdateCheck).TotalDays > 6;
                case LibraryUpdateCheckFrequency.Manually:
                    return false;
                case LibraryUpdateCheckFrequency.OnEveryStartup:
                default:
                    return true;
            }
        }

        public bool ShouldCheckEmuLibraryOnStartup()
        {
            switch (CheckForEmulatedLibraryUpdates)
            {
                case LibraryUpdateCheckFrequency.OnceADay:
                    return DateTimes.Now.Date != LastEmuLibraryUpdateCheck.Date;
                case LibraryUpdateCheckFrequency.OnceAWeek:
                    return (DateTimes.Now - LastEmuLibraryUpdateCheck).TotalDays > 6;
                case LibraryUpdateCheckFrequency.Manually:
                    return false;
                case LibraryUpdateCheckFrequency.OnEveryStartup:
                default:
                    return true;
            }
        }

        public bool ShouldDataBackupOnStartup()
        {
            if (!AutoBackupEnabled)
            {
                return false;
            }

            switch (AutoBackupFrequency)
            {
                case AutoBackupFrequency.OnceADay:
                    return DateTimes.Now.Date != LastAutoBackup.Date;
                case AutoBackupFrequency.OnceAWeek:
                    return (DateTimes.Now - LastAutoBackup).TotalDays > 6;
                default:
                    return false;
            }
        }

        #region Serialization Conditions

        public bool ShouldSerializeDisabledPlugins()
        {
            return DisabledPlugins.HasItems();
        }

        #endregion Serialization Conditions
    }
}
