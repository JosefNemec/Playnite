using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{

    public class DatabaseFilter : ObservableObject
    {
        private GameDatabase database;        
        private FilterSettings filter;

        public SelectableItemList<ILibraryPlugin> Libraries { get; set; }

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

        public DatabaseFilter(GameDatabase database, ExtensionFactory extensions, FilterSettings filter)
        {
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

            var libs = extensions.LibraryPlugins.Values.Select(a => a.Plugin).ToList();
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
            Genres = new SelectableDbItemList(database.Genres);
            Platforms = new SelectableDbItemList(database.Platforms);
            AgeRatings = new SelectableDbItemList(database.AgeRatings);
            Categories = new SelectableDbItemList(database.Categories);
            Publishers = new SelectableDbItemList(database.Companies);
            Developers = new SelectableDbItemList(database.Companies);
            Regions = new SelectableDbItemList(database.Regions);
            Series = new SelectableDbItemList(database.Series);
            Sources = new SelectableDbItemList(database.Sources);
            Tags = new SelectableDbItemList(database.Tags);

            database.Genres.ItemCollectionChanged += (s, args) => UpdateAvailableFilterList(Genres, args);
            database.Platforms.ItemCollectionChanged += (s, args) => UpdateAvailableFilterList(Platforms, args);
            database.AgeRatings.ItemCollectionChanged += (s, args) => UpdateAvailableFilterList(AgeRatings, args);
            database.Categories.ItemCollectionChanged += (s, args) => UpdateAvailableFilterList(Categories, args);
            database.Regions.ItemCollectionChanged += (s, args) => UpdateAvailableFilterList(Regions, args);
            database.Series.ItemCollectionChanged += (s, args) => UpdateAvailableFilterList(Series, args);
            database.Sources.ItemCollectionChanged += (s, args) => UpdateAvailableFilterList(Sources, args);
            database.Tags.ItemCollectionChanged += (s, args) => UpdateAvailableFilterList(Tags, args);
            database.Companies.ItemCollectionChanged += (s, args) =>
            {
                UpdateAvailableFilterList(Publishers, args);
                UpdateAvailableFilterList(Developers, args);
            };
        }

        private void UpdateAvailableFilterList<T>(SelectableDbItemList targetList, ItemCollectionChangedEventArgs<T> args) where T : DatabaseObject
        {
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
