using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Playnite.Database
{
    public class DatabaseFilter : ObservableObject
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static object syncLockYears = new object();
        private static object syncLockGenres = new object();
        private static object syncLockPlatforms = new object();
        private static object syncLockAges = new object();
        private static object syncLockCategories = new object();
        private static object syncLockPublishers = new object();
        private static object syncLockDevelopers = new object();
        private static object syncLockRegions = new object();
        private static object syncLockSeries = new object();
        private static object syncLockSources = new object();
        private static object syncLockTags = new object();
        private static object syncLockFeatures = new object();
        private readonly SynchronizationContext context;
        private readonly GameDatabase database;
        private readonly FilterSettings filter;
        private readonly PlayniteSettings settings;

        private bool missedDbUpdate = false;
        private readonly List<GameDatabaseCollection> missedCollection = new List<GameDatabaseCollection>(Enum.GetValues(typeof(GameDatabaseCollection)).Length);
        private bool ignoreDatabaseUpdates = false;
        public bool IgnoreDatabaseUpdates
        {
            get => ignoreDatabaseUpdates;
            set
            {
                ignoreDatabaseUpdates = value;
                if (value == false && missedDbUpdate)
                {
                    UpdateAllCollections(missedCollection);
                }

                missedDbUpdate = false;
                missedCollection.Clear();
            }
        }

        #region Filter lists

        public SelectableIdItemList<LibraryPlugin> Libraries { get; set; }

        private SelectableDbItemList genres;
        public SelectableDbItemList Genres
        {
            get => genres;
            private set
            {
                genres = value;
                OnPropertyChanged();
            }
        }

        private SelectableDbItemList platforms;
        public SelectableDbItemList Platforms
        {
            get => platforms;
            private set
            {
                platforms = value;
                OnPropertyChanged();
            }
        }

        private SelectableDbItemList ageRatings;
        public SelectableDbItemList AgeRatings
        {
            get => ageRatings;
            private set
            {
                ageRatings = value;
                OnPropertyChanged();
            }
        }

        private SelectableDbItemList categories;
        public SelectableDbItemList Categories
        {
            get => categories;
            private set
            {
                categories = value;
                OnPropertyChanged();
            }
        }

        private SelectableDbItemList publishers;
        public SelectableDbItemList Publishers
        {
            get => publishers;
            private set
            {
                publishers = value;
                OnPropertyChanged();
            }
        }

        private SelectableDbItemList developers;
        public SelectableDbItemList Developers
        {
            get => developers;
            private set
            {
                developers = value;
                OnPropertyChanged();
            }
        }

        private SelectableDbItemList regions;
        public SelectableDbItemList Regions
        {
            get => regions;
            private set
            {
                regions = value;
                OnPropertyChanged();
            }
        }

        private SelectableDbItemList series;
        public SelectableDbItemList Series
        {
            get => series;
            private set
            {
                series = value;
                OnPropertyChanged();
            }
        }

        private SelectableDbItemList sources;
        public SelectableDbItemList Sources
        {
            get => sources;
            private set
            {
                sources = value;
                OnPropertyChanged();
            }
        }

        private SelectableDbItemList tags;
        public SelectableDbItemList Tags
        {
            get => tags;
            private set
            {
                tags = value;
                OnPropertyChanged();
            }
        }

        private SelectableStringList releaseYears;
        public SelectableStringList ReleaseYears
        {
            get => releaseYears;
            private set
            {
                releaseYears = value;
                OnPropertyChanged();
            }
        }

        private SelectableDbItemList features;
        public SelectableDbItemList Features
        {
            get => features;
            private set
            {
                features = value;
                OnPropertyChanged();
            }
        }

        #endregion Filter lists

        public DatabaseFilter(GameDatabase database, ExtensionFactory extensions, PlayniteSettings settings, FilterSettings filter)
        {
            this.context = SynchronizationContext.Current;
            this.database = database;
            this.settings = settings;
            this.filter = filter;
            this.settings.PropertyChanged += Settings_PropertyChanged;

            if (database.IsOpen)
            {
                LoadFilterCollection();
            }
            else
            {
                database.DatabaseOpened += (s, e) => LoadFilterCollection();
            }

            var libs = extensions.LibraryPlugins.OrderBy(a => a.Name).ToList();
            libs.Add(new FakePlayniteLibraryPlugin());
            Libraries = new SelectableLibraryPluginList(libs);

            // Remove filters for unloaded plugins
            var missing = filter.Library?.Ids.Where(a => Libraries.FirstOrDefault(b => b.Item.Id == a) == null)?.ToList();
            if (missing?.Any() == true)
            {
                if (filter.Library != null)
                {
                    missing.ForEach(a => filter.Library.Ids.Remove(a));
                }
            }

            database.Games.ItemCollectionChanged += Games_ItemCollectionChanged;
            database.Games.ItemUpdated += Games_ItemUpdated;
            database.Genres.ItemCollectionChanged += (_, args) => FullUpdateAvailableFilterList(Genres, args, database.Genres, filter.Genre);
            database.Platforms.ItemCollectionChanged += (_, args) => FullUpdateAvailableFilterList(Platforms, args, database.Platforms, filter.Platform);
            database.AgeRatings.ItemCollectionChanged += (_, args) => FullUpdateAvailableFilterList(AgeRatings, args, database.AgeRatings, filter.AgeRating);
            database.Categories.ItemCollectionChanged += (_, args) => FullUpdateAvailableFilterList(Categories, args, database.Categories, filter.Category);
            database.Regions.ItemCollectionChanged += (_, args) => FullUpdateAvailableFilterList(Regions, args, database.Regions, filter.Region);
            database.Series.ItemCollectionChanged += (_, args) => FullUpdateAvailableFilterList(Series, args, database.Series, filter.Series);
            database.Sources.ItemCollectionChanged += (_, args) => FullUpdateAvailableFilterList(Sources, args, database.Sources, filter.Source);
            database.Tags.ItemCollectionChanged += (_, args) => FullUpdateAvailableFilterList(Tags, args, database.Tags, filter.Tag);
            database.Features.ItemCollectionChanged += (_, args) => FullUpdateAvailableFilterList(Features, args, database.Features, filter.Feature);
            database.Companies.ItemCollectionChanged += (_, args) =>
            {
                FullUpdateAvailableFilterList(Publishers, args, database.Companies, filter.Publisher);
                FullUpdateAvailableFilterList(Developers, args, database.Companies, filter.Developer);
            };

            database.AgeRatingsInUseUpdated += (_, __) => InUseOnlyUpdateAvailableFilterList(AgeRatings, database.AgeRatings, database.UsedAgeRatings, filter.AgeRating);
            database.CategoriesInUseUpdated += (_, __) => InUseOnlyUpdateAvailableFilterList(Categories, database.Categories, database.UsedCategories, filter.Category);
            database.DevelopersInUseUpdated += (_, __) => InUseOnlyUpdateAvailableFilterList(Developers, database.Companies, database.UsedDevelopers, filter.Developer);
            database.FeaturesInUseUpdated += (_, __) => InUseOnlyUpdateAvailableFilterList(Features, database.Features, database.UsedFeastures, filter.Feature);
            database.GenresInUseUpdated += (_, __) => InUseOnlyUpdateAvailableFilterList(Genres, database.Genres, database.UsedGenres, filter.Genre);
            database.PlatformsInUseUpdated += (_, __) => InUseOnlyUpdateAvailableFilterList(Platforms, database.Platforms, database.UsedPlatforms, filter.Platform);
            database.PublishersInUseUpdated += (_, __) => InUseOnlyUpdateAvailableFilterList(Publishers, database.Companies, database.UsedPublishers, filter.Publisher);
            database.RegionsInUseUpdated += (_, __) => InUseOnlyUpdateAvailableFilterList(Regions, database.Regions, database.UsedRegions, filter.Region);
            database.SeriesInUseUpdated += (_, __) => InUseOnlyUpdateAvailableFilterList(Series, database.Series, database.UsedSeries, filter.Series);
            database.SourcesInUseUpdated += (_, __) => InUseOnlyUpdateAvailableFilterList(Sources, database.Sources, database.UsedSources, filter.Source);
            database.TagsInUseUpdated += (_, __) => InUseOnlyUpdateAvailableFilterList(Tags, database.Tags, database.UsedTags, filter.Tag);
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlayniteSettings.UsedFieldsOnlyOnFilterLists))
            {
                UpdateAllCollections();
            }
        }

        internal void LoadFilterCollection()
        {
            var years = database.Games.Where(a => a.ReleaseYear != null).Select(a => a.ReleaseYear).Distinct().OrderBy(a => a.Value).Select(a => a.ToString());
            ReleaseYears = new SelectableStringList(years, null, true);

            if (settings.UsedFieldsOnlyOnFilterLists)
            {
                Genres = new SelectableDbItemList(database.Genres.Get(database.UsedGenres), null, null, true);
                Platforms = new SelectableDbItemList(database.Platforms.Get(database.UsedPlatforms), null, null, true);
                AgeRatings = new SelectableDbItemList(database.AgeRatings.Get(database.UsedAgeRatings), null, null, true);
                Categories = new SelectableDbItemList(database.Categories.Get(database.UsedCategories), null, null, true);
                Publishers = new SelectableDbItemList(database.Companies.Get(database.UsedPublishers), null, null, true);
                Developers = new SelectableDbItemList(database.Companies.Get(database.UsedDevelopers), null, null, true);
                Regions = new SelectableDbItemList(database.Regions.Get(database.UsedRegions), null, null, true);
                Series = new SelectableDbItemList(database.Series.Get(database.UsedSeries), null, null, true);
                Sources = new SelectableDbItemList(database.Sources.Get(database.UsedSources), null, null, true);
                Tags = new SelectableDbItemList(database.Tags.Get(database.UsedTags), null, null, true);
                Features = new SelectableDbItemList(database.Features.Get(database.UsedFeastures), null, null, true);
            }
            else
            {
                Genres = new SelectableDbItemList(database.Genres, null, null, true);
                Platforms = new SelectableDbItemList(database.Platforms, null, null, true);
                AgeRatings = new SelectableDbItemList(database.AgeRatings, null, null, true);
                Categories = new SelectableDbItemList(database.Categories, null, null, true);
                Publishers = new SelectableDbItemList(database.Companies, null, null, true);
                Developers = new SelectableDbItemList(database.Companies, null, null, true);
                Regions = new SelectableDbItemList(database.Regions, null, null, true);
                Series = new SelectableDbItemList(database.Series, null, null, true);
                Sources = new SelectableDbItemList(database.Sources, null, null, true);
                Tags = new SelectableDbItemList(database.Tags, null, null, true);
                Features = new SelectableDbItemList(database.Features, null, null, true);
            }

            context.Send((a) =>
            {
                BindingOperations.EnableCollectionSynchronization(ReleaseYears, syncLockYears);
                BindingOperations.EnableCollectionSynchronization(Genres, syncLockGenres);
                BindingOperations.EnableCollectionSynchronization(Platforms, syncLockPlatforms);
                BindingOperations.EnableCollectionSynchronization(AgeRatings, syncLockAges);
                BindingOperations.EnableCollectionSynchronization(Categories, syncLockCategories);
                BindingOperations.EnableCollectionSynchronization(Publishers, syncLockPublishers);
                BindingOperations.EnableCollectionSynchronization(Developers, syncLockDevelopers);
                BindingOperations.EnableCollectionSynchronization(Regions, syncLockRegions);
                BindingOperations.EnableCollectionSynchronization(Series, syncLockSeries);
                BindingOperations.EnableCollectionSynchronization(Sources, syncLockSources);
                BindingOperations.EnableCollectionSynchronization(Tags, syncLockTags);
                BindingOperations.EnableCollectionSynchronization(Features, syncLockFeatures);
            }, null);
        }

        private void Games_ItemUpdated(object sender, ItemUpdatedEventArgs<Game> e)
        {
            if (IgnoreDatabaseUpdates)
            {
                missedDbUpdate = true;
                missedCollection.AddMissing(GameDatabaseCollection.Games);
                return;
            }

            foreach (var update in e.UpdatedItems)
            {
                if (update.OldData.ReleaseDate != update.NewData.ReleaseDate && update.NewData.ReleaseDate != null)
                {
                    ReleaseYears.Add(update.NewData.ReleaseYear.ToString());
                }
            }
        }

        private void Games_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<Game> e)
        {
            if (IgnoreDatabaseUpdates)
            {
                missedDbUpdate = true;
                missedCollection.AddMissing(GameDatabaseCollection.Games);
                return;
            }

            foreach (var update in e.AddedItems)
            {
                if (update.ReleaseDate != null)
                {
                    ReleaseYears.Add(update.ReleaseYear.ToString());
                }
            }
        }

        internal void UpdateAllCollections(List<GameDatabaseCollection> updateFields)
        {
            foreach (var field in updateFields)
            {
                UpdateAllCollections(field);
            }
        }

        internal void UpdateAllCollections(GameDatabaseCollection field)
        {
            switch (field)
            {
                case GameDatabaseCollection.Games:
                    var years = database.Games.Where(a => a.ReleaseYear != null).Select(a => a.ReleaseYear).Distinct().OrderBy(a => a.Value).Select(a => a.ToString());
                    ReleaseYears.SetItems(years, filter.ReleaseYear?.Values);
                    break;
                case GameDatabaseCollection.Platforms:
                    if (settings.UsedFieldsOnlyOnFilterLists)
                    {
                        Platforms.SetItems(database.Platforms.Get(database.UsedPlatforms), filter.Platform?.Ids);
                    }
                    else
                    {
                        Platforms.SetItems(database.Platforms, filter.Platform?.Ids);
                    }
                    break;
                case GameDatabaseCollection.Genres:
                    if (settings.UsedFieldsOnlyOnFilterLists)
                    {
                        Genres.SetItems(database.Genres.Get(database.UsedGenres), filter.Genre?.Ids);
                    }
                    else
                    {
                        Genres.SetItems(database.Genres, filter.Genre?.Ids);
                    }
                    break;
                case GameDatabaseCollection.Companies:
                    if (settings.UsedFieldsOnlyOnFilterLists)
                    {
                        Developers.SetItems(database.Companies.Get(database.UsedDevelopers), filter.Developer?.Ids);
                        Publishers.SetItems(database.Companies.Get(database.UsedPublishers), filter.Publisher?.Ids);
                    }
                    else
                    {
                        Developers.SetItems(database.Companies, filter.Developer?.Ids);
                        Publishers.SetItems(database.Companies, filter.Publisher?.Ids);
                    }
                    break;
                case GameDatabaseCollection.Tags:
                    if (settings.UsedFieldsOnlyOnFilterLists)
                    {
                        Tags.SetItems(database.Tags.Get(database.UsedTags), filter.Tag?.Ids);
                    }
                    else
                    {
                        Tags.SetItems(database.Tags, filter.Tag?.Ids);
                    }
                    break;
                case GameDatabaseCollection.Categories:
                    if (settings.UsedFieldsOnlyOnFilterLists)
                    {
                        Categories.SetItems(database.Categories.Get(database.UsedCategories), filter.Category?.Ids);
                    }
                    else
                    {
                        Categories.SetItems(database.Categories, filter.Category?.Ids);
                    }
                    break;
                case GameDatabaseCollection.Series:
                    if (settings.UsedFieldsOnlyOnFilterLists)
                    {
                        Series.SetItems(database.Series.Get(database.UsedSeries), filter.Series?.Ids);
                    }
                    else
                    {
                        Series.SetItems(database.Series, filter.Series?.Ids);
                    }
                    break;
                case GameDatabaseCollection.AgeRatings:
                    if (settings.UsedFieldsOnlyOnFilterLists)
                    {
                        AgeRatings.SetItems(database.AgeRatings.Get(database.UsedAgeRatings), filter.AgeRating?.Ids);
                    }
                    else
                    {
                        AgeRatings.SetItems(database.AgeRatings, filter.AgeRating?.Ids);
                    }
                    break;
                case GameDatabaseCollection.Regions:
                    if (settings.UsedFieldsOnlyOnFilterLists)
                    {
                        Regions.SetItems(database.Regions.Get(database.UsedRegions), filter.Region?.Ids);
                    }
                    else
                    {
                        Regions.SetItems(database.Regions, filter.Region?.Ids);
                    }
                    break;
                case GameDatabaseCollection.Sources:
                    if (settings.UsedFieldsOnlyOnFilterLists)
                    {
                        Sources.SetItems(database.Sources.Get(database.UsedSources), filter.Source?.Ids);
                    }
                    else
                    {
                        Sources.SetItems(database.Sources, filter.Source?.Ids);
                    }
                    break;
                case GameDatabaseCollection.Features:
                    if (settings.UsedFieldsOnlyOnFilterLists)
                    {
                        Features.SetItems(database.Features.Get(database.UsedFeastures), filter.Feature?.Ids);
                    }
                    else
                    {
                        Features.SetItems(database.Features, filter.Feature?.Ids);
                    }
                    break;
                default:
                    if (PlayniteEnvironment.ThrowAllErrors)
                    {
                        throw new NotSupportedException();
                    }
                    else
                    {
                        break;
                    }
            }
        }

        internal void UpdateAllCollections()
        {
            UpdateAllCollections(GameDatabaseCollection.Games);
            UpdateAllCollections(GameDatabaseCollection.Platforms);
            UpdateAllCollections(GameDatabaseCollection.Genres);
            UpdateAllCollections(GameDatabaseCollection.Companies);
            UpdateAllCollections(GameDatabaseCollection.Tags);
            UpdateAllCollections(GameDatabaseCollection.Categories);
            UpdateAllCollections(GameDatabaseCollection.Series);
            UpdateAllCollections(GameDatabaseCollection.AgeRatings);
            UpdateAllCollections(GameDatabaseCollection.Regions);
            UpdateAllCollections(GameDatabaseCollection.Sources);
            UpdateAllCollections(GameDatabaseCollection.Features);
        }

        private void InUseOnlyUpdateAvailableFilterList<T>(
            SelectableDbItemList targetList,
            IItemCollection<T> sourceColletion,
            List<Guid> usedList,
            FilterItemProperites filter) where T : DatabaseObject
        {
            if (!settings.UsedFieldsOnlyOnFilterLists)
            {
                return;
            }

            if (IgnoreDatabaseUpdates)
            {
                missedDbUpdate = true;
                missedCollection.AddMissing(sourceColletion.CollectionType);
                return;
            }

            targetList.SetItems(sourceColletion.Get(usedList), filter?.Ids);
        }

        private void FullUpdateAvailableFilterList<T>(
            SelectableDbItemList targetList,
            ItemCollectionChangedEventArgs<T> args,
            IItemCollection<T> sourceColletion,
            FilterItemProperites filter) where T : DatabaseObject
        {
            if (settings.UsedFieldsOnlyOnFilterLists)
            {
                return;
            }

            if (IgnoreDatabaseUpdates)
            {
                missedDbUpdate = true;
                missedCollection.AddMissing(sourceColletion.CollectionType);
                return;
            }

            if (args.AddedItems.HasItems() || args.RemovedItems.HasItems())
            {
                targetList.SetItems(sourceColletion, filter?.Ids);
            }
        }
    }
}
