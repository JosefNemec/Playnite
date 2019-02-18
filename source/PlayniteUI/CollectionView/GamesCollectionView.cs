using Playnite;
using Playnite.Database;
using Playnite.SDK.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Playnite.SDK;
using Playnite.Settings;
using Playnite.SDK.Plugins;
using Playnite.API;
using Playnite.Plugins;

namespace PlayniteUI
{
    public enum GamesViewType
    {
        Standard,
        ListGrouped
    }

    public class GamesCollectionView : ObservableObject, IDisposable
    {
        private static ILogger logger = LogManager.GetLogger();        
        private ExtensionFactory extensions;
        private ViewSettings viewSettings;

        public GameDatabase Database { get; private set; }

        public bool IsFullscreen
        {
            get; private set;
        }            

        private ListCollectionView collectionView;
        public ListCollectionView CollectionView
        {
            get => collectionView;
            private set
            {
                collectionView = value;
                OnPropertyChanged();
            }
        }

        public PlayniteSettings Settings
        {
            get;
            private set;
        }

        private GroupOrder? currentGrouping = null;

        private GamesViewType? viewType = null;
        public GamesViewType? ViewType
        {
            get => viewType;
            set
            {
                SetViewType(value);
                viewType = value;
            }
        }

        public RangeObservableCollection<GameViewEntry> Items
        {
            get; set;
        }

        public GamesCollectionView(GameDatabase database, PlayniteSettings settings, bool fullScreen, ExtensionFactory extensions)
        {
            IsFullscreen = fullScreen;
            this.Database = database;
            this.extensions = extensions;
            database.Games.ItemCollectionChanged += Database_GamesCollectionChanged;
            database.Games.ItemUpdated += Database_GameUpdated;
            database.Platforms.ItemUpdated += Database_PlatformUpdated;
            database.Genres.ItemUpdated += Genres_ItemUpdated;
            database.Categories.ItemUpdated += Categories_ItemUpdated;
            database.AgeRatings.ItemUpdated += AgeRatings_ItemUpdated;
            database.Companies.ItemUpdated += Companies_ItemUpdated;
            database.Regions.ItemUpdated += Regions_ItemUpdated;
            database.Series.ItemUpdated += Series_ItemUpdated;
            database.Sources.ItemUpdated += Sources_ItemUpdated;
            database.Tags.ItemUpdated += Tags_ItemUpdated;

            Items = new RangeObservableCollection<GameViewEntry>();
            Settings = settings;
            if (IsFullscreen)
            {
                Settings.FullscreenViewSettings.PropertyChanged += Settings_PropertyChanged;
                Settings.FullScreenFilterSettings.FilterChanged += FilterSettings_FilterChanged;
            }
            else
            {
                Settings.ViewSettings.PropertyChanged += Settings_PropertyChanged;
                Settings.FilterSettings.FilterChanged += FilterSettings_FilterChanged;
            }

            viewSettings = IsFullscreen ? Settings.FullscreenViewSettings : Settings.ViewSettings;

            CollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(Items);
            CollectionView.Filter = Filter;
            SetViewConfiguration();
        }

        public void Dispose()
        {
            Database.Games.ItemCollectionChanged -= Database_GamesCollectionChanged;
            Database.Games.ItemUpdated -= Database_GameUpdated;
            Database.Platforms.ItemUpdated -= Database_PlatformUpdated;
            Database.Platforms.ItemUpdated -= Database_PlatformUpdated;
            Database.Genres.ItemUpdated -= Genres_ItemUpdated;
            Database.Categories.ItemUpdated -= Categories_ItemUpdated;
            Database.AgeRatings.ItemUpdated -= AgeRatings_ItemUpdated;
            Database.Companies.ItemUpdated -= Companies_ItemUpdated;
            Database.Regions.ItemUpdated -= Regions_ItemUpdated;
            Database.Series.ItemUpdated -= Series_ItemUpdated;
            Database.Sources.ItemUpdated -= Sources_ItemUpdated;
            Database.Tags.ItemUpdated -= Tags_ItemUpdated;

            Settings.PropertyChanged -= Settings_PropertyChanged;
            if (IsFullscreen)
            {
                Settings.FullscreenViewSettings.PropertyChanged -= Settings_PropertyChanged;
                Settings.FullScreenFilterSettings.FilterChanged -= FilterSettings_FilterChanged;
            }
            else
            {
                Settings.ViewSettings.PropertyChanged -= Settings_PropertyChanged;
                Settings.FilterSettings.FilterChanged -= FilterSettings_FilterChanged;
            }

            Items.Clear();
            Items = null;
        }

