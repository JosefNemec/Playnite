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
        CategoryGrouped
    }

    public class GamesCollectionView : ObservableObject, IDisposable
    {
        private static ILogger logger = LogManager.GetLogger();

        private GameDatabase database;
        private List<Platform> platformsCache;
        private ExtensionFactory extensions;

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
                OnPropertyChanged("CollectionView");
            }
        }

        public PlayniteSettings Settings
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

        public GamesCollectionView(GameDatabase database, PlayniteSettings settings, bool fullScreen, ExtensionFactory extensions)
        {
            IsFullscreen = fullScreen;
            this.database = database;
            this.extensions = extensions;
            platformsCache = database.Platforms.ToList();
            database.Games.ItemCollectionChanged += Database_GamesCollectionChanged;
            database.Games.ItemUpdated += Database_GameUpdated;
            database.Platforms.ItemCollectionChanged += Database_PlatformsCollectionChanged;
            database.Platforms.ItemUpdated += Database_PlatformUpdated;
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

            CollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(Items);
            CollectionView.Filter = Filter;
            SetViewConfiguration();
        }

        public void Dispose()
        {
            database.Games.ItemCollectionChanged -= Database_GamesCollectionChanged;
            database.Games.ItemUpdated -= Database_GameUpdated;
            database.Platforms.ItemCollectionChanged -= Database_PlatformsCollectionChanged;
            database.Platforms.ItemUpdated -= Database_PlatformUpdated;
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

            // ------------------ Hidden
            bool hiddenResult = true;
            if (filterSettings.Hidden && game.Hidden)
            {
                hiddenResult = true;
            }
            else if (!filterSettings.Hidden && game.Hidden)
            {
                hiddenResult = false;
            }
            else if (filterSettings.Hidden && !game.Hidden)
            {
                hiddenResult = false;
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

            // ------------------ Providers
            bool librariesFilter = false;
            if (filterSettings.Libraries?.Any() == true)
            {
                var libInter = filterSettings.Libraries?.Intersect(new List<Guid> { game.PluginId });
                librariesFilter = libInter?.Any() == true;
            }
            else
            {
                librariesFilter = true;
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
                    nameResult = false;
                }
                else
                {
                    nameResult = (game.Name.IndexOf(filterSettings.Name, StringComparison.OrdinalIgnoreCase) >= 0);
                }
            }

            // ------------------ Series filter
            bool seriesResult = false;
            if (string.IsNullOrEmpty(filterSettings.Series))
            {
                seriesResult = true;
            }
            else
            {
                if (string.IsNullOrEmpty(game.Series))
                {
                    seriesResult = false;
                }
                else
                {
                    seriesResult = (game.Series.IndexOf(filterSettings.Series, StringComparison.OrdinalIgnoreCase) >= 0);
                }
            }

            // ------------------ Region filter
            bool regionResult = false;
            if (string.IsNullOrEmpty(filterSettings.Region))
            {
                regionResult = true;
            }
            else
            {
                if (string.IsNullOrEmpty(game.Region))
                {
                    regionResult = false;
                }
                else
                {
                    regionResult = (game.Region.IndexOf(filterSettings.Region, StringComparison.OrdinalIgnoreCase) >= 0);
                }
            }

            // ------------------ Source filter
            bool sourceResult = false;
            if (string.IsNullOrEmpty(filterSettings.Source))
            {
                sourceResult = true;
            }
            else
            {
                if (string.IsNullOrEmpty(game.Source))
                {
                    sourceResult = false;
                }
                else
                {
                    sourceResult = (game.Source.IndexOf(filterSettings.Source, StringComparison.OrdinalIgnoreCase) >= 0);
                }
            }

            // ------------------ AgeRating filter
            bool ageRatingResult = false;
            if (string.IsNullOrEmpty(filterSettings.AgeRating))
            {
                ageRatingResult = true;
            }
            else
            {
                if (string.IsNullOrEmpty(game.AgeRating))
                {
                    ageRatingResult = false;
                }
                else
                {
                    ageRatingResult = (game.AgeRating.IndexOf(filterSettings.AgeRating, StringComparison.OrdinalIgnoreCase) >= 0);
                }
            }

            // ------------------ Genre
            bool genreResult = false;
            if (filterSettings.Genres == null || filterSettings.Genres.Count == 0)
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
                    genreResult = filterSettings.Genres.IntersectsPartiallyWith(game.Genres);
                }
            }

            // ------------------ Platform
            bool platformResult = false;
            if (filterSettings.Platforms == null || filterSettings.Platforms.Count == 0)
            {
                platformResult = true;
            }
            else
            {
                if (game.PlatformId == Guid.Empty)
                {
                    platformResult = false;
                }
                else
                {
                    var platform = GetPlatformFromCache(game);
                    if (platform == null)
                    {
                        platformResult = false;
                    }
                    else
                    {
                        platformResult = filterSettings.Platforms.Any(a => !string.IsNullOrEmpty(a) && platform.Name.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0);
                    }
                }
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

                    releaseDateResult = false;
                }
                else
                {
                    releaseDateResult = game.ReleaseDate.Value.ToString(Constants.DateUiFormat).IndexOf(filterSettings.ReleaseDate, StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }

            // ------------------ Publisher
            bool publisherResult = false;
            if (filterSettings.Publishers == null || filterSettings.Publishers.Count == 0)
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
                    publisherResult = filterSettings.Publishers.IntersectsPartiallyWith(game.Publishers);
                }
            }

            // ------------------ Developer
            bool developerResult = false;
            if (filterSettings.Developers == null || filterSettings.Developers.Count == 0)
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
                    developerResult = filterSettings.Developers.IntersectsPartiallyWith(game.Developers);
                }
            }

            // ------------------ Category
            bool categoryResult = false;
            if (filterSettings.Categories == null || filterSettings.Categories.Count == 0)
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
                        categoryResult = filterSettings.Categories.IntersectsPartiallyWith(game.Categories);
                    }
                    else
                    {
                        categoryResult = filterSettings.Categories.Any(a => entry.Category.Category.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0);
                    }
                }
            }

            // ------------------ Tags
            bool tagResult = false;
            if (filterSettings.Tags == null || filterSettings.Tags.Count == 0)
            {
                tagResult = true;
            }
            else
            {
                if (game.Tags == null)
                {
                    tagResult = false;
                }
                else
                {
                    tagResult = filterSettings.Tags.IntersectsPartiallyWith(game.Tags);
                }
            }

            return installedResult &&
                hiddenResult &&
                favoriteResult &&
                nameResult &&
                librariesFilter &&
                genreResult &&
                platformResult &&
                releaseDateResult &&
                publisherResult &&
                developerResult &&
                categoryResult &&
                tagResult &&
                seriesResult &&
                regionResult &&
                sourceResult &&
                ageRatingResult;
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((new string[] { "SortingOrder", "GroupingOrder", "SortingOrderDirection" }).Contains(e.PropertyName))
            {
                UpdateViewConfiguration();
            }
        }

        private void FilterSettings_FilterChanged(object sender, FilterChangedEventArgs e)
        {
            logger.Debug("Refreshing collection view filter.");
            CollectionView.Refresh();
        }

        private void SetViewDescriptions()
        {
            ViewSettings viewSettings = IsFullscreen ? Settings.FullscreenViewSettings : Settings.ViewSettings;            
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
                    case GroupOrder.Platform:
                        ViewType = GamesViewType.Standard;
                        break;
                    case GroupOrder.Category:
                        ViewType = GamesViewType.CategoryGrouped;
                        break;
                }
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
                CollectionView.GroupDescriptions.Add(new PropertyGroupDescription(viewSettings.GroupingOrder.ToString()));
                if (CollectionView.SortDescriptions.First().PropertyName != viewSettings.GroupingOrder.ToString())
                {
                    CollectionView.SortDescriptions.Insert(0, new SortDescription(viewSettings.GroupingOrder.ToString(), ListSortDirection.Ascending));
                }
            }
        }

        private void SetViewConfiguration()
        {
            using (CollectionView.DeferRefresh())
            {
                SetViewDescriptions();

                CollectionView.LiveGroupingProperties.Add("PluginId");
                CollectionView.LiveGroupingProperties.Add("Category");
                CollectionView.LiveGroupingProperties.Add("Platform");

                //CollectionView.LiveSortingProperties.Add("Provider");
                CollectionView.LiveSortingProperties.Add("Name");
                CollectionView.LiveSortingProperties.Add("Categories");
                CollectionView.LiveSortingProperties.Add("Genres");
                CollectionView.LiveSortingProperties.Add("ReleaseDate");
                CollectionView.LiveSortingProperties.Add("Developers");
                CollectionView.LiveSortingProperties.Add("Tags");
                CollectionView.LiveSortingProperties.Add("Publishers");
                CollectionView.LiveSortingProperties.Add("IsInstalled");
                CollectionView.LiveSortingProperties.Add("Hidden");
                CollectionView.LiveSortingProperties.Add("Favorite");
                CollectionView.LiveSortingProperties.Add("LastActivity");
                CollectionView.LiveSortingProperties.Add("Platform");

                //CollectionView.LiveFilteringProperties.Add("Provider");
                CollectionView.LiveFilteringProperties.Add("Name");
                CollectionView.LiveFilteringProperties.Add("Categories");
                CollectionView.LiveFilteringProperties.Add("Genres");
                CollectionView.LiveFilteringProperties.Add("ReleaseDate");
                CollectionView.LiveFilteringProperties.Add("Developers");
                CollectionView.LiveFilteringProperties.Add("Tags");
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

            if (IsFullscreen)
            {
                Items.Clear();
                Items.AddRange(database.Games.Select(x => new GameViewEntry(x, string.Empty, GetPlatformFromCache(x), GetLibraryPlugin(x))));
            }
            else
            {
                switch (viewType)
                {
                    case GamesViewType.Standard:
                        Items.Clear();
                        Items.AddRange(database.Games.Select(x => new GameViewEntry(x, string.Empty, GetPlatformFromCache(x), GetLibraryPlugin(x))));
                        break;

                    case GamesViewType.CategoryGrouped:
                        Items.Clear();
                        Items.AddRange(database.Games.SelectMany(x =>
                        {
                            if (x.Categories == null || x.Categories.Count == 0)
                            {
                                return new List<GameViewEntry>()
                                {
                            new GameViewEntry(x, null, GetPlatformFromCache(x), GetLibraryPlugin(x))
                                };
                            }
                            else
                            {
                                return x.Categories.Select(c =>
                                {
                                    return new GameViewEntry(x, c, GetPlatformFromCache(x), GetLibraryPlugin(x));
                                });
                            }
                        }));

                        break;
                }
            }

            this.viewType = viewType;
        }

        private Platform GetPlatformFromCache(Game game)
        {
            return platformsCache?.FirstOrDefault(a => a.Id == game.PlatformId);
        }

        private ILibraryPlugin GetLibraryPlugin(Game game)
        {
            if (game.PluginId != Guid.Empty && extensions.LibraryPlugins.TryGetValue(game.PluginId, out var plugin))
            {
                return plugin.Plugin;
            }

            return null;
        }

        private void Database_PlatformUpdated(object sender, ItemUpdatedEventArgs<Platform> args)
        {
            platformsCache = database.Platforms.ToList();
            var platformIds = args.UpdatedItems.Select(a => a.NewData.Id).ToList();
            foreach (var item in Items.Where(a => a.PlatformId != null && platformIds.Contains(a.PlatformId)))
            {
                item.Platform.Platform = GetPlatformFromCache(item.Game);
                item.OnPropertyChanged("Platform");
                item.OnPropertyChanged("DefaultIcon");
                item.OnPropertyChanged("DefaultImage");
            }
        }

        private void Database_PlatformsCollectionChanged(object sender, ItemCollectionChangedEventArgs<Platform> args)
        {
            platformsCache = database.Platforms.ToList();
        }

        private void Database_GameUpdated(object sender, ItemUpdatedEventArgs<Game> args)
        {
            var replaceList = new List<Game>();
            foreach (var update in args.UpdatedItems)
            {
                if (update.OldData.Categories.IsListEqual(update.NewData.Categories))
                {
                    var existingItem = Items.FirstOrDefault(a => a.Game.Id == update.NewData.Id);
                    if (existingItem != null)
                    {
                        if (update.NewData.PlatformId != update.OldData.PlatformId)
                        {
                            existingItem.Platform = new PlatformView(update.NewData.PlatformId, GetPlatformFromCache(update.NewData));
                        }
                    }
                    else
                    {
                        logger.Warn("Receivied update for unknown game id " + update.NewData.Id);
                    }
                }
                else
                {
                    replaceList.Add(update.NewData);
                }
            }

            if (replaceList.Count > 0)
            {
                Database_GamesCollectionChanged(this, new ItemCollectionChangedEventArgs<Game>(replaceList, replaceList));
            }
        }

        private void Database_GamesCollectionChanged(object sender, ItemCollectionChangedEventArgs<Game> args)
        {
            // DO NOT use *Range methods for "Items" object.
            // It can throw weird exceptions in virtualization panel, directly in WPF (without known fix from MS).
            // https://github.com/JosefNemec/Playnite/issues/796

            if (args.RemovedItems.Count > 0)
            {
                var removeIds = args.RemovedItems.Select(a => a.Id);
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
                    addList.Add(new GameViewEntry(game, string.Empty, GetPlatformFromCache(game), GetLibraryPlugin(game)));
                }
                else
                {
                    switch (ViewType)
                    {
                        case GamesViewType.Standard:
                            addList.Add(new GameViewEntry(game, string.Empty, GetPlatformFromCache(game), GetLibraryPlugin(game)));
                            break;

                        case GamesViewType.CategoryGrouped:
                            if (game.Categories == null || game.Categories.Count == 0)
                            {
                                addList.Add(new GameViewEntry(game, string.Empty, GetPlatformFromCache(game), GetLibraryPlugin(game)));
                            }
                            else
                            {
                                addList.AddRange(game.Categories.Select(a => new GameViewEntry(game, a, GetPlatformFromCache(game), GetLibraryPlugin(game))));
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
