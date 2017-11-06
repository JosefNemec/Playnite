using NLog;
using Playnite;
using Playnite.Database;
using Playnite.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PlayniteUI
{
    public class CategoryView : IComparable
    {
        public string Category
        {
            get; set;
        }

        public CategoryView(string category)
        {
            Category = category;
        }

        public int CompareTo(object obj)
        {
            var cat = (obj as CategoryView).Category;

            if (string.IsNullOrEmpty(Category) && string.IsNullOrEmpty(cat))
            {
                return 0;
            }
            if (string.IsNullOrEmpty(Category))
            {
                return 1;
            }
            if (string.IsNullOrEmpty(cat))
            {
                return -1;
            }
            if (Category.Equals(cat))
            {
                return 0;
            }

            return string.Compare(Category, cat, true);
        }

        public override bool Equals(object obj)
        {
            var cat = ((CategoryView)obj).Category;

            if (string.IsNullOrEmpty(Category) && string.IsNullOrEmpty(cat))
            {
                return true;
            }
            if (string.IsNullOrEmpty(Category))
            {
                return false;
            }
            if (string.IsNullOrEmpty(cat))
            {
                return false;
            }
            if (Category.Equals(cat))
            {
                return true;
            }

            return string.Compare(Category, cat, true) == 0;
        }

        public override int GetHashCode()
        {
            if (Category == null)
            {
                return 0;
            }
            else
            {
                return Category.GetHashCode();
            }
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Category) ? "No Category" : Category;
        }
    }

    public class PlatformView : IComparable
    {
        public Platform Platform
        {
            get; set;
        }

        public int? PlatformId
        {
            get; set;
        }

        public string Name
        {
            get => Platform?.Name ?? string.Empty;
        }

        public PlatformView(int? platformId, Platform platform)
        {
            Platform = platform;
            PlatformId = platformId;
        }

        public int CompareTo(object obj)
        {
            var platform = (obj as PlatformView).Name;

            if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(platform))
            {
                return 0;
            }
            if (string.IsNullOrEmpty(Name))
            {
                return 1;
            }
            if (string.IsNullOrEmpty(platform))
            {
                return -1;
            }
            if (Name.Equals(platform))
            {
                return 0;
            }

            return string.Compare(Name, platform, true);
        }

        public override bool Equals(object obj)
        {
            var platform = (obj as PlatformView).Name;

            if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(platform))
            {
                return true;
            }
            if (string.IsNullOrEmpty(Name))
            {
                return false;
            }
            if (string.IsNullOrEmpty(platform))
            {
                return false;
            }
            if (Name.Equals(platform))
            {
                return true;
            }

            return string.Compare(Name, platform, true) == 0;
        }

        public override int GetHashCode()
        {
            if (Name == null)
            {
                return 0;
            }
            else
            {
                return Name.GetHashCode();
            }
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? "No Platform" : Name;
        }
    }

    public class GameViewEntry : INotifyPropertyChanged
    {
        public int Id => Game.Id;
        public Provider Provider => Game.Provider;
        public List<string> Categories => Game.Categories;
        public List<string> Genres => Game.Genres;
        public DateTime? ReleaseDate => Game.ReleaseDate;
        public DateTime? LastActivity => Game.LastActivity;
        public List<string> Developers => Game.Developers;
        public List<string> Publishers => Game.Publishers;
        public ObservableCollection<Link> Links => Game.Links;
        public string Icon => Game.Icon;
        public string Image => Game.Image;
        public string BackgroundImage => Game.BackgroundImage;
        public bool IsInstalled => Game.IsInstalled;
        public bool Hidden => Game.Hidden;
        public bool Favorite => Game.Favorite;
        public string InstallDirectory => Game.InstallDirectory;
        public int? PlatformId => Game.PlatformId;
        public ObservableCollection<GameTask> OtherTasks => Game.OtherTasks;
        public string DescriptionView => Game.DescriptionView;
        public string DisplayName => Game.Name;        

        public string Name
        {
            get
            {
                return (string.IsNullOrEmpty(Game.SortingName)) ? Game.Name : Game.SortingName;
            }           
        }

        public bool IsSetupInProgress
        {
            get
            {
                return Game is Game ? (Game as Game).IsSetupInProgress : false;
            }
        }

        public CategoryView Category
        {
            get; set;
        }

        private PlatformView platform;
        public PlatformView Platform
        {
            get => platform;
            set
            {
                platform = value;
                OnPropertyChanged("PlatformId");
            }
        }

        public IGame Game
        {
            get; set;
        }       

        public string DefaultIcon
        {
            get
            {
                if (!string.IsNullOrEmpty(Platform?.Platform?.Icon))
                {
                    return Platform.Platform.Icon;
                }
                else
                {
                    switch (Game.Provider)
                    {
                        case Provider.GOG:
                            return @"resources:/Images/gogicon.png";
                        case Provider.Origin:
                            return @"resources:/Images/originicon.png";
                        case Provider.Steam:
                            return @"resources:/Images/steamicon.png";
                        case Provider.Uplay:
                            return @"resources:/Images/uplayicon.png";
                        case Provider.BattleNet:
                            return @"resources:/Images/battleneticon.png";
                        case Provider.Custom:
                        default:
                            return @"resources:/Images/applogo.png";
                    }
                }
            }
        }

        public string DefaultImage
        {
            get
            {
                if (!string.IsNullOrEmpty(Platform?.Platform?.Cover))
                {
                    return Platform.Platform.Cover;
                }
                else
                {
                    switch (Game.Provider)
                    {
                        case Provider.GOG:
                        case Provider.Origin:
                        case Provider.Steam:
                        case Provider.Uplay:
                        case Provider.Custom:
                        default:
                            return @"resources:/Images/custom_cover_background.png";
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public GameViewEntry(IGame game, string category, Platform platform)
        {
            Category = new CategoryView(category);
            Game = game;
            Game.PropertyChanged += Game_PropertyChanged;
            Platform = new PlatformView(PlatformId, platform);
        }

        private void Game_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        public void OnPropertyChanged(string propertyName)
        {            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName == "PlatformId")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Platform"));
            }

            if (propertyName == "SortingName")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
            }
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", Name, Category);
        }

        public static explicit operator Game(GameViewEntry entry)
        {
            return entry.Game as Game;
        }
    }

    public enum GamesViewType
    {
        Standard,
        CategoryGrouped
    }

    public class GamesCollectionView : ObservableObject, IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private GameDatabase database;
        private List<Platform> platformsCache;

        private ListCollectionView collectionView;
        public ListCollectionView CollectionView
        {
            get => collectionView;
            private set
            {
                collectionView = value;
                OnPropertyChanged("CollectionView");
            }
        }

        public Settings Settings
        {
            get;
            private set;
        }

        private GamesViewType? viewType = null;
        public GamesViewType? ViewType
        {
            get => viewType;
            set
            {
                if (value == viewType)
                {
                    return;
                }

                SetViewType(value);
                viewType = value;
            }
        }

        public RangeObservableCollection<GameViewEntry> Items
        {
            get; set;
        }

        public GamesCollectionView(GameDatabase database, Settings settings)
        {
            this.database = database;
            platformsCache = database.PlatformsCollection.FindAll().ToList();
            database.GamesCollectionChanged += Database_GamesCollectionChanged;
            database.GameUpdated += Database_GameUpdated;
            database.PlatformsCollectionChanged += Database_PlatformsCollectionChanged;
            database.PlatformUpdated += Database_PlatformUpdated;
            Items = new RangeObservableCollection<GameViewEntry>();
            Settings = settings;
            Settings.PropertyChanged += Settings_PropertyChanged;
            Settings.FilterSettings.PropertyChanged += FilterSettings_PropertyChanged;
            CollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(Items);
            SetViewConfiguration();
            CollectionView.Filter = Filter;
        }

        public void Dispose()
        {
            database.GamesCollectionChanged -= Database_GamesCollectionChanged;
            database.GameUpdated -= Database_GameUpdated;
            database.PlatformsCollectionChanged -= Database_PlatformsCollectionChanged;
            database.PlatformUpdated -= Database_PlatformUpdated;
            Settings.PropertyChanged -= Settings_PropertyChanged;
            Settings.FilterSettings.PropertyChanged -= FilterSettings_PropertyChanged;
        }

        private bool Filter(object item)
        {
            var entry = (GameViewEntry)item;
            var game = entry.Game;

            if (!Settings.FilterSettings.Active)
            {
                if (game.Hidden)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            // ------------------ Installed
            bool installedResult = false;
            if (Settings.FilterSettings.IsInstalled && game.IsInstalled)
            {
                installedResult = true;
            }
            else if (!Settings.FilterSettings.IsInstalled)
            {
                installedResult = true;
            }

            // ------------------ UnInstalled
            bool unInstalledResult = false;
            if (Settings.FilterSettings.IsUnInstalled && !game.IsInstalled)
            {
                unInstalledResult = true;
            }
            else if (!Settings.FilterSettings.IsUnInstalled)
            {
                unInstalledResult = true;
            }

            // ------------------ Hidden
            bool hiddenResult = true;
            if (Settings.FilterSettings.Hidden && game.Hidden)
            {
                hiddenResult = true;
            }
            else if (!Settings.FilterSettings.Hidden && game.Hidden)
            {
                hiddenResult = false;
            }
            else if (Settings.FilterSettings.Hidden && !game.Hidden)
            {
                hiddenResult = false;
            }

            // ------------------ Favorite
            bool favoriteResult = false;
            if (Settings.FilterSettings.Favorite && game.Favorite)
            {
                favoriteResult = true;
            }
            else if (!Settings.FilterSettings.Favorite)
            {
                favoriteResult = true;
            }

            // ------------------ Providers
            bool providersFilter = false;
            if (Settings.FilterSettings.Steam == false &&
                Settings.FilterSettings.Origin == false &&
                Settings.FilterSettings.GOG == false &&
                Settings.FilterSettings.Custom == false &&
                Settings.FilterSettings.Uplay == false &&
                Settings.FilterSettings.BattleNet == false)
            {
                providersFilter = true;
            }
            else
            {
                switch (game.Provider)
                {
                    case Provider.Custom:
                        if (Settings.FilterSettings.Custom)
                        {
                            providersFilter = true;
                        }
                        break;
                    case Provider.GOG:
                        if (Settings.FilterSettings.GOG)
                        {
                            providersFilter = true;
                        }
                        break;
                    case Provider.Origin:
                        if (Settings.FilterSettings.Origin)
                        {
                            providersFilter = true;
                        }
                        break;
                    case Provider.Steam:
                        if (Settings.FilterSettings.Steam)
                        {
                            providersFilter = true;
                        }
                        break;
                    case Provider.Uplay:
                        if (Settings.FilterSettings.Uplay)
                        {
                            providersFilter = true;
                        }
                        break;
                    case Provider.BattleNet:
                        if (Settings.FilterSettings.BattleNet)
                        {
                            providersFilter = true;
                        }
                        break;
                }
            }

            // ------------------ Name filter
            bool textResult = false;
            if (string.IsNullOrEmpty(Settings.FilterSettings.Name))
            {
                textResult = true;
            }
            else
            {
                if (string.IsNullOrEmpty(game.Name))
                {
                    textResult = false;
                }
                else
                {
                    textResult = (game.Name.IndexOf(Settings.FilterSettings.Name, StringComparison.OrdinalIgnoreCase) >= 0);
                }
            }

            // ------------------ Genre
            bool genreResult = false;
            if (Settings.FilterSettings.Genres == null || Settings.FilterSettings.Genres.Count == 0)
            {
                genreResult = true;
            }
            else
            {
                if (game.Genres == null)
                {
                    genreResult = false;
                }
                else
                {
                    genreResult = Settings.FilterSettings.Genres.IntersectsPartiallyWith(game.Genres);
                }
            }

            // ------------------ Platform
            bool platformResult = false;
            if (Settings.FilterSettings.Platforms == null || Settings.FilterSettings.Platforms.Count == 0)
            {
                platformResult = true;
            }
            else
            {
                if (game.PlatformId == null)
                {
                    platformResult = false;
                }
                else
                {
                    var platform = GetPlatformFromCache(game.PlatformId);
                    if (platform == null)
                    {
                        platformResult = false;
                    }
                    else
                    {
                        platformResult = Settings.FilterSettings.Platforms.Any(a => !string.IsNullOrEmpty(a) && platform.Name.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0);
                    }
                }
            }

            // ------------------ Release Date
            bool releaseDateResult = false;
            if (string.IsNullOrEmpty(Settings.FilterSettings.ReleaseDate))
            {
                releaseDateResult = true;
            }
            else
            {
                if (game.ReleaseDate == null)
                {

                    releaseDateResult = false;
                }
                else
                {
                    releaseDateResult = game.ReleaseDate.Value.ToString(Constants.DateUiFormat).IndexOf(Settings.FilterSettings.ReleaseDate, StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }

            // ------------------ Publisher
            bool publisherResult = false;
            if (Settings.FilterSettings.Publishers == null || Settings.FilterSettings.Publishers.Count == 0)
            {
                publisherResult = true;
            }
            else
            {
                if (game.Publishers == null)
                {
                    publisherResult = false;
                }
                else
                {
                    publisherResult = Settings.FilterSettings.Publishers.IntersectsPartiallyWith(game.Publishers);
                }
            }

            // ------------------ Developer
            bool developerResult = false;
            if (Settings.FilterSettings.Developers == null || Settings.FilterSettings.Developers.Count == 0)
            {
                developerResult = true;
            }
            else
            {
                if (game.Developers == null)
                {
                    developerResult = false;
                }
                else
                {
                    developerResult = Settings.FilterSettings.Developers.IntersectsPartiallyWith(game.Developers);
                }
            }

            // ------------------ Category
            bool categoryResult = false;
            if (Settings.FilterSettings.Categories == null || Settings.FilterSettings.Categories.Count == 0)
            {
                categoryResult = true;
            }
            else
            {
                if (game.Categories == null)
                {
                    categoryResult = false;
                }
                else
                {
                    if (ViewType == GamesViewType.Standard)
                    {
                        categoryResult = Settings.FilterSettings.Categories.IntersectsPartiallyWith(game.Categories);
                    }
                    else
                    {
                        categoryResult = Settings.FilterSettings.Categories.Any(a => entry.Category.Category.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0);
                    }
                }
            }

            return installedResult && unInstalledResult && hiddenResult && favoriteResult && textResult && providersFilter && genreResult && platformResult && releaseDateResult && publisherResult && developerResult && categoryResult;
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((new string[] { "SortingOrder", "GroupingOrder", "SortingOrderDirection" }).Contains(e.PropertyName))
            {
                UpdateViewConfiguration();
            }
        }

        private void FilterSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Active")
            {
                return;
            }

            logger.Debug("Refreshing collection view filter.");
            CollectionView.Refresh();
        }

        private void SetViewDescriptions()
        {
            var sortDirection = Settings.SortingOrderDirection == SortOrderDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending;

            switch (Settings.GroupingOrder)
            {
                case GroupOrder.None:
                    ViewType = GamesViewType.Standard;
                    break;
                case GroupOrder.Provider:
                    ViewType = GamesViewType.Standard;
                    break;
                case GroupOrder.Platform:
                    ViewType = GamesViewType.Standard;
                    break;
                case GroupOrder.Category:
                    ViewType = GamesViewType.CategoryGrouped;
                    break;
            }

            CollectionView.SortDescriptions.Add(new SortDescription(Settings.SortingOrder.ToString(), sortDirection));
            if (Settings.SortingOrder != SortOrder.Name)
            {
                CollectionView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            }

            if (Settings.GroupingOrder != GroupOrder.None)
            {
                CollectionView.GroupDescriptions.Add(new PropertyGroupDescription(Settings.GroupingOrder.ToString()));
                if (CollectionView.SortDescriptions.First().PropertyName != Settings.GroupingOrder.ToString())
                {
                    CollectionView.SortDescriptions.Insert(0, new SortDescription(Settings.GroupingOrder.ToString(), ListSortDirection.Ascending));
                }
            }
        }

        private void SetViewConfiguration()
        {
            using (CollectionView.DeferRefresh())
            {
                SetViewDescriptions();
                CollectionView.LiveGroupingProperties.Add("Provider");
                CollectionView.LiveGroupingProperties.Add("Category");
                CollectionView.LiveGroupingProperties.Add("Platform");

                CollectionView.LiveSortingProperties.Add("Provider");
                CollectionView.LiveSortingProperties.Add("Name");
                CollectionView.LiveSortingProperties.Add("Categories");
                CollectionView.LiveSortingProperties.Add("Genres");
                CollectionView.LiveSortingProperties.Add("ReleaseDate");
                CollectionView.LiveSortingProperties.Add("Developers");
                CollectionView.LiveSortingProperties.Add("Publishers");
                CollectionView.LiveSortingProperties.Add("IsInstalled");
                CollectionView.LiveSortingProperties.Add("Hidden");
                CollectionView.LiveSortingProperties.Add("Favorite");
                CollectionView.LiveSortingProperties.Add("LastActivity");
                CollectionView.LiveSortingProperties.Add("Platform");

                CollectionView.LiveFilteringProperties.Add("Provider");
                CollectionView.LiveFilteringProperties.Add("Name");
                CollectionView.LiveFilteringProperties.Add("Categories");
                CollectionView.LiveFilteringProperties.Add("Genres");
                CollectionView.LiveFilteringProperties.Add("ReleaseDate");
                CollectionView.LiveFilteringProperties.Add("Developers");
                CollectionView.LiveFilteringProperties.Add("Publishers");
                CollectionView.LiveFilteringProperties.Add("IsInstalled");
                CollectionView.LiveFilteringProperties.Add("Hidden");
                CollectionView.LiveFilteringProperties.Add("Favorite");
                CollectionView.LiveFilteringProperties.Add("PlatformId");

                CollectionView.IsLiveSorting = true;
                CollectionView.IsLiveFiltering = true;
                CollectionView.IsLiveGrouping = true;
            };
        }

        private void UpdateViewConfiguration()
        {
            logger.Debug("Updating collection view settings.");
            using (CollectionView.DeferRefresh())
            {
                CollectionView.SortDescriptions.Clear();
                CollectionView.GroupDescriptions.Clear();
                SetViewDescriptions();                
            }
        }

        public void SetViewType(GamesViewType? viewType)
        {
            if (viewType == ViewType)
            {
                return;
            }

            switch (viewType)
            {
                case GamesViewType.Standard:
                    Items.Clear();
                    Items.AddRange(database.GamesCollection.FindAll().Select(x => new GameViewEntry(x, string.Empty, GetPlatformFromCache(x.PlatformId))));
                    break;

                case GamesViewType.CategoryGrouped:
                    Items.Clear();
                    Items.AddRange(database.GamesCollection.FindAll().SelectMany(x =>
                    {
                        if (x.Categories == null || x.Categories.Count == 0)
                        {
                            return new List<GameViewEntry>()
                            {
                                new GameViewEntry(x, null, GetPlatformFromCache(x.PlatformId))
                            };
                        }
                        else
                        {
                            return x.Categories.Select(c =>
                            {
                                return new GameViewEntry(x, c, GetPlatformFromCache(x.PlatformId));
                            });
                        }
                    }));

                    break;
            }

            this.viewType = viewType;
        }

        private Platform GetPlatformFromCache(int? id)
        {
            return platformsCache?.FirstOrDefault(a => a.Id == id);
        }

        private void Database_PlatformUpdated(object sender, PlatformUpdatedEventArgs args)
        {
            platformsCache = database.PlatformsCollection.FindAll().ToList();
            var platformIds = args.UpdatedPlatforms.Select(a => a.NewData.Id).ToList();
            foreach (var item in Items.Where(a => a.PlatformId != null && platformIds.Contains(a.PlatformId.Value)))
            {
                item.Platform.Platform = GetPlatformFromCache(item.PlatformId);
                item.OnPropertyChanged("Platform");
                item.OnPropertyChanged("DefaultIcon");
                item.OnPropertyChanged("DefaultImage");
            }
        }

        private void Database_PlatformsCollectionChanged(object sender, PlatformsCollectionChangedEventArgs args)
        {
            platformsCache = database.PlatformsCollection.FindAll().ToList();
        }

        private void Database_GameUpdated(object sender, GameUpdatedEventArgs args)
        {
            foreach (var update in args.UpdatedGames)
            {
                if (update.OldData.Categories.IsListEqual(update.NewData.Categories))
                {
                    var existingItem = Items.FirstOrDefault(a => a.Game.Id == update.NewData.Id);
                    if (existingItem != null)
                    {
                        if (update.NewData.PlatformId != update.OldData.PlatformId)
                        {
                            existingItem.Platform = new PlatformView(update.NewData.PlatformId, GetPlatformFromCache(update.NewData.PlatformId));
                        }

                        update.NewData.CopyProperties(existingItem.Game, true);
                    }
                    else
                    {
                        logger.Warn("Receivied update for unknown game id " + update.NewData.Id);
                    }
                }
                else
                {
                    Database_GamesCollectionChanged(this, new GamesCollectionChangedEventArgs(
                        new List<IGame>() { update.NewData },
                        new List<IGame>() { update.NewData }));
                }
            }
        }

        private void Database_GamesCollectionChanged(object sender, GamesCollectionChangedEventArgs args)
        {
            foreach (var game in args.RemovedGames)
            {
                foreach (var item in Items.ToList())
                {
                    if (item.Game.Id == game.Id)
                    {
                        Items.Remove(item);
                    }
                }
            }

            foreach (var game in args.AddedGames)
            {
                switch (ViewType)
                {
                    case GamesViewType.Standard:
                        Items.Add(new GameViewEntry(game, string.Empty, GetPlatformFromCache(game.PlatformId)));
                        break;

                    case GamesViewType.CategoryGrouped:
                        if (game.Categories == null || game.Categories.Count == 0)
                        {
                            Items.Add(new GameViewEntry(game, string.Empty, GetPlatformFromCache(game.PlatformId)));
                        }
                        else
                        {
                            Items.AddRange(game.Categories.Select(a => new GameViewEntry(game, a, GetPlatformFromCache(game.PlatformId))));
                        }
                        break;
                }
            }
        }
    }
}
