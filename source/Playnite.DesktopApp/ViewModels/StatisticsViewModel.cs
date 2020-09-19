using Playnite.Database;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.DesktopApp.ViewModels
{
    public class StatisticsViewModel : ObservableObject
    {
        private static StatisticsViewModel designIntance;
        public static StatisticsViewModel DesignIntance
        {
            get
            {
                if (designIntance == null)
                {
                    designIntance = new StatisticsViewModel
                    {
                        GlobalStats = new GameStats
                        {
                            TotalCount = 33,
                            Favorite = new BaseStatInfo("", 1, 33),
                            Hidden = new BaseStatInfo("", 5, 33),
                            Installed = new BaseStatInfo("", 6, 33),
                            NotInstalled = new BaseStatInfo("", 20, 33),
                            TopPlayed = new List<BaseStatInfo>
                            {
                                new BaseStatInfo("Unreal Tournament", 2665165),
                            }
                        }
                    };
                }

                return designIntance;
            }
        }

        public class FilterSection
        {
            public GameField Field { get; }
            public string Name { get; }

            public FilterSection(GameField field, string name)
            {
                Field = field;
                Name = ResourceProvider.GetString(name);
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public class FilterObject : ObservableObject
        {
            public string DisplayName { get; }
            public object Value { get; }

            public string Name
            {
                get => DisplayName.IsNullOrEmpty() ? Value.ToString() : DisplayName;
            }

            public FilterObject(object value, string displayName)
            {
                Value = value;
                DisplayName = displayName;
            }

            public FilterObject(object value)
            {
                Value = value;
            }
        }

        public class BaseStatInfo
        {
            public string Name { get; set; }
            public long Value { get; set; }
            public int Percentage { get; set; }
            public Game Game { get; set; }

            public BaseStatInfo(string name, long value)
            {
                Name = name;
                Value = value;
            }

            public BaseStatInfo(string name, long value, long total) : this(name, value)
            {
                if (total != 0)
                {
                    Percentage = Convert.ToInt32(((double)value / (double)total) * 100);
                }
            }
        }

        public List<FilterSection> Filters { get; } = new List<FilterSection>
        {
            new FilterSection(GameField.None, LOC.None),
            new FilterSection(GameField.PluginId, LOC.GameProviderTitle),
            new FilterSection(GameField.Genres, LOC.GenresLabel),
            new FilterSection(GameField.Features, LOC.FeaturesLabel),
            new FilterSection(GameField.Tags, LOC.TagsLabel),
            new FilterSection(GameField.Platform, LOC.PlatformsTitle),
            new FilterSection(GameField.Developers, LOC.DevelopersLabel),
            new FilterSection(GameField.Publishers, LOC.PublishersLabel),
            new FilterSection(GameField.Categories, LOC.CategoriesLabel),
            new FilterSection(GameField.ReleaseYear, LOC.GameReleaseYearTitle),
            new FilterSection(GameField.Series, LOC.SeriesLabel),
            new FilterSection(GameField.AgeRating, LOC.AgeRatingsLabel),
            new FilterSection(GameField.Region, LOC.RegionsLabel),
            new FilterSection(GameField.Source, LOC.SourcesLabel),
        };

        private FilterSection selectedFilter;
        public FilterSection SelectedFilter
        {
            get => selectedFilter;
            set
            {
                selectedFilter = value;
                selectedFilterObject = null;
                LoadFilterObjects();
                OnPropertyChanged();
                ReloadFilteredData();
            }
        }

        private List<FilterObject> filterObjects;
        public List<FilterObject> FilterObjects
        {
            get => filterObjects;
            set
            {
                filterObjects = value;
                OnPropertyChanged();
            }
        }
        private FilterObject selectedFilterObject;
        public FilterObject SelectedFilterObject
        {
            get => selectedFilterObject;
            set
            {
                selectedFilterObject = value;
                OnPropertyChanged();
                ReloadFilteredData();
            }
        }

        public class GameStats
        {
            public List<BaseStatInfo> TopPlayed { get; set; }
            public List<BaseStatInfo> CompletionStates { get; set; }

            public long TotalCount { get; set; }
            public BaseStatInfo Installed { get; set; }
            public BaseStatInfo NotInstalled { get; set; }
            public BaseStatInfo Hidden { get; set; }
            public BaseStatInfo Favorite { get; set; }
            public long TotalPlayTime { get; set; }
            public long AvaragePlayTime { get; set; }
        }

        private GameDatabase database;
        private ExtensionFactory extensions;
        private PlayniteSettings settings;

        private GameStats globalStats;
        public GameStats GlobalStats
        {
            get => globalStats;
            set
            {
                globalStats = value;
                OnPropertyChanged();
            }
        }

        private GameStats filteredStats;
        public GameStats FilteredStats
        {
            get => filteredStats;
            set
            {
                filteredStats = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<Game> NavigateToGameCommand { get; }

        public RelayCommand<object> NavigateBackCommand { get; }

        public StatisticsViewModel()
        {
        }

        public StatisticsViewModel(
            GameDatabase database,
            ExtensionFactory extensions,
            PlayniteSettings settings,
            Action<Game> gameSelectionAction)
        {
            this.database = database;
            this.extensions = extensions;
            this.settings = settings;
            SelectedFilter = Filters[0];

            NavigateBackCommand = new RelayCommand<object>((a) =>
            {
                settings.CurrentApplicationView = ApplicationView.Library;
            });

            NavigateToGameCommand = new RelayCommand<Game>((a) =>
            {
                gameSelectionAction(a);
            });
        }

        public void Calculate()
        {
            if (!database.IsOpen)
            {
                return;
            }

            GlobalStats = FillData(false);
            FilteredStats = FillData(true);
        }

        private void LoadFilterObjects()
        {
            switch (SelectedFilter.Field)
            {
                case GameField.None:
                    FilterObjects = new List<FilterObject>();
                    break;
                case GameField.PluginId:
                    var libs = extensions.LibraryPlugins.ToList();
                    libs.Add(new FakePlayniteLibraryPlugin());
                    FilterObjects = new List<FilterObject>(libs.Select(a => new FilterObject(a, a.Name)));
                    break;
                case GameField.Genres:
                    FilterObjects = new List<FilterObject>(database.UsedGenres.Select(a => database.Genres[a]).OrderBy(a => a.Name).Select(a => new FilterObject(a)));
                    break;
                case GameField.Features:
                    FilterObjects = new List<FilterObject>(database.UsedFeastures.Select(a => database.Features[a]).OrderBy(a => a.Name).Select(a => new FilterObject(a)));
                    break;
                case GameField.Tags:
                    FilterObjects = new List<FilterObject>(database.UsedTags.Select(a => database.Tags[a]).OrderBy(a => a.Name).Select(a => new FilterObject(a)));
                    break;
                case GameField.Platform:
                    FilterObjects = new List<FilterObject>(database.UsedPlatforms.Select(a => database.Platforms[a]).OrderBy(a => a.Name).Select(a => new FilterObject(a)));
                    break;
                case GameField.Developers:
                    FilterObjects = new List<FilterObject>(database.UsedDevelopers.Select(a => database.Companies[a]).OrderBy(a => a.Name).Select(a => new FilterObject(a)));
                    break;
                case GameField.Publishers:
                    FilterObjects = new List<FilterObject>(database.UsedPublishers.Select(a => database.Companies[a]).OrderBy(a => a.Name).Select(a => new FilterObject(a)));
                    break;
                case GameField.Categories:
                    FilterObjects = new List<FilterObject>(database.UsedCategories.Select(a => database.Categories[a]).OrderBy(a => a.Name).Select(a => new FilterObject(a)));
                    break;
                case GameField.ReleaseYear:
                    var years = database.Games.Where(a => a.ReleaseYear != null).Select(a => a.ReleaseYear).Distinct().OrderBy(a => a.Value);
                    FilterObjects = new List<FilterObject>(years.Select(a => new FilterObject(a)));
                    break;
                case GameField.Series:
                    FilterObjects = new List<FilterObject>(database.UsedSeries.Select(a => database.Series[a]).OrderBy(a => a.Name).Select(a => new FilterObject(a)));
                    break;
                case GameField.AgeRating:
                    FilterObjects = new List<FilterObject>(database.UsedAgeRatings.Select(a => database.AgeRatings[a]).OrderBy(a => a.Name).Select(a => new FilterObject(a)));
                    break;
                case GameField.Region:
                    FilterObjects = new List<FilterObject>(database.UsedRegions.Select(a => database.Regions[a]).OrderBy(a => a.Name).Select(a => new FilterObject(a)));
                    break;
                case GameField.Source:
                    FilterObjects = new List<FilterObject>(database.UsedSources.Select(a => database.Sources[a]).OrderBy(a => a.Name).Select(a => new FilterObject(a)));
                    break;
                default:
                    if (PlayniteEnvironment.ThrowAllErrors)
                    {
                        throw new NotImplementedException($"LoadFilterObjects {SelectedFilter.Field}");
                    }
                    break;
            }
        }

        private GameStats FillData(bool filtered)
        {
            var total = 0;
            var installed = 0;
            var notinstalled = 0;
            var hidden = 0;
            var favorite = 0;
            long totalPlaytime = 0;

            var playStates = new Dictionary<CompletionStatus, long>();
            foreach (CompletionStatus enm in Enum.GetValues(typeof(CompletionStatus)))
            {
                playStates.Add(enm, 0);
            }

            foreach (var game in database.Games)
            {
                if (filtered && !PassesFilter(game))
                {
                    continue;
                }

                total++;
                totalPlaytime += game.Playtime;

                if (game.IsInstalled)
                {
                    installed++;
                }

                if (!game.IsInstalled)
                {
                    notinstalled++;
                }

                if (game.Hidden)
                {
                    hidden++;
                }

                if (game.Favorite)
                {
                    favorite++;
                }

                playStates[game.CompletionStatus]++;
            }

            return new GameStats
            {
                CompletionStates = playStates.
                    OrderByDescending(a => a.Value).
                    Select(a => new BaseStatInfo(a.Key.GetDescription(), a.Value, total)).
                    ToList(),
                Favorite = new BaseStatInfo("", favorite, total),
                Hidden = new BaseStatInfo("", hidden, total),
                Installed = new BaseStatInfo("", installed, total),
                NotInstalled = new BaseStatInfo("", notinstalled, total),
                TotalCount = total,
                TotalPlayTime = totalPlaytime,
                AvaragePlayTime = total > 0 ? totalPlaytime / total : 0,
                TopPlayed = database.Games.
                    Where(a => !filtered || PassesFilter(a)).
                    OrderByDescending(a => a.Playtime).
                    Take(50).
                    Select(a => new BaseStatInfo(a.Name, a.Playtime, totalPlaytime) { Game = a }).ToList()
            };
        }

        private bool PassesFilter(Game game)
        {
            if (SelectedFilter == null || SelectedFilterObject == null)
            {
                return true;
            }

            switch (SelectedFilter.Field)
            {
                case GameField.None:
                    FilterObjects = new List<FilterObject>();
                    break;
                case GameField.PluginId:
                    return game.PluginId == ((LibraryPlugin)SelectedFilterObject.Value).Id;
                case GameField.Genres:
                    return game.GenreIds?.Contains(((DatabaseObject)SelectedFilterObject.Value).Id) == true;
                case GameField.Features:
                    return game.FeatureIds?.Contains(((DatabaseObject)SelectedFilterObject.Value).Id) == true;
                case GameField.Tags:
                    return game.TagIds?.Contains(((DatabaseObject)SelectedFilterObject.Value).Id) == true;
                case GameField.Platform:
                    return game.PlatformId == ((DatabaseObject)SelectedFilterObject.Value).Id;
                case GameField.Developers:
                    return game.DeveloperIds?.Contains(((DatabaseObject)SelectedFilterObject.Value).Id) == true;
                case GameField.Publishers:
                    return game.PublisherIds?.Contains(((DatabaseObject)SelectedFilterObject.Value).Id) == true;
                case GameField.Categories:
                    return game.CategoryIds?.Contains(((DatabaseObject)SelectedFilterObject.Value).Id) == true;
                case GameField.ReleaseYear:
                    return game.ReleaseDate?.Year == (int?)SelectedFilterObject.Value;
                case GameField.Series:
                    return game.SeriesId == ((DatabaseObject)SelectedFilterObject.Value).Id;
                case GameField.AgeRating:
                    return game.AgeRatingId == ((DatabaseObject)SelectedFilterObject.Value).Id;
                case GameField.Region:
                    return game.RegionId == ((DatabaseObject)SelectedFilterObject.Value).Id;
                case GameField.Source:
                    return game.SourceId == ((DatabaseObject)SelectedFilterObject.Value).Id;
                default:
                    if (PlayniteEnvironment.ThrowAllErrors)
                    {
                        throw new NotImplementedException($"PassesFilter {SelectedFilter.Field}");
                    }
                    break;
            }

            return true;
        }

        private void ReloadFilteredData()
        {
            FilteredStats = FillData(true);
        }
    }
}
