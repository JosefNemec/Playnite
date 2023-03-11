using Newtonsoft.Json;
using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Playnite
{
    public class ListViewColumnProperty : ObservableObject
    {
        public GameField Field { get; set; }

        private bool visible = false;
        public bool Visible
        {
            get
            {
                return visible;
            }

            set
            {
                visible = value;
                OnPropertyChanged();
            }
        }

        private double width = double.NaN;
        public double Width
        {
            get
            {
                return width;
            }

            set
            {
                width = value;
                // Don't allow exteremly small size because it could lead to a user accidentely hiding the column by resizing: #2257
                if (width < 25)
                {
                    width = 25;
                }

                OnPropertyChanged();
            }
        }

        public ListViewColumnProperty()
        {
        }

        public ListViewColumnProperty(GameField field)
        {
            Field = field;
        }
    }

    public class ListViewColumnsProperties : ObservableObject
    {
        private ListViewColumnProperty icon = new ListViewColumnProperty(GameField.Icon);
        public ListViewColumnProperty Icon
        {
            get
            {
                return icon;
            }

            set
            {
                icon = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty name = new ListViewColumnProperty(GameField.Name);
        public ListViewColumnProperty Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty platform = new ListViewColumnProperty(GameField.Platforms);
        public ListViewColumnProperty Platform
        {
            get
            {
                return platform;
            }

            set
            {
                platform = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty developers = new ListViewColumnProperty(GameField.Developers);
        public ListViewColumnProperty Developers
        {
            get
            {
                return developers;
            }

            set
            {
                developers = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty publishers = new ListViewColumnProperty(GameField.Publishers);
        public ListViewColumnProperty Publishers
        {
            get
            {
                return publishers;
            }

            set
            {
                publishers = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty releaseDate = new ListViewColumnProperty(GameField.ReleaseDate);
        public ListViewColumnProperty ReleaseDate
        {
            get
            {
                return releaseDate;
            }

            set
            {
                releaseDate = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty genres = new ListViewColumnProperty(GameField.Genres);
        public ListViewColumnProperty Genres
        {
            get
            {
                return genres;
            }

            set
            {
                genres = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty lastActivity = new ListViewColumnProperty(GameField.LastActivity);
        public ListViewColumnProperty LastActivity
        {
            get
            {
                return lastActivity;
            }

            set
            {
                lastActivity = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty recentActivity = new ListViewColumnProperty(GameField.RecentActivity);
        public ListViewColumnProperty RecentActivity
        {
            get
            {
                return recentActivity;
            }

            set
            {
                recentActivity = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty isInstalled = new ListViewColumnProperty(GameField.IsInstalled);
        public ListViewColumnProperty IsInstalled
        {
            get
            {
                return isInstalled;
            }

            set
            {
                isInstalled = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty installDirectory = new ListViewColumnProperty(GameField.InstallDirectory);
        public ListViewColumnProperty InstallDirectory
        {
            get
            {
                return installDirectory;
            }

            set
            {
                installDirectory = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty categories = new ListViewColumnProperty(GameField.Categories);
        public ListViewColumnProperty Categories
        {
            get
            {
                return categories;
            }

            set
            {
                categories = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty playtime = new ListViewColumnProperty(GameField.Playtime);
        public ListViewColumnProperty Playtime
        {
            get
            {
                return playtime;
            }

            set
            {
                playtime = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty added = new ListViewColumnProperty(GameField.Added);
        public ListViewColumnProperty Added
        {
            get
            {
                return added;
            }

            set
            {
                added = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty modified = new ListViewColumnProperty(GameField.Modified);
        public ListViewColumnProperty Modified
        {
            get
            {
                return modified;
            }

            set
            {
                modified = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty playCount = new ListViewColumnProperty(GameField.PlayCount);
        public ListViewColumnProperty PlayCount
        {
            get
            {
                return playCount;
            }

            set
            {
                playCount = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty installSize = new ListViewColumnProperty(GameField.InstallSize);
        public ListViewColumnProperty InstallSize
        {
            get
            {
                return installSize;
            }

            set
            {
                installSize = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty series = new ListViewColumnProperty(GameField.Series);
        public ListViewColumnProperty Series
        {
            get
            {
                return series;
            }

            set
            {
                series = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty version = new ListViewColumnProperty(GameField.Version);
        public ListViewColumnProperty Version
        {
            get
            {
                return version;
            }

            set
            {
                version = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty ageRating = new ListViewColumnProperty(GameField.AgeRatings);
        public ListViewColumnProperty AgeRating
        {
            get
            {
                return ageRating;
            }

            set
            {
                ageRating = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty region = new ListViewColumnProperty(GameField.Regions);
        public ListViewColumnProperty Region
        {
            get
            {
                return region;
            }

            set
            {
                region = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty source = new ListViewColumnProperty(GameField.Source);
        public ListViewColumnProperty Source
        {
            get
            {
                return source;
            }

            set
            {
                source = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty completionStatus = new ListViewColumnProperty(GameField.CompletionStatus);
        public ListViewColumnProperty CompletionStatus
        {
            get
            {
                return completionStatus;
            }

            set
            {
                completionStatus = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty userScore = new ListViewColumnProperty(GameField.UserScore);
        public ListViewColumnProperty UserScore
        {
            get
            {
                return userScore;
            }

            set
            {
                userScore = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty criticScore = new ListViewColumnProperty(GameField.CriticScore);
        public ListViewColumnProperty CriticScore
        {
            get
            {
                return criticScore;
            }

            set
            {
                criticScore = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty communityScore = new ListViewColumnProperty(GameField.CommunityScore);
        public ListViewColumnProperty CommunityScore
        {
            get
            {
                return communityScore;
            }

            set
            {
                communityScore = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty tags = new ListViewColumnProperty(GameField.Tags);
        public ListViewColumnProperty Tags
        {
            get
            {
                return tags;
            }

            set
            {
                tags = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty pluginId = new ListViewColumnProperty(GameField.PluginId);
        public ListViewColumnProperty PluginId
        {
            get
            {
                return pluginId;
            }

            set
            {
                pluginId = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty features = new ListViewColumnProperty(GameField.Features);
        public ListViewColumnProperty Features
        {
            get
            {
                return features;
            }

            set
            {
                features = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty roms = new ListViewColumnProperty(GameField.Roms);
        public ListViewColumnProperty Roms
        {
            get
            {
                return roms;
            }

            set
            {
                roms = value;
                OnPropertyChanged();
            }
        }
    }

    public class ViewSettingsBase : ObservableObject
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

        private SortOrderDirection sortingOrderDirection = SortOrderDirection.Ascending;
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
    }

    public class ViewSettings : ViewSettingsBase
    {
        public const double MinGridItemWidth = 60;
        public const double DefaultGridItemWidth = 200;
        public const double MaxGridItemWidth = 700;

        private GroupableField groupingOrder = GroupableField.None;
        public GroupableField GroupingOrder
        {
            get
            {
                return groupingOrder;
            }

            set
            {
                groupingOrder = value;
                OnPropertyChanged();
            }
        }

        private DesktopView gamesViewType = DesktopView.Details;
        public DesktopView GamesViewType
        {
            get
            {
                return gamesViewType;
            }

            set
            {
                gamesViewType = value;
                OnPropertyChanged();
            }
        }

        private ExplorerField selectedExplorerField = ExplorerField.Library;
        public ExplorerField SelectedExplorerField
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

        private ListViewColumnsProperties listViewColumns;
        public ListViewColumnsProperties ListViewColumns
        {
            get
            {
                return listViewColumns;
            }

            set
            {
                listViewColumns = value;
                OnPropertyChanged();
            }
        }

        private List<GameField> listViewColumsOrder;
        public List<GameField> ListViewColumsOrder
        {
            get
            {
                return listViewColumsOrder;
            }

            set
            {
                listViewColumsOrder = value;
                OnPropertyChanged();
            }
        }

        private Dictionary<GroupableField, List<string>> collapsedGroups = new Dictionary<GroupableField, List<string>>();
        public Dictionary<GroupableField, List<string>> CollapsedGroups
        {
            get
            {
                return collapsedGroups;
            }

            set
            {
                collapsedGroups = value;
                OnPropertyChanged();
            }
        }

        public bool IsGroupCollapsed(GroupableField field, string groupName)
        {
            if (collapsedGroups.ContainsKey(field) &&
                collapsedGroups[field].ContainsString(groupName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public void ExpandAllGroups(GroupableField field)
        {
            if (collapsedGroups.ContainsKey(field))
            {
                collapsedGroups.Remove(field);
                OnPropertyChanged(nameof(CollapsedGroups));
            }
        }

        public void CollapseGroups(GroupableField field, List<string> groupNames)
        {
            if (!collapsedGroups.ContainsKey(field))
            {
                collapsedGroups.Add(field, new List<string>());
            }

            foreach (var groupName in groupNames)
            {
                if (!collapsedGroups[field].ContainsString(groupName, StringComparison.OrdinalIgnoreCase))
                {
                    collapsedGroups[field].Add(groupName);
                }
            }

            OnPropertyChanged(nameof(CollapsedGroups));
        }

        public void SetGroupCollapseState(GroupableField field, string groupName, bool collapsed)
        {
            if (collapsed)
            {
                if (!collapsedGroups.ContainsKey(field))
                {
                    collapsedGroups.Add(field, new List<string>());
                }

                if (!collapsedGroups[field].ContainsString(groupName, StringComparison.OrdinalIgnoreCase))
                {
                    collapsedGroups[field].Add(groupName);
                    OnPropertyChanged(nameof(CollapsedGroups));
                }
            }
            else
            {
                if (collapsedGroups.ContainsKey(field))
                {
                    var existing = collapsedGroups[field].FirstOrDefault(a => string.Equals(a, groupName, StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                    {
                        collapsedGroups[field].Remove(existing);
                    }

                    if (collapsedGroups[field].Count == 0)
                    {
                        collapsedGroups.Remove(field);
                    }

                    OnPropertyChanged(nameof(CollapsedGroups));
                }
            }
        }

        public ViewSettings()
        {
        }
    }
}