        private bool IsFilterMatching(FilterItemProperites filter, List<Guid> idData, IEnumerable<DatabaseObject> objectData)
        {
            if (filter == null || !filter.IsSet)
            {
                return true;
            }

            if (objectData == null && (filter == null || !filter.IsSet))
            {
                return true;
            }

            if (objectData == null && filter?.IsSet == true)
            {
                return false;
            }

            if (!filter.Text.IsNullOrEmpty())
            {
                if (filter.Text.Contains(Constants.ListSeparator))
                {
                    return filter.Texts.IntersectsPartiallyWith(objectData?.Select(a => a.Name));
                }
                else
                {
                    return objectData.Any(a => a.Name.Contains(filter.Text, StringComparison.InvariantCultureIgnoreCase));
                }
            }
            else if (filter.Ids.HasItems())
            {
                if (!idData.HasItems())
                {
                    return false;
                }
                else
                {
                    return filter.Ids.Intersect(idData).Any();
                }
            }
            else
            {
                return true;
            }
        }

        private bool IsFilterMatchingSingle(FilterItemProperites filter, Guid idData, DatabaseObject objectData)
        {
            if (filter == null || !filter.IsSet)
            {
                return true;
            }

            if (objectData == null && (filter == null || !filter.IsSet))
            {
                return true;
            }

            if (objectData == null && filter?.IsSet == true)
            {
                return false;
            }

            if (!filter.Text.IsNullOrEmpty())
            {
                if (filter.Text.Contains(Constants.ListSeparator))
                {
                    return filter.Texts.ContainsStringPartial(objectData.Name);
                }
                else
                {
                    return objectData.Name.Contains(filter.Text, StringComparison.InvariantCultureIgnoreCase);
                }
            }
            else if (filter.Ids.HasItems())
            {
                if (idData == null)
                {
                    return false;
                }
                else
                {
                    return filter.Ids.Contains(idData);
                }
            }
            else
            {
                return true;
            }
        }
 
