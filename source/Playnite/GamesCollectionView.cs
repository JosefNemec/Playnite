using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Playnite.Database;
using Playnite.SDK.Models;
using Playnite.SDK;
using Playnite.Settings;
using Playnite.Plugins;
using Playnite.SDK.Plugins;

namespace Playnite
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

        private GroupableField? currentGrouping = null;

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

        public RangeObservableCollection<GamesCollectionViewEntry> Items
        {
            get; set;
        }

        public GamesCollectionView(GameDatabase database, PlayniteSettings settings, ExtensionFactory extensions)
        {
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

            Items = new RangeObservableCollection<GamesCollectionViewEntry>();
            Settings = settings;
            Settings.ViewSettings.PropertyChanged += Settings_PropertyChanged;
            Settings.FilterSettings.FilterChanged += FilterSettings_FilterChanged; 
            viewSettings = Settings.ViewSettings;

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
            Settings.ViewSettings.PropertyChanged -= Settings_PropertyChanged;
            Settings.FilterSettings.FilterChanged -= FilterSettings_FilterChanged;

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

            if (!filter.Text.IsNullOrEmpty())
            {
                if (objectData == null)
                {
                    return false;
                }

                if (filter.Text.Contains(Common.Constants.ListSeparator))
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
                if (filter.Ids.Contains(Guid.Empty) && !idData.HasItems())
                {
                    return true;
                }
                else if (!idData.HasItems())
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

            if (!filter.Text.IsNullOrEmpty())
            {
                if (objectData == null)
                {
                    return false;
                }

                if (filter.Text.Contains(Common.Constants.ListSeparator))
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
                if (filter.Ids.Contains(Guid.Empty) && idData == Guid.Empty)
                {
                    return true;
                }
                else if (idData == Guid.Empty)
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

        private bool IsScoreFilterMatching(EnumFilterItemProperites filter, ScoreGroup score)
        {
            if (filter == null)
            {
                return true;
            }

            return filter.Values.Contains((int)score);
        }

        private bool Filter(object item)
        {
            var entry = (GamesCollectionViewEntry)item;
            var game = entry.Game;
            var filterSettings = Settings.FilterSettings;

            if (!filterSettings.IsActive)
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
            if (!filterSettings.Name.IsNullOrEmpty())
            {
                if (game.Name.IsNullOrEmpty())
                {
                    return false;
                }
                else if (game.Name.IndexOf(filterSettings.Name, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return false;
                }
            }

            // ------------------ Release Year       
            if (filterSettings.ReleaseYear?.IsSet == true)
            {
                if (game.ReleaseDate == null && !filterSettings.ReleaseYear.Values.Contains(FilterSettings.MissingFieldString))
                {
                    return false;
                }
                else if (game.ReleaseDate != null && !filterSettings.ReleaseYear.Values.Contains(game.ReleaseYear.ToString()))
                {
                    return false;
                }
            }

            // ------------------ Playtime
            if (filterSettings.PlayTime?.IsSet == true && !filterSettings.PlayTime.Values.Contains((int)game.PlaytimeCategory))
            {
                return false;                     
            }

            // ------------------ Version
                if (!filterSettings.Version.IsNullOrEmpty() && game.Version?.Contains(filterSettings.Version) != true)
            {
                return false;
            }

            // ------------------ Completion Status
            if (filterSettings.CompletionStatus?.IsSet == true && !filterSettings.CompletionStatus.Values.Contains((int)game.CompletionStatus))
            {
                return false;
            }

            // ------------------ Last Activity
            if (filterSettings.LastActivity?.IsSet == true && !filterSettings.LastActivity.Values.Contains((int)game.LastActivitySegment))
            {
                return false;
            }

            // ------------------ Added
            if (filterSettings.Added?.IsSet == true && !filterSettings.Added.Values.Contains((int)game.AddedSegment))
            {
                return false;
            }

            // ------------------ Modified
            if (filterSettings.Modified?.IsSet == true && !filterSettings.Modified.Values.Contains((int)game.ModifiedSegment))
            {
                return false;
            }

            // ------------------ User Score
            if (!IsScoreFilterMatching(filterSettings.UserScore, game.UserScoreGroup))
            {
                return false;
            }

            // ------------------ Community Score
            if (!IsScoreFilterMatching(filterSettings.CommunityScore, game.CommunityScoreGroup))
            {
                return false;
            }

            // ------------------ Critic Score
            if (!IsScoreFilterMatching(filterSettings.CriticScore, game.CriticScoreGroup))
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
            switch (viewSettings.GroupingOrder)
            {
                case GroupableField.Category:
                case GroupableField.Genre:
                case GroupableField.Developer:
                case GroupableField.Publisher:
                case GroupableField.Tag:
                    ViewType = GamesViewType.ListGrouped;
                    break;
                case GroupableField.None:
                case GroupableField.Library:
                case GroupableField.Platform:
                case GroupableField.Series:
                case GroupableField.AgeRating:
                case GroupableField.Region:
                case GroupableField.Source:
                case GroupableField.ReleaseYear:
                case GroupableField.CompletionStatus:
                case GroupableField.UserScore:
                case GroupableField.CriticScore:
                case GroupableField.CommunityScore:
                case GroupableField.LastActivity:
                case GroupableField.Added:
                case GroupableField.Modified:
                case GroupableField.PlayTime:
                    ViewType = GamesViewType.Standard;
                    break;
                default:
                    throw new Exception("Uknown GroupingOrder");
            }

            currentGrouping = viewSettings.GroupingOrder;
            

            if (viewSettings.SortingOrder == SortOrder.Name)
            {
                sortDirection = sortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            }
       
            CollectionView.SortDescriptions.Add(new SortDescription(viewSettings.SortingOrder.ToString(), sortDirection));
            if (viewSettings.SortingOrder != SortOrder.Name)
            {
                CollectionView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            }

            if (viewSettings.GroupingOrder != GroupableField.None)
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

        private Dictionary<GroupableField, string> groupFields = new Dictionary<GroupableField, string>()
        {
            { GroupableField.Library, nameof(GamesCollectionViewEntry.Library) },
            { GroupableField.Category, nameof(GamesCollectionViewEntry.Category) },
            { GroupableField.Genre, nameof(GamesCollectionViewEntry.Genre) },
            { GroupableField.Developer, nameof(GamesCollectionViewEntry.Developer) },
            { GroupableField.Publisher, nameof(GamesCollectionViewEntry.Publisher) },
            { GroupableField.Tag, nameof(GamesCollectionViewEntry.Tag) },
            { GroupableField.Platform, nameof(GamesCollectionViewEntry.Platform) },
            { GroupableField.Series, nameof(GamesCollectionViewEntry.Series) },
            { GroupableField.AgeRating, nameof(GamesCollectionViewEntry.AgeRating) },
            { GroupableField.Region, nameof(GamesCollectionViewEntry.Region) },
            { GroupableField.Source, nameof(GamesCollectionViewEntry.Source) },
            { GroupableField.ReleaseYear, nameof(GamesCollectionViewEntry.ReleaseYear) },
            { GroupableField.CompletionStatus, nameof(GamesCollectionViewEntry.CompletionStatus) },
            { GroupableField.UserScore, nameof(GamesCollectionViewEntry.UserScoreGroup) },
            { GroupableField.CommunityScore, nameof(GamesCollectionViewEntry.CommunityScoreGroup) },
            { GroupableField.CriticScore, nameof(GamesCollectionViewEntry.CriticScoreGroup) },
            { GroupableField.LastActivity, nameof(GamesCollectionViewEntry.LastActivitySegment) },
            { GroupableField.Added, nameof(GamesCollectionViewEntry.AddedSegment) },
            { GroupableField.Modified, nameof(GamesCollectionViewEntry.ModifiedSegment) },
            { GroupableField.PlayTime, nameof(GamesCollectionViewEntry.PlaytimeCategory) }
        };

        private Dictionary<GroupableField, Type> groupTypes = new Dictionary<GroupableField, Type>()
        {            
            { GroupableField.Category, typeof(Category) },
            { GroupableField.Genre, typeof(Genre) },
            { GroupableField.Developer, typeof(Developer) },
            { GroupableField.Publisher, typeof(Publisher) },
            { GroupableField.Tag, typeof(Tag) }
        };

        private Guid GetGroupingId(GroupableField orderField, Game sourceGame)
        {
            switch (orderField)
            {
                case GroupableField.AgeRating:
                    return sourceGame.AgeRatingId;
                case GroupableField.Platform:
                    return sourceGame.PlatformId;
                case GroupableField.Region:
                    return sourceGame.RegionId;
                case GroupableField.Series:
                    return sourceGame.SeriesId;
                case GroupableField.Source:
                    return sourceGame.SourceId;
                case GroupableField.None:
                    return Guid.Empty;
                default:
                    throw new Exception("Wrong grouping configuration.");
            }
        }

        private IEnumerable<Guid> GetGroupingIds(GroupableField orderField, Game sourceGame)
        {
            switch (orderField)
            {
                case GroupableField.Category:
                    return sourceGame.CategoryIds;
                case GroupableField.Genre:
                    return sourceGame.GenreIds;
                case GroupableField.Developer:
                    return sourceGame.DeveloperIds;
                case GroupableField.Publisher:
                    return sourceGame.PublisherIds;
                case GroupableField.Tag:
                    return sourceGame.TagIds;
                case GroupableField.None:
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

            Items.Clear();
            switch (viewType)
            {
                case GamesViewType.Standard:
                    Items.Clear();
                    Items.AddRange(Database.Games.Select(x => new GamesCollectionViewEntry(x, GetLibraryPlugin(x))));
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
                                return new GamesCollectionViewEntry(x, GetLibraryPlugin(x), groupTypes[viewSettings.GroupingOrder], c);
                            });
                        }
                        else
                        {
                            return new List<GamesCollectionViewEntry>()
                            {
                                new GamesCollectionViewEntry(x, GetLibraryPlugin(x))
                            };
                        }
                    }));

                    break;
            }

            this.viewType = viewType;
        }

        private LibraryPlugin GetLibraryPlugin(Game game)
        {
            if (game.PluginId != Guid.Empty && extensions.Plugins.TryGetValue(game.PluginId, out var plugin))
            {
                return (LibraryPlugin)plugin.Plugin;
            }

            return null;
        }

        private void Database_PlatformUpdated(object sender, ItemUpdatedEventArgs<Platform> e)
        {
            DoGroupDbObjectsUpdate(
               GroupableField.Platform, e,
               (a, b) => a.PlatformId != Guid.Empty && b.Contains(a.PlatformId));
        }

        private void Genres_ItemUpdated(object sender, ItemUpdatedEventArgs<Genre> e)
        {
            DoGroupDbObjectsUpdate(
                GroupableField.Genre, e,
                (a, b) => a.GenreIds?.Any() == true && b.Intersect(a.GenreIds).Any(),
                nameof(Game.Genres));
        }

        private void Tags_ItemUpdated(object sender, ItemUpdatedEventArgs<Tag> e)
        {
            DoGroupDbObjectsUpdate(
                GroupableField.Tag, e,
                (a, b) => a.TagIds?.Any() == true && b.Intersect(a.TagIds).Any(),
                nameof(Game.Tags));
        }

        private void Sources_ItemUpdated(object sender, ItemUpdatedEventArgs<GameSource> e)
        {
            DoGroupDbObjectsUpdate(
               GroupableField.Source, e,
               (a, b) => a.SourceId != Guid.Empty && b.Contains(a.SourceId));
        }

        private void Series_ItemUpdated(object sender, ItemUpdatedEventArgs<Series> e)
        {
            DoGroupDbObjectsUpdate(
               GroupableField.Series, e,
               (a, b) => a.SeriesId != Guid.Empty && b.Contains(a.SeriesId));
        }

        private void Regions_ItemUpdated(object sender, ItemUpdatedEventArgs<Region> e)
        {
            DoGroupDbObjectsUpdate(
               GroupableField.Region, e,
               (a, b) => a.RegionId != Guid.Empty && b.Contains(a.RegionId));
        }

        private void Companies_ItemUpdated(object sender, ItemUpdatedEventArgs<Company> e)
        {
            DoGroupDbObjectsUpdate(
                GroupableField.Developer, e,
                (a, b) => a.DeveloperIds?.Any() == true && b.Intersect(a.DeveloperIds).Any(),
                nameof(Game.Developers));          

            DoGroupDbObjectsUpdate(
                GroupableField.Publisher, e,
                (a, b) => a.PublisherIds?.Any() == true && b.Intersect(a.PublisherIds).Any(),
                nameof(Game.Publishers));
        }

        private void AgeRatings_ItemUpdated(object sender, ItemUpdatedEventArgs<AgeRating> e)
        {
            DoGroupDbObjectsUpdate(
               GroupableField.AgeRating, e,
               (a, b) => a.AgeRatingId != Guid.Empty && b.Contains(a.AgeRatingId));
        }

        private void Categories_ItemUpdated(object sender, ItemUpdatedEventArgs<Category> e)
        {
            DoGroupDbObjectsUpdate(
                GroupableField.Category, e,
                (a, b) => a.CategoryIds?.Any() == true && b.Intersect(a.CategoryIds).Any(),
                nameof(Game.Categories));
        }

        private void DoGroupDbObjectsUpdate<TItem>(
            GroupableField order,
            ItemUpdatedEventArgs<TItem> updatedItems,
            Func<GamesCollectionViewEntry, List<Guid>, bool> condition,
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

        private bool GetRelevantDataDiffer(Game oldData, Game newData)
        {
            switch (viewSettings.GroupingOrder)
            {
                case GroupableField.None:
                case GroupableField.Library:
                    return false;
                case GroupableField.Category:
                case GroupableField.Genre:
                case GroupableField.Developer:
                case GroupableField.Publisher:
                case GroupableField.Tag:
                case GroupableField.Platform:
                    return ViewType == GamesViewType.ListGrouped && !GetGroupingIds(viewSettings.GroupingOrder, oldData).IsListEqual(GetGroupingIds(viewSettings.GroupingOrder, newData));                
                case GroupableField.Series:
                case GroupableField.AgeRating:
                case GroupableField.Region:
                case GroupableField.Source:
                    return ViewType == GamesViewType.Standard && !GetGroupingId(viewSettings.GroupingOrder, oldData).Equals(GetGroupingId(viewSettings.GroupingOrder, newData));
                case GroupableField.ReleaseYear:
                    return oldData.ReleaseYear != newData.ReleaseYear;
                case GroupableField.CompletionStatus:
                    return oldData.CompletionStatus != newData.CompletionStatus;
                case GroupableField.UserScore:
                    return oldData.UserScore != newData.UserScore;
                case GroupableField.CriticScore:
                    return oldData.CriticScore != newData.CriticScore;
                case GroupableField.CommunityScore:
                    return oldData.CommunityScore != newData.CommunityScore;
                case GroupableField.LastActivity:
                    return oldData.LastActivity != newData.LastActivity;
                case GroupableField.Added:
                    return oldData.Added != newData.Added;
                case GroupableField.Modified:
                    return oldData.Modified != newData.Modified;
                case GroupableField.PlayTime:
                    return oldData.Playtime != newData.Playtime;
                default:
                    throw new Exception("Uknown GroupableField");
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
                    if (GetRelevantDataDiffer(update.OldData, update.NewData))
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

            var addList = new List<GamesCollectionViewEntry>();
            foreach (var game in args.AddedItems)
            {  
                switch (ViewType)
                {
                    case GamesViewType.Standard:
                        addList.Add(new GamesCollectionViewEntry(game, GetLibraryPlugin(game)));
                        break;

                    case GamesViewType.ListGrouped:
                           
                        var ids = GetGroupingIds(viewSettings.GroupingOrder, game);
                        if (ids?.Any() == true)
                        {
                            addList.AddRange(ids.Select(c =>
                            {
                                return new GamesCollectionViewEntry(game, GetLibraryPlugin(game), groupTypes[viewSettings.GroupingOrder], c);
                            }));
                        }
                        else
                        {
                            addList.Add(new GamesCollectionViewEntry(game, GetLibraryPlugin(game)));
                        }

                        break;
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
