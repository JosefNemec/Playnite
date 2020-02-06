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
        private GameDatabase database;
        private FilterSettings filter;

        private bool missedDbUpdate = false;
        private bool ignoreDatabaseUpdates = false;
        public bool IgnoreDatabaseUpdates
        {
            get => ignoreDatabaseUpdates;
            set
            {
                ignoreDatabaseUpdates = value;
                if (value == false && missedDbUpdate)
                {
                    LoadFilterCollection();
                }

                missedDbUpdate = false;
            }
        }

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

        public DatabaseFilter(GameDatabase database, ExtensionFactory extensions, FilterSettings filter)
        {
            this.context = SynchronizationContext.Current;
            this.database = database;
            this.filter = filter;

            if (database.IsOpen)
            {
                LoadFilterCollection();
            }
            else
            {
                database.DatabaseOpened += (s, e) => LoadFilterCollection();
            }

            var libs = extensions.LibraryPlugins.ToList();
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
        }

        internal void LoadFilterCollection()
        {
            var years = database.Games.Where(a => a.ReleaseYear != null).Select(a => a.ReleaseYear).Distinct().OrderBy(a => a.Value).Select(a => a.ToString());
            ReleaseYears = new SelectableStringList(years, null, true);
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

            database.Games.ItemCollectionChanged += Games_ItemCollectionChanged;
            database.Games.ItemUpdated += Games_ItemUpdated;
            database.Genres.ItemCollectionChanged += (s, args) => UpdateAvailableFilterList(Genres, args);
            database.Platforms.ItemCollectionChanged += (s, args) => UpdateAvailableFilterList(Platforms, args);
            database.AgeRatings.ItemCollectionChanged += (s, args) => UpdateAvailableFilterList(AgeRatings, args);
            database.Categories.ItemCollectionChanged += (s, args) => UpdateAvailableFilterList(Categories, args);
            database.Regions.ItemCollectionChanged += (s, args) => UpdateAvailableFilterList(Regions, args);
            database.Series.ItemCollectionChanged += (s, args) => UpdateAvailableFilterList(Series, args);
            database.Sources.ItemCollectionChanged += (s, args) => UpdateAvailableFilterList(Sources, args);
            database.Tags.ItemCollectionChanged += (s, args) => UpdateAvailableFilterList(Tags, args);
            database.Features.ItemCollectionChanged += (s, args) => UpdateAvailableFilterList(Features, args);
            database.Companies.ItemCollectionChanged += (s, args) =>
            {
                UpdateAvailableFilterList(Publishers, args);
                UpdateAvailableFilterList(Developers, args);
            };
        }

        private void Games_ItemUpdated(object sender, ItemUpdatedEventArgs<Game> e)
        {
            if (IgnoreDatabaseUpdates)
            {
                missedDbUpdate = true;
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

        private void UpdateAvailableFilterList<T>(SelectableDbItemList targetList, ItemCollectionChangedEventArgs<T> args) where T : DatabaseObject
        {
            if (IgnoreDatabaseUpdates)
            {
                missedDbUpdate = true;
                return;
            }

            if (args.AddedItems.HasItems())
            {
                args.AddedItems.ForEach(a => targetList.Add(a));
            }

            if (args.RemovedItems.HasItems())
            {
                args.RemovedItems.ForEach(a => targetList.Remove(a));
            }
        }
    }
}