        private bool Filter(object item)
        {
            var entry = (GameViewEntry)item;
            var game = entry.Game;
            var filterSettings = IsFullscreen ? Settings.FullScreenFilterSettings : Settings.FilterSettings;

            if (!filterSettings.Active)
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
            if ((filterSettings.IsInstalled && filterSettings.IsUnInstalled) ||
                (!filterSettings.IsInstalled && !filterSettings.IsUnInstalled))
            {
                installedResult = true;
            }
            else
            {
                if (filterSettings.IsInstalled && game.IsInstalled)
                {
                    installedResult = true;
                }
                else if (filterSettings.IsUnInstalled && !game.IsInstalled)
                {
                    installedResult = true;
                }
            }

            if (!installedResult)
            {
                return false;
            }

            // ------------------ Hidden
            bool hiddenResult = true;
            if (filterSettings.Hidden && game.Hidden)
            {
                hiddenResult = true;
            }
            else if (!filterSettings.Hidden && game.Hidden)
            {
                return false;
            }
            else if (filterSettings.Hidden && !game.Hidden)
            {
                return false;
            }

            if (!hiddenResult)
            {
                return false;
            }

            // ------------------ Favorite
            bool favoriteResult = false;
            if (filterSettings.Favorite && game.Favorite)
            {
                favoriteResult = true;
            }
            else if (!filterSettings.Favorite)
            {
                favoriteResult = true;
            }

            if (!favoriteResult)
            {
                return false;
            }

            // ------------------ Providers
            bool librariesFilter = false;
            if (filterSettings.Library?.IsSet == true)
            {
                var libInter = filterSettings.Library.Ids?.Intersect(new List<Guid> { game.PluginId });
                librariesFilter = libInter?.Any() == true;
            }
            else
            {
                librariesFilter = true;
            }

            if (!librariesFilter)
            {
                return false;
            }

            // ------------------ Name filter
            bool nameResult = false;
            if (string.IsNullOrEmpty(filterSettings.Name))
            {
                nameResult = true;
            }
            else
            {
                if (string.IsNullOrEmpty(game.Name))
                {
                    return false;
                }
                else
                {
                    nameResult = (game.Name.IndexOf(filterSettings.Name, StringComparison.OrdinalIgnoreCase) >= 0);
                }
            }

            if (!nameResult)
            {
                return false;
            }

            // ------------------ Release Date
            bool releaseDateResult = false;
            if (string.IsNullOrEmpty(filterSettings.ReleaseDate))
            {
                releaseDateResult = true;
            }
            else
            {
                if (game.ReleaseDate == null)
                {
                    return false;
                }
                else
                {
                    releaseDateResult = game.ReleaseDate.Value.ToString(Constants.DateUiFormat).IndexOf(filterSettings.ReleaseDate, StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }

            if (!releaseDateResult)
            {
                return false;
            }

            // ------------------ Series filter
            if (!IsFilterMatchingSingle(filterSettings.Series, game.SeriesId, game.Series))
            {
                return false;
            }

            // ------------------ Region filter
            if (!IsFilterMatchingSingle(filterSettings.Region, game.RegionId, game.Region))
            {
                return false;
            }

            // ------------------ Source filter
            if (!IsFilterMatchingSingle(filterSettings.Source, game.SourceId, game.Source))
            {
                return false;
            }

            // ------------------ AgeRating filter
            if (!IsFilterMatchingSingle(filterSettings.AgeRating, game.AgeRatingId, game.AgeRating))
            {
                return false;
            }

            //// ------------------ Genre
            if (!IsFilterMatching(filterSettings.Genre, game.GenreIds, game.Genres))
            {
                return false;
            }

            //// ------------------ Platform
            if (!IsFilterMatchingSingle(filterSettings.Platform, game.PlatformId, game.Platform))
            {
                return false;
            }

            // ------------------ Publisher
            if (!IsFilterMatching(filterSettings.Publisher, game.PublisherIds, game.Publishers))
            {
                return false;
            }

            // ------------------ Developer
            if (!IsFilterMatching(filterSettings.Developer, game.DeveloperIds, game.Developers))
            {
                return false;
            }

            // ------------------ Category
            if (!IsFilterMatching(filterSettings.Category, game.CategoryIds, game.Categories))
            {
                return false;
            }

            // ------------------ Tags
            if (!IsFilterMatching(filterSettings.Tag, game.TagIds, game.Tags))
            {
                return false;
            }

            return true;
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((new string[] { nameof(ViewSettings.SortingOrder), nameof(ViewSettings.GroupingOrder), nameof(ViewSettings.SortingOrderDirection) }).Contains(e.PropertyName))
            {
                logger.Debug("Updating collection view settings.");
                using (CollectionView.DeferRefresh())
                {
                    CollectionView.SortDescriptions.Clear();               
                    CollectionView.GroupDescriptions.Clear();
                    SetViewDescriptions();
                }
            }
        }

        private void FilterSettings_FilterChanged(object sender, FilterChangedEventArgs e)
        {
            logger.Debug("Refreshing collection view filter.");
            CollectionView.Refresh();
        }

        private void SetViewDescriptions()
        {         
            var sortDirection = viewSettings.SortingOrderDirection == SortOrderDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending;

            if (IsFullscreen)
            {
                ViewType = GamesViewType.Standard;
            }
            else
            {
                switch (viewSettings.GroupingOrder)
                {
                    case GroupOrder.None:
                        ViewType = GamesViewType.Standard;
                        break;
                    case GroupOrder.Provider:
                        ViewType = GamesViewType.Standard;
                        break;
                    case GroupOrder.Category:
                        ViewType = GamesViewType.ListGrouped;
                        break;
                    case GroupOrder.Genre:
                        ViewType = GamesViewType.ListGrouped;
                        break;
                    case GroupOrder.Developer:
                        ViewType = GamesViewType.ListGrouped;
                        break;
                    case GroupOrder.Publisher:
                        ViewType = GamesViewType.ListGrouped;
                        break;
                    case GroupOrder.Tag:
                        ViewType = GamesViewType.ListGrouped;
                        break;
                    case GroupOrder.Platform:
                        ViewType = GamesViewType.Standard;
                        break;
                    case GroupOrder.Series:
                        ViewType = GamesViewType.Standard;
                        break;
                    case GroupOrder.AgeRating:
                        ViewType = GamesViewType.Standard;
                        break;
                    case GroupOrder.Region:
                        ViewType = GamesViewType.Standard;
                        break;
                    case GroupOrder.Source:
                        ViewType = GamesViewType.Standard;
                        break;
                    case GroupOrder.ReleaseYear:
                        ViewType = GamesViewType.Standard;
                        break;
                    default:
                        throw new Exception("Uknown GroupingOrder");
                }

                currentGrouping = viewSettings.GroupingOrder;
            }

            if (viewSettings.SortingOrder == SortOrder.Name)
            {
                sortDirection = sortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            }
       
            CollectionView.SortDescriptions.Add(new SortDescription(viewSettings.SortingOrder.ToString(), sortDirection));
            if (viewSettings.SortingOrder != SortOrder.Name)
            {
                CollectionView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            }

            if (viewSettings.GroupingOrder != GroupOrder.None)
            {
                CollectionView.GroupDescriptions.Add(new PropertyGroupDescription(groupFields[viewSettings.GroupingOrder]));
                if (CollectionView.SortDescriptions.First().PropertyName != groupFields[viewSettings.GroupingOrder])
                {
                    CollectionView.SortDescriptions.Insert(0, new SortDescription(groupFields[viewSettings.GroupingOrder], ListSortDirection.Ascending));
                }
            }
        }

        private void SetViewConfiguration()
        {
            using (CollectionView.DeferRefresh())
            {
                SetViewDescriptions();
            };
        }

        private Dictionary<GroupOrder, string> groupFields = new Dictionary<GroupOrder, string>()
        {
            { GroupOrder.Provider, nameof(GameViewEntry.Provider) },
            { GroupOrder.Category, nameof(GameViewEntry.Category) },
            { GroupOrder.Genre, nameof(GameViewEntry.Genre) },
            { GroupOrder.Developer, nameof(GameViewEntry.Developer) },
            { GroupOrder.Publisher, nameof(GameViewEntry.Publisher) },
            { GroupOrder.Tag, nameof(GameViewEntry.Tag) },
            { GroupOrder.Platform, nameof(GameViewEntry.Platform) },
            { GroupOrder.Series, nameof(GameViewEntry.Series) },
            { GroupOrder.AgeRating, nameof(GameViewEntry.AgeRating) },
            { GroupOrder.Region, nameof(GameViewEntry.Region) },
            { GroupOrder.Source, nameof(GameViewEntry.Source) },
            { GroupOrder.ReleaseYear, nameof(GameViewEntry.ReleaseYear) }
        };

        private Dictionary<GroupOrder, Type> groupTypes = new Dictionary<GroupOrder, Type>()
        {            
            { GroupOrder.Category, typeof(Category) },
            { GroupOrder.Genre, typeof(Genre) },
            { GroupOrder.Developer, typeof(Developer) },
            { GroupOrder.Publisher, typeof(Publisher) },
            { GroupOrder.Tag, typeof(Tag) }
        };

        private Guid GetGroupingId(GroupOrder orderField, Game sourceGame)
        {
            switch (orderField)
            {
                case GroupOrder.AgeRating:
                    return sourceGame.AgeRatingId;
                case GroupOrder.Platform:
                    return sourceGame.PlatformId;
                case GroupOrder.Region:
                    return sourceGame.RegionId;
                case GroupOrder.Series:
                    return sourceGame.SeriesId;
                case GroupOrder.Source:
                    return sourceGame.SourceId;
                case GroupOrder.None:
                    return Guid.Empty;
                default:
                    throw new Exception("Wrong grouping configuration.");
            }
        }

        private IEnumerable<Guid> GetGroupingIds(GroupOrder orderField, Game sourceGame)
        {
            switch (orderField)
            {
                case GroupOrder.Category:
                    return sourceGame.CategoryIds;
                case GroupOrder.Genre:
                    return sourceGame.GenreIds;
                case GroupOrder.Developer:
                    return sourceGame.DeveloperIds;
                case GroupOrder.Publisher:
                    return sourceGame.PublisherIds;
                case GroupOrder.Tag:
                    return sourceGame.TagIds;
                case GroupOrder.None:
                    return null;
                default:
                    throw new Exception("Wrong grouping configuration.");
            }
        }

        public void SetViewType(GamesViewType? viewType)
        {
            if (currentGrouping == viewSettings.GroupingOrder)
            {
                return;
            }

            if (IsFullscreen)
            {
                Items.Clear();
                Items.AddRange(Database.Games.Select(x => new GameViewEntry(x, GetLibraryPlugin(x))));
            }
            else
            {
                Items.Clear();

                switch (viewType)
                {
                    case GamesViewType.Standard:
                        Items.Clear();
                        Items.AddRange(Database.Games.Select(x => new GameViewEntry(x, GetLibraryPlugin(x))));
                        break;

                    case GamesViewType.ListGrouped:
                        Items.Clear();
                        Items.AddRange(Database.Games.SelectMany(x =>
                        {
                            var ids = GetGroupingIds(viewSettings.GroupingOrder, x);
                            if (ids?.Any() == true)
                            {
                                return ids.Select(c =>
                                {
                                    return new GameViewEntry(x, GetLibraryPlugin(x), groupTypes[viewSettings.GroupingOrder], c);
                                });
                            }
                            else
                            {
                                return new List<GameViewEntry>()
                                {
                                    new GameViewEntry(x, GetLibraryPlugin(x))
                                };
                            }
                        }));

                        break;
                }
            }

            this.viewType = viewType;
        }

        private ILibraryPlugin GetLibraryPlugin(Game game)
        {
            if (game.PluginId != Guid.Empty && extensions.LibraryPlugins.TryGetValue(game.PluginId, out var plugin))
            {
                return plugin.Plugin;
            }

            return null;
        }

        private void Database_PlatformUpdated(object sender, ItemUpdatedEventArgs<Platform> e)
        {
            DoGroupDbObjectsUpdate(
               GroupOrder.Platform, e,
               (a, b) => a.PlatformId != Guid.Empty && b.Contains(a.PlatformId));
        }

        private void Genres_ItemUpdated(object sender, ItemUpdatedEventArgs<Genre> e)
        {
            DoGroupDbObjectsUpdate(
                GroupOrder.Genre, e,
                (a, b) => a.GenreIds?.Any() == true && b.Intersect(a.GenreIds).Any(),
                nameof(Game.Genres));
        }

        private void Tags_ItemUpdated(object sender, ItemUpdatedEventArgs<Tag> e)
        {
            DoGroupDbObjectsUpdate(
                GroupOrder.Tag, e,
                (a, b) => a.TagIds?.Any() == true && b.Intersect(a.TagIds).Any(),
                nameof(Game.Tags));
        }

        private void Sources_ItemUpdated(object sender, ItemUpdatedEventArgs<GameSource> e)
        {
            DoGroupDbObjectsUpdate(
               GroupOrder.Source, e,
               (a, b) => a.SourceId != Guid.Empty && b.Contains(a.SourceId));
        }

        private void Series_ItemUpdated(object sender, ItemUpdatedEventArgs<Series> e)
        {
            DoGroupDbObjectsUpdate(
               GroupOrder.Series, e,
               (a, b) => a.SeriesId != Guid.Empty && b.Contains(a.SeriesId));
        }

        private void Regions_ItemUpdated(object sender, ItemUpdatedEventArgs<Region> e)
        {
            DoGroupDbObjectsUpdate(
               GroupOrder.Region, e,
               (a, b) => a.RegionId != Guid.Empty && b.Contains(a.RegionId));
        }

        private void Companies_ItemUpdated(object sender, ItemUpdatedEventArgs<Company> e)
        {
            DoGroupDbObjectsUpdate(
                GroupOrder.Developer, e,
                (a, b) => a.DeveloperIds?.Any() == true && b.Intersect(a.DeveloperIds).Any(),
                nameof(Game.Developers));          

            DoGroupDbObjectsUpdate(
                GroupOrder.Publisher, e,
                (a, b) => a.PublisherIds?.Any() == true && b.Intersect(a.PublisherIds).Any(),
                nameof(Game.Publishers));
        }

        private void AgeRatings_ItemUpdated(object sender, ItemUpdatedEventArgs<AgeRating> e)
        {
            DoGroupDbObjectsUpdate(
               GroupOrder.AgeRating, e,
               (a, b) => a.AgeRatingId != Guid.Empty && b.Contains(a.AgeRatingId));
        }

        private void Categories_ItemUpdated(object sender, ItemUpdatedEventArgs<Category> e)
        {
            DoGroupDbObjectsUpdate(
                GroupOrder.Category, e,
                (a, b) => a.CategoryIds?.Any() == true && b.Intersect(a.CategoryIds).Any(),
                nameof(Game.Categories));
        }

        private void DoGroupDbObjectsUpdate<TItem>(
            GroupOrder order,
            ItemUpdatedEventArgs<TItem> updatedItems,
            Func<GameViewEntry, List<Guid>, bool> condition,
            string extraPropNotify = null) where TItem : DatabaseObject
        {
            var updatedIds = new List<Guid>(updatedItems.UpdatedItems.Select(a => a.NewData.Id));
            var doUpdate = false;
            foreach (var item in Items.Where(a => condition(a, updatedIds)))
            {
                doUpdate = true;
                item.OnPropertyChanged(groupFields[order]);
                if (!extraPropNotify.IsNullOrEmpty())
                {
                    item.OnPropertyChanged(extraPropNotify);
                }
            }

            if (doUpdate && viewSettings.GroupingOrder == order)
            {
                FilterSettings_FilterChanged(this, null);
            }
        }

        private void Database_GameUpdated(object sender, ItemUpdatedEventArgs<Game> args)
        {
            var refreshList = new List<Game>();
            foreach (var update in args.UpdatedItems)
            {
                var existingItem = Items.FirstOrDefault(a => a.Game.Id == update.NewData.Id);
                if (existingItem != null)
                {
                    var fullRefresh = false;

                    if (ViewType == GamesViewType.Standard && !GetGroupingId(viewSettings.GroupingOrder, update.OldData).Equals(GetGroupingId(viewSettings.GroupingOrder, update.NewData)))
                    {
                        fullRefresh = true;
                    }

                    if (ViewType == GamesViewType.ListGrouped && !GetGroupingIds(viewSettings.GroupingOrder, update.OldData).IsListEqual(GetGroupingIds(viewSettings.GroupingOrder, update.NewData)))
                    {
                        fullRefresh = true;
                    }

                    if (fullRefresh)
                    {
                        refreshList.Add(update.NewData);
                    }
                    else
                    {
                        // Forces CollectionView to re-sort items without full list refresh.
                        Items.OnItemMoved(existingItem, 0, 0);
                    }
                }
            }

            if (refreshList.Any())
            {
                Database_GamesCollectionChanged(this, new ItemCollectionChangedEventArgs<Game>(refreshList, refreshList));
            }
        }

        private void Database_GamesCollectionChanged(object sender, ItemCollectionChangedEventArgs<Game> args)
        {
            // DO NOT use *Range methods for "Items" object.
            // It can throw weird exceptions in virtualization panel, directly in WPF (without known fix from MS).
            // https://github.com/JosefNemec/Playnite/issues/796

            if (args.RemovedItems.Count > 0)
            {
                var removeIds = new HashSet<Guid>(args.RemovedItems.Select(a => a.Id));
                var toRemove = Items.Where(a => removeIds.Contains(a.Id))?.ToList();
                if (toRemove != null)
                {
                    foreach (var item in toRemove)
                    {
                        Items.Remove(item);
                    }
                }
            }

            var addList = new List<GameViewEntry>();
            foreach (var game in args.AddedItems)
            {
                if (IsFullscreen)
                {
                    addList.Add(new GameViewEntry(game, GetLibraryPlugin(game)));
                }
                else
                {
                    switch (ViewType)
                    {
                        case GamesViewType.Standard:
                            addList.Add(new GameViewEntry(game, GetLibraryPlugin(game)));
                            break;

                        case GamesViewType.ListGrouped:
                           
                            var ids = GetGroupingIds(viewSettings.GroupingOrder, game);
                            if (ids?.Any() == true)
                            {
                                addList.AddRange(ids.Select(c =>
                                {
                                    return new GameViewEntry(game, GetLibraryPlugin(game), groupTypes[viewSettings.GroupingOrder], c);
                                }));
                            }
                            else
                            {
                                addList.Add(new GameViewEntry(game, GetLibraryPlugin(game)));
                            }

                            break;
                    }
                }
            }

            if (addList.Count > 0)
            {
                foreach (var item in addList)
                {
                    Items.Add(item);
                }
            }
        }
    }
}
