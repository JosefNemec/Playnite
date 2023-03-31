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
            public ulong Value { get; set; }
            public int Percentage { get; set; }
            public Game Game { get; set; }

            public BaseStatInfo(string name, ulong value)
            {
                Name = name;
                Value = value;
            }

            public BaseStatInfo(string name, ulong value, ulong total) : this(name, value)
            {
                if (total > 0 && value <= total)
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
            new FilterSection(GameField.Platforms, LOC.PlatformsTitle),
            new FilterSection(GameField.Developers, LOC.DevelopersLabel),
            new FilterSection(GameField.Publishers, LOC.PublishersLabel),
            new FilterSection(GameField.Categories, LOC.CategoriesLabel),
            new FilterSection(GameField.ReleaseYear, LOC.GameReleaseYearTitle),
            new FilterSection(GameField.Series, LOC.SeriesLabel),
            new FilterSection(GameField.AgeRatings, LOC.AgeRatingsLabel),
            new FilterSection(GameField.Regions, LOC.RegionsLabel),
            new FilterSection(GameField.Source, LOC.SourcesLabel),
            new FilterSection(GameField.CompletionStatus, LOC.CompletionStatus),
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

            public ulong TotalCount { get; set; }
            public BaseStatInfo Installed { get; set; }
            public BaseStatInfo NotInstalled { get; set; }
            public BaseStatInfo Hidden { get; set; }
            public BaseStatInfo Favorite { get; set; }
            public ulong TotalPlayTime { get; set; }
            public ulong AvaragePlayTime { get; set; }
            public ulong TotalInstallSize { get; set; }
        }

        private IGameDatabaseMain database;
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

        private bool includeHidden;
        public bool IncludeHidden
        {
            get => includeHidden;
            set
            {
                includeHidden = value;
                OnPropertyChanged();
                Calculate();
            }
        }

        public RelayCommand<Game> NavigateToGameCommand { get; }

        public RelayCommand<object> NavigateBackCommand { get; }

        public StatisticsViewModel()
        {
        }

        public StatisticsViewModel(
            IGameDatabaseMain database,
            ExtensionFactory extensions,
            PlayniteSettings settings,
            Action switchToLibraryViewAction,
            Action<Game> gameSelectionAction)
        {
            this.database = database;
            this.extensions = extensions;
            this.settings = settings;
            SelectedFilter = Filters[0];

            NavigateBackCommand = new RelayCommand<object>((a) =>
            {
                switchToLibraryViewAction();
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
                case GameField.Platforms:
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
                case GameField.AgeRatings:
                    FilterObjects = new List<FilterObject>(database.UsedAgeRatings.Select(a => database.AgeRatings[a]).OrderBy(a => a.Name).Select(a => new FilterObject(a)));
                    break;
                case GameField.Regions:
                    FilterObjects = new List<FilterObject>(database.UsedRegions.Select(a => database.Regions[a]).OrderBy(a => a.Name).Select(a => new FilterObject(a)));
                    break;
                case GameField.Source:
                    FilterObjects = new List<FilterObject>(database.UsedSources.Select(a => database.Sources[a]).OrderBy(a => a.Name).Select(a => new FilterObject(a)));
                    break;
                case GameField.CompletionStatus:
                    FilterObjects = new List<FilterObject>(database.UsedCompletionStatuses.Select(a => database.CompletionStatuses[a]).OrderBy(a => a.Name).Select(a => new FilterObject(a)));
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
            ulong totalGames = 0;
            ulong totalGamesWithPlayTime = 0;
            ulong installed = 0;
            ulong notinstalled = 0;
            ulong hidden = 0;
            ulong favorite = 0;
            ulong totalPlaytime = 0;
            ulong totalInstallSize = 0;

            var compStats = new Dictionary<Guid, ulong>();
            foreach (var game in database.Games)
            {
                if (game.Hidden && !IncludeHidden)
                {
                    continue;
                }

                if (filtered && !PassesFilter(game))
                {
                    continue;
                }

                totalGames++;
                if (game.Playtime > 0)
                {
                    totalGamesWithPlayTime++;
                    totalPlaytime += game.Playtime;
                }

                if (game.IsInstalled && game?.InstallSize > 0)
                {
                    totalInstallSize += game.InstallSize.Value;
                }

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

                if (database.CompletionStatuses[game.CompletionStatusId] != null)
                {
                    if (compStats.TryGetValue(game.CompletionStatusId, out var currentCount))
                    {
                        compStats[game.CompletionStatusId] = currentCount + 1;
                    }
                    else
                    {
                        compStats.Add(game.CompletionStatusId, 1);
                    }
                }
            }

            return new GameStats
            {
                CompletionStates = compStats.
                    OrderByDescending(a => a.Value).
                    Select(a => new BaseStatInfo(database.CompletionStatuses[a.Key].Name, a.Value, totalGames)).
                    ToList(),
                Favorite = new BaseStatInfo("", favorite, totalGames),
                Hidden = new BaseStatInfo("", hidden, totalGames),
                Installed = new BaseStatInfo("", installed, totalGames),
                NotInstalled = new BaseStatInfo("", notinstalled, totalGames),
                TotalCount = totalGames,
                TotalPlayTime = totalPlaytime,
                AvaragePlayTime = totalGamesWithPlayTime > 0 ? totalPlaytime / totalGamesWithPlayTime : 0,
                TopPlayed = database.Games.
                    Where(a => !filtered || PassesFilter(a)).
                    Where(a => !a.Hidden || (a.Hidden && IncludeHidden)).
                    OrderByDescending(a => a.Playtime).
                    Take(50).
                    Select(a => new BaseStatInfo(a.Name, a.Playtime, totalPlaytime) { Game = a }).ToList(),
                TotalInstallSize = totalInstallSize
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
                case GameField.Platforms:
                    return game.PlatformIds?.Contains(((DatabaseObject)SelectedFilterObject.Value).Id) == true;
                case GameField.Developers:
                    return game.DeveloperIds?.Contains(((DatabaseObject)SelectedFilterObject.Value).Id) == true;
                case GameField.Publishers:
                    return game.PublisherIds?.Contains(((DatabaseObject)SelectedFilterObject.Value).Id) == true;
                case GameField.Categories:
                    return game.CategoryIds?.Contains(((DatabaseObject)SelectedFilterObject.Value).Id) == true;
                case GameField.ReleaseYear:
                    return game.ReleaseDate?.Year == (int?)SelectedFilterObject.Value;
                case GameField.Series:
                    return game.SeriesIds?.Contains(((DatabaseObject)SelectedFilterObject.Value).Id) == true;
                case GameField.AgeRatings:
                    return game.AgeRatingIds?.Contains(((DatabaseObject)SelectedFilterObject.Value).Id) == true;
                case GameField.Regions:
                    return game.RegionIds?.Contains(((DatabaseObject)SelectedFilterObject.Value).Id) == true;
                case GameField.Source:
                    return game.SourceId == ((DatabaseObject)SelectedFilterObject.Value).Id;
                case GameField.CompletionStatus:
                    return game.CompletionStatusId == ((DatabaseObject)SelectedFilterObject.Value).Id;
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
