using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Playnite.DesktopApp
{
    public class DesktopCollectionView : BaseCollectionView
    {
        public enum GamesViewType
        {
            Standard,
            ListGrouped
        }

        private PlayniteSettings settings;
        private ViewSettings viewSettings;
        private GroupableField? currentGrouping = null;
        private GamesViewType? loadedViewType = null;

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
            { GroupableField.PlayTime, nameof(GamesCollectionViewEntry.PlaytimeCategory) },
            { GroupableField.Feature, nameof(GamesCollectionViewEntry.Feature) },
            { GroupableField.InstallationStatus, nameof(GamesCollectionViewEntry.InstallationState) }
        };

        private Dictionary<GroupableField, Type> groupTypes = new Dictionary<GroupableField, Type>()
        {
            { GroupableField.Category, typeof(Category) },
            { GroupableField.Genre, typeof(Genre) },
            { GroupableField.Developer, typeof(Developer) },
            { GroupableField.Publisher, typeof(Publisher) },
            { GroupableField.Tag, typeof(Tag) },
            { GroupableField.Feature, typeof(GameFeature) }
        };

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

        public DesktopCollectionView(
            IGameDatabase database,
            PlayniteSettings settings,
            ExtensionFactory extensions) : base(database, extensions, settings.FilterSettings)
        {
            this.settings = settings;
            Database.Games.ItemCollectionChanged += Database_GamesCollectionChanged;
            Database.Games.ItemUpdated += Database_GameUpdated;
            Database.Platforms.ItemUpdated += Database_PlatformUpdated;
            Database.Genres.ItemUpdated += Genres_ItemUpdated;
            Database.Categories.ItemUpdated += Categories_ItemUpdated;
            Database.AgeRatings.ItemUpdated += AgeRatings_ItemUpdated;
            Database.Companies.ItemUpdated += Companies_ItemUpdated;
            Database.Regions.ItemUpdated += Regions_ItemUpdated;
            Database.Series.ItemUpdated += Series_ItemUpdated;
            Database.Sources.ItemUpdated += Sources_ItemUpdated;
            Database.Tags.ItemUpdated += Tags_ItemUpdated;
            Database.Features.ItemUpdated += Features_ItemUpdated;
            viewSettings = settings.ViewSettings;
            viewSettings.PropertyChanged += ViewSettings_PropertyChanged;
            using (CollectionView.DeferRefresh())
            {
                SetViewDescriptions();
            };
        }

        public override void Dispose()
        {
            ClearItems();
            base.Dispose();
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
            Database.Features.ItemUpdated -= Features_ItemUpdated;
            viewSettings.PropertyChanged -= ViewSettings_PropertyChanged;
        }

        private void ViewSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((new string[]
            {
                nameof(ViewSettings.SortingOrder),
                nameof(ViewSettings.GroupingOrder),
                nameof(ViewSettings.SortingOrderDirection),
                nameof(ViewSettings.GamesViewType)
            }).Contains(e.PropertyName))
            {
                Logger.Debug("Updating collection view settings.");
                using (CollectionView.DeferRefresh())
                {
                    CollectionView.SortDescriptions.Clear();
                    CollectionView.GroupDescriptions.Clear();
                    SetViewDescriptions();
                }
            }
        }

        private void SetViewDescriptions()
        {
            var sortDirection = viewSettings.SortingOrderDirection == SortOrderDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending;
            if (viewSettings.GamesViewType == Playnite.ViewType.Grid)
            {
                ViewType = GamesViewType.Standard;
            }
            else
            {
                switch (viewSettings.GroupingOrder)
                {
                    case GroupableField.Category:
                    case GroupableField.Genre:
                    case GroupableField.Developer:
                    case GroupableField.Publisher:
                    case GroupableField.Tag:
                    case GroupableField.Feature:
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
                    case GroupableField.InstallationStatus:
                        ViewType = GamesViewType.Standard;
                        break;
                    default:
                        throw new Exception("Uknown GroupingOrder");
                }
            }

            currentGrouping = viewSettings.GroupingOrder;
            CollectionView.SortDescriptions.Add(new SortDescription(viewSettings.SortingOrder.ToString(), sortDirection));
            if (viewSettings.SortingOrder != SortOrder.Name)
            {
                CollectionView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            }

            if (viewSettings.GroupingOrder != GroupableField.None)
            {
                if (viewSettings.GamesViewType != Playnite.ViewType.Grid)
                {
                    CollectionView.GroupDescriptions.Add(new PropertyGroupDescription(groupFields[viewSettings.GroupingOrder]));
                }

                if (CollectionView.SortDescriptions.First().PropertyName != groupFields[viewSettings.GroupingOrder])
                {
                    CollectionView.SortDescriptions.Insert(0, new SortDescription(groupFields[viewSettings.GroupingOrder], ListSortDirection.Ascending));
                }
            }
        }

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
                case GroupableField.Feature:
                    return sourceGame.FeatureIds;
                case GroupableField.None:
                    return null;
                default:
                    throw new Exception("Wrong grouping configuration.");
            }
        }

        public void SetViewType(GamesViewType? viewType)
        {
            if (currentGrouping == viewSettings.GroupingOrder && viewType == loadedViewType)
            {
                return;
            }

            ClearItems();
            switch (viewType)
            {
                case GamesViewType.Standard:
                    Items.AddRange(Database.Games.Select(x => new GamesCollectionViewEntry(x, GetLibraryPlugin(x), settings)));
                    break;

                case GamesViewType.ListGrouped:
                    Items.AddRange(Database.Games.SelectMany(x =>
                    {
                        var ids = GetGroupingIds(viewSettings.GroupingOrder, x);
                        if (ids?.Any() == true)
                        {
                            return ids.Select(c =>
                            {
                            return new GamesCollectionViewEntry(x, GetLibraryPlugin(x), groupTypes[viewSettings.GroupingOrder], c, settings);
                            });
                        }
                        else
                        {
                            return new List<GamesCollectionViewEntry>()
                            {
                                new GamesCollectionViewEntry(x, GetLibraryPlugin(x), settings)
                            };
                        }
                    }));

                    break;
            }

            this.viewType = viewType;
            loadedViewType = viewType;
        }

        private void ClearItems()
        {
            if (Items.HasItems())
            {
                foreach (var item in Items)
                {
                    item.Dispose();
                }

                Items.Clear();
            }
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

        private void Features_ItemUpdated(object sender, ItemUpdatedEventArgs<GameFeature> e)
        {
            DoGroupDbObjectsUpdate(
                GroupableField.Feature, e,
                (a, b) => a.FeatureIds?.Any() == true && b.Intersect(a.FeatureIds).Any(),
                nameof(Game.Features));
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
                Logger.Debug("Refreshing collection view filter.");
                CollectionView.Refresh();
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
                case GroupableField.Feature:
                    return ViewType == GamesViewType.ListGrouped && !GetGroupingIds(viewSettings.GroupingOrder, oldData).IsListEqual(GetGroupingIds(viewSettings.GroupingOrder, newData));
                case GroupableField.Platform:
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
                case GroupableField.InstallationStatus:
                    return oldData.IsInstalled != newData.IsInstalled;
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
                        try
                        {
                            Items.OnItemMoved(existingItem, 0, 0);
                        }
                        catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                        {
                            // Another weird and rare "out of range" bug in System.Windows.Data.CollectionView.OnCollectionChanged.
                            // No idea why it's happening.
                            Logger.Error(e, "Items.OnItemMoved failed.");
                        }
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
                        item.Dispose();
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
                        addList.Add(new GamesCollectionViewEntry(game, GetLibraryPlugin(game), settings));
                        break;

                    case GamesViewType.ListGrouped:

                        var ids = GetGroupingIds(viewSettings.GroupingOrder, game);
                        if (ids?.Any() == true)
                        {
                            addList.AddRange(ids.Select(c =>
                            {
                                return new GamesCollectionViewEntry(game, GetLibraryPlugin(game), groupTypes[viewSettings.GroupingOrder], c, settings);
                            }));
                        }
                        else
                        {
                            addList.Add(new GamesCollectionViewEntry(game, GetLibraryPlugin(game), settings));
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
