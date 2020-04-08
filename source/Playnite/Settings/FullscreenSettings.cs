using Newtonsoft.Json;
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

    public class FullscreenViewSettings : ObservableObject
    {
        private SortOrder sortingOrder = SortOrder.Name;
        public SortOrder SortingOrder
        {
            get
            {
                return sortingOrder;
            }

            set
            {
                sortingOrder = value;
                OnPropertyChanged();
            }
        }

        private SortOrderDirection sortingOrderDirection = SortOrderDirection.Descending;
        public SortOrderDirection SortingOrderDirection
        {
            get
            {
                return sortingOrderDirection;
            }

            set
            {
                sortingOrderDirection = value;
                OnPropertyChanged();
            }
        }

        private GroupableField selectedExplorerField = GroupableField.Library;
        public GroupableField SelectedExplorerField
        {
            get
            {
                return selectedExplorerField;
            }

            set
            {
                selectedExplorerField = value;
                OnPropertyChanged();
            }
        }
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
                    CompletionStatus?.IsSet == true ||
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
        public List<ThemeDescription> AvailableThemes => ThemeManager.GetAvailableThemes(ApplicationMode.Fullscreen);

        [JsonIgnore]
        public const FullscreenButtonPrompts DefaultButtonPrompts = FullscreenButtonPrompts.Xbox;

        private ActiveFullscreenView activeView = ActiveFullscreenView.RecentlyPlayed;
        public ActiveFullscreenView ActiveView
        {
            get
            {
                return activeView;
            }

            set
            {
                activeView = value;
                OnPropertyChanged();
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

        private string theme = "Default";
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

        private bool installedOnlyInQuickFilters = false;
        public bool InstalledOnlyInQuickFilters
        {
            get
            {
                return installedOnlyInQuickFilters;
            }

            set
            {
                installedOnlyInQuickFilters = value;
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
    }
}
