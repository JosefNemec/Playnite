using NUnit.Framework;
using Playnite.Common;
using Playnite.Database;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Playnite.Tests.Database
{
    [TestFixture]
    public class GameLibraryFilterTests
    {
        private TempDirectory TempDirectory { get; set; }
        private GameDatabase GameDatabase { get; set; }

        private Game GameNone { get; set; }
        private Game GameA { get; set; }
        private Game GameB { get; set; }
        private Game GameBoth { get; set; }
        private Game GameHidden { get; set; }

        private readonly Guid SteamPluginId = Guid.NewGuid();
        private readonly Guid XboxPluginId = Guid.NewGuid();

        [OneTimeSetUp]
        public void Init()
        {
            ulong secondsFromHours(double hours) => (ulong)(hours * 3600D);

            TempDirectory = TempDirectory.Create();
            GameDatabase = new TestGameDatabase(TempDirectory.TempPath);
            GameDatabase.OpenDatabase();
            GameDatabase.AgeRatings.Add("Everyone");
            GameDatabase.AgeRatings.Add("Mature");
            GameDatabase.Categories.Add("Has cats");
            GameDatabase.Categories.Add("Has dogs");
            GameDatabase.Companies.Add("iD Software");
            GameDatabase.Companies.Add("Bethesda");
            var notPlayed = GameDatabase.CompletionStatuses.Add("Not played");
            var played = GameDatabase.CompletionStatuses.Add("Played");
            var beaten = GameDatabase.CompletionStatuses.Add("Beaten");
            GameDatabase.Features.Add("Singleplayer");
            GameDatabase.Features.Add("Multiplayer");
            GameDatabase.Genres.Add("Action");
            GameDatabase.Genres.Add("Strategy");
            GameDatabase.Platforms.Add("PC (Windows)");
            GameDatabase.Platforms.Add("Xbox");
            GameDatabase.Regions.Add("Europe");
            GameDatabase.Regions.Add("Japan");
            GameDatabase.Series.Add("Assassin's Creed");
            GameDatabase.Series.Add("Forza");
            GameDatabase.Sources.Add("Steam");
            GameDatabase.Sources.Add("Xbox");
            GameDatabase.Tags.Add("Cinematic");
            GameDatabase.Tags.Add("Surreal");

            var now = DateTime.Now;

            GameNone = new Game("Game None")
            {
                Version = null,
                ReleaseDate = null,
                Added = null,
                LastActivity = null,
                Modified = null,
                Playtime = 0,
                InstallSize = null,
                CommunityScore = null,
                CriticScore = null,
                UserScore = null,
                Favorite = false,
                IsInstalled = false,
                PluginId = Guid.Empty,
                SourceId = Guid.Empty,
                CompletionStatusId = Guid.Empty,
            };

            GameA = new Game("Game Alpha")
            {
                Version = "1.0",
                ReleaseDate = new ReleaseDate(2015, 12, 9),
                Added = now - TimeSpan.FromDays(200),
                LastActivity = null,
                Modified = null,
                Playtime = 0,
                InstallSize = 1000, //under 100MB
                CommunityScore = 11,
                CriticScore = 11,
                UserScore = 11,
                Favorite = true,
                IsInstalled = true,
                PluginId = SteamPluginId,
                SourceId = GameDatabase.Sources.First().Id,
                CompletionStatusId = notPlayed.Id,
                AgeRatingIds = new List<Guid> { GameDatabase.AgeRatings.First().Id },
                CategoryIds = new List<Guid> { GameDatabase.Categories.First().Id },
                DeveloperIds = new List<Guid> { GameDatabase.Companies.First().Id },
                PublisherIds = new List<Guid> { GameDatabase.Companies.First().Id },
                FeatureIds = new List<Guid> { GameDatabase.Features.First().Id },
                GenreIds = new List<Guid> { GameDatabase.Genres.First().Id },
                PlatformIds = new List<Guid> { GameDatabase.Platforms.First().Id },
                RegionIds = new List<Guid> { GameDatabase.Regions.First().Id },
                SeriesIds = new List<Guid> { GameDatabase.Series.First().Id },
                TagIds = new List<Guid> { GameDatabase.Tags.First().Id },
            };

            GameB = new Game("Game Beta")
            {
                Version = "1.5.1",
                ReleaseDate = new ReleaseDate(2023, 3, 26),
                Added = now - TimeSpan.FromDays(6),
                LastActivity = now - TimeSpan.FromDays(6),
                Modified = now - TimeSpan.FromDays(6),
                Playtime = secondsFromHours(5D),
                InstallSize = 0x40000001, //over 1GB
                CommunityScore = 51,
                CriticScore = 51,
                UserScore = 51,
                Favorite = false,
                IsInstalled = false,
                PluginId = XboxPluginId,
                SourceId = GameDatabase.Sources.Last().Id,
                CompletionStatusId = played.Id,
                AgeRatingIds = new List<Guid> { GameDatabase.AgeRatings.Last().Id },
                CategoryIds = new List<Guid> { GameDatabase.Categories.Last().Id },
                DeveloperIds = new List<Guid> { GameDatabase.Companies.Last().Id },
                PublisherIds = new List<Guid> { GameDatabase.Companies.Last().Id },
                FeatureIds = new List<Guid> { GameDatabase.Features.Last().Id },
                GenreIds = new List<Guid> { GameDatabase.Genres.Last().Id },
                PlatformIds = new List<Guid> { GameDatabase.Platforms.Last().Id },
                RegionIds = new List<Guid> { GameDatabase.Regions.Last().Id },
                SeriesIds = new List<Guid> { GameDatabase.Series.Last().Id },
                TagIds = new List<Guid> { GameDatabase.Tags.Last().Id },
            };

            GameBoth = new Game("Game Both")
            {
                Version = "alpha-2",
                ReleaseDate = new ReleaseDate(2025, 8, 26),
                Added = now.Date.AddMinutes(1),
                LastActivity = now.Date.AddMinutes(1),
                Modified = now.Date.AddMinutes(1),
                Playtime = secondsFromHours(50D),
                InstallSize = 0x280000001, //over 10GB
                CommunityScore = 100,
                CriticScore = 100,
                UserScore = 100,
                Favorite = true,
                IsInstalled = true,
                PluginId = Guid.Empty,
                SourceId = Guid.Empty,
                CompletionStatusId = beaten.Id,
                AgeRatingIds = GameDatabase.AgeRatings.Select(x => x.Id).ToList(),
                CategoryIds = GameDatabase.Categories.Select(x => x.Id).ToList(),
                DeveloperIds = GameDatabase.Companies.Select(x => x.Id).ToList(),
                PublisherIds = GameDatabase.Companies.Select(x => x.Id).ToList(),
                FeatureIds = GameDatabase.Features.Select(x => x.Id).ToList(),
                GenreIds = GameDatabase.Genres.Select(x => x.Id).ToList(),
                PlatformIds = GameDatabase.Platforms.Select(x => x.Id).ToList(),
                RegionIds = GameDatabase.Regions.Select(x => x.Id).ToList(),
                SeriesIds = GameDatabase.Series.Select(x => x.Id).ToList(),
                TagIds = GameDatabase.Tags.Select(x => x.Id).ToList(),
            };

            GameHidden = new Game("Game Hidden") { Hidden = true };

            GameDatabase.Games.Add(new[] { GameNone, GameA, GameB, GameBoth, GameHidden });
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            GameNone = null;
            GameA = null;
            GameB = null;
            GameBoth = null;
            GameHidden = null;
            GameDatabase.Dispose();
            TempDirectory.Dispose();
        }

        [Test]
        public void SingleValueFieldsEmpty()
        {
            var filterSettings = new FilterSettings
            {
                ReleaseYear = new StringFilterItemProperties(FilterSettings.MissingFieldString),
                Added = GetTodayFilter(), //Added, Modified, and RecentActivity are automatically set
                RecentActivity = GetTodayFilter(),
                Modified = GetTodayFilter(),
                LastActivity = new EnumFilterItemProperties((int)PastTimeSegment.Never),
                PlayTime = new EnumFilterItemProperties((int)PlaytimeCategory.NotPlayed),
                InstallSize = new EnumFilterItemProperties((int)InstallSizeGroup.None),
                CommunityScore = new EnumFilterItemProperties((int)ScoreGroup.None),
                CriticScore = new EnumFilterItemProperties((int)ScoreGroup.None),
                UserScore = new EnumFilterItemProperties((int)ScoreGroup.None),
                Library = new IdItemFilterItemProperties(Guid.Empty),
                Source = new IdItemFilterItemProperties(Guid.Empty),
                CompletionStatuses = new IdItemFilterItemProperties(Guid.Empty),
            };

            var filteredGames = GameDatabase.GetFilteredGames(filterSettings, useFuzzyNameMatch: false).ToList();

            AssertCollectionsContentsAreEqual(filteredGames, GameNone);
        }

        [Test]
        public void SingleValueFieldsIgnoreANDFilteringStyle()
        {
            var scoreFilter = new EnumFilterItemProperties(new List<int> { (int)ScoreGroup.None, (int)ScoreGroup.O1x, (int)ScoreGroup.O5x });
            var filterSettings = new FilterSettings
            {
                ReleaseYear = new StringFilterItemProperties(new List<string> { FilterSettings.MissingFieldString, "2015", "2023" }),
                Added = GetTodayFilter(), //Added, Modified, and RecentActivity are automatically set
                RecentActivity = GetTodayFilter(),
                Modified = GetTodayFilter(),
                LastActivity = new EnumFilterItemProperties(new List<int> { (int)PastTimeSegment.Never, (int)PastTimeSegment.PastYear, (int)PastTimeSegment.PastWeek }),
                PlayTime = new EnumFilterItemProperties(new List<int> { (int)PlaytimeCategory.NotPlayed, (int)PlaytimeCategory.LessThenHour, (int)PlaytimeCategory.O1_10 }),
                InstallSize = new EnumFilterItemProperties(new List<int> { (int)InstallSizeGroup.None, (int)InstallSizeGroup.S0_0MB_100MB, (int)InstallSizeGroup.S2_1GB_5GB }),
                CommunityScore = scoreFilter,
                CriticScore = scoreFilter,
                UserScore = scoreFilter,
                Library = new IdItemFilterItemProperties(new List<Guid> { Guid.Empty, SteamPluginId, XboxPluginId }),
                Source = new IdItemFilterItemProperties(new List<Guid>(GameDatabase.Sources.Select(x => x.Id)) { Guid.Empty }),
                CompletionStatuses = new IdItemFilterItemProperties(new List<Guid>(GameDatabase.CompletionStatuses.Select(x => x.Id)) { Guid.Empty }),
                UseAndFilteringStyle = true,
            };

            var filteredGames = GameDatabase.GetFilteredGames(filterSettings, useFuzzyNameMatch: false).ToList();

            AssertCollectionsContentsAreEqual(filteredGames, GameNone, GameA, GameB);
        }

        [Test]
        public void MultipleValueFieldsMatchAny()
        {
            var filterSettings = GetMultipleValueFieldFilterSettings();
            var filteredGames = GameDatabase.GetFilteredGames(filterSettings, useFuzzyNameMatch: false).ToList();

            AssertCollectionsContentsAreEqual(filteredGames, GameA, GameB, GameBoth);
        }

        [Test]
        public void MultipleValueFieldsMatchAll()
        {
            var filterSettings = GetMultipleValueFieldFilterSettings();
            filterSettings.UseAndFilteringStyle = true;

            var filteredGames = GameDatabase.GetFilteredGames(filterSettings, useFuzzyNameMatch: false).ToList();

            AssertCollectionsContentsAreEqual(filteredGames, GameBoth);
        }

        [Test]
        public void NameFilter()
        {
            void AssertNameMatch(string nameFilter, bool useFuzzyNameMatch, params Game[] expected)
            {
                var filterSettings = new FilterSettings { Name = nameFilter };
                var filteredGames = GameDatabase.GetFilteredGames(filterSettings, useFuzzyNameMatch).ToList();

                AssertCollectionsContentsAreEqual(filteredGames, expected);
            }

            AssertNameMatch("alpha", false, GameA);
            AssertNameMatch("alpha", true, GameA); //see if fuzzy filter matches exact strings too
            AssertNameMatch("!alpha", true, GameA); //! at the start of strings makes the matching exact instead of fuzzy
            AssertNameMatch("game alpga", true, GameA);
            AssertNameMatch("game alpga", false, expected: Array.Empty<Game>());
            AssertNameMatch("!game alpga", true, expected: Array.Empty<Game>());
            AssertNameMatch("^A", false, expected: Array.Empty<Game>()); //^ at the start of strings matches the name grouping character
            AssertNameMatch("^A", true, expected: Array.Empty<Game>());
            AssertNameMatch("^G", false, GameNone, GameA, GameB, GameBoth);
            AssertNameMatch("^G", true, GameNone, GameA, GameB, GameBoth);
        }

        [Test]
        public void InstallFilterEmpty()
        {
            void AssertAllGamesAreInFilter(FilterSettings filterSettings)
            {
                var filteredGames = GameDatabase.GetFilteredGames(filterSettings, useFuzzyNameMatch: false).ToList();

                AssertCollectionsContentsAreEqual(filteredGames, GameNone, GameA, GameB, GameBoth);
            }

            AssertAllGamesAreInFilter(new FilterSettings { IsInstalled = false, IsUnInstalled = false });
            AssertAllGamesAreInFilter(new FilterSettings { IsInstalled = true, IsUnInstalled = true });
        }

        [Test]
        public void FilterInstalled()
        {
            var filterSettings = new FilterSettings { IsInstalled = true };
            var filteredGames = GameDatabase.GetFilteredGames(filterSettings, useFuzzyNameMatch: false).ToList();

            AssertCollectionsContentsAreEqual(filteredGames, GameA, GameBoth);
        }

        [Test]
        public void FilterUninstalled()
        {
            var filterSettings = new FilterSettings { IsUnInstalled = true };
            var filteredGames = GameDatabase.GetFilteredGames(filterSettings, useFuzzyNameMatch: false).ToList();

            AssertCollectionsContentsAreEqual(filteredGames, GameNone, GameB);
        }

        [Test]
        public void FilterFavorite()
        {
            var filterSettings = new FilterSettings { Favorite = true };
            var filteredGames = GameDatabase.GetFilteredGames(filterSettings, useFuzzyNameMatch: false).ToList();

            AssertCollectionsContentsAreEqual(filteredGames, GameA, GameBoth);
        }

        [Test]
        public void FilterHidden()
        {
            var filterSettings = new FilterSettings { Hidden = true };
            var filteredGames = GameDatabase.GetFilteredGames(filterSettings, useFuzzyNameMatch: false).ToList();

            AssertCollectionsContentsAreEqual(filteredGames, GameHidden);
        }

        [Test]
        public void FilterVersion()
        {
            var filterSettings = new FilterSettings { Version = "1." };
            var filteredGames = GameDatabase.GetFilteredGames(filterSettings, useFuzzyNameMatch: false).ToList();

            AssertCollectionsContentsAreEqual(filteredGames, GameA, GameB);
        }

        [Test]
        public void FilterMultipleValuesByName()
        {
            List<Game> FilterFeatures(string featureFilterText, bool and)
            {
                var filterSettings = new FilterSettings
                {
                    Feature = new IdItemFilterItemProperties(featureFilterText),
                    UseAndFilteringStyle = and,
                };
                return GameDatabase.GetFilteredGames(filterSettings, useFuzzyNameMatch: false).ToList();
            }

            // "player" matches both Singleplayer and Multiplayer, but counts as 1 condition, thus it matches all 3 of these games
            AssertCollectionsContentsAreEqual(FilterFeatures("player", true), GameA, GameB, GameBoth);
            AssertCollectionsContentsAreEqual(FilterFeatures("single, multi", true), GameBoth);

            AssertCollectionsContentsAreEqual(FilterFeatures("player", false), GameA, GameB, GameBoth);
            AssertCollectionsContentsAreEqual(FilterFeatures("single, multi", false), GameA, GameB, GameBoth);
        }

        [Test]
        public void FilterSingleValuesByName()
        {
            List<Game> FilterCompletionStatus(string completionFilterText, bool and)
            {
                var filterSettings = new FilterSettings
                {
                    CompletionStatuses = new IdItemFilterItemProperties(completionFilterText),
                    UseAndFilteringStyle = and,
                };
                return GameDatabase.GetFilteredGames(filterSettings, useFuzzyNameMatch: false).ToList();
            }

            AssertCollectionsContentsAreEqual(FilterCompletionStatus("played", true), GameA, GameB);
            AssertCollectionsContentsAreEqual(FilterCompletionStatus("played", false), GameA, GameB);
            AssertCollectionsContentsAreEqual(FilterCompletionStatus("not played, beaten", true), GameA, GameBoth);
            AssertCollectionsContentsAreEqual(FilterCompletionStatus("not played, beaten", false), GameA, GameBoth);
        }

        private FilterSettings GetMultipleValueFieldFilterSettings()
        {
            IdItemFilterItemProperties getIdFilter(IEnumerable<DatabaseObject> items)
            {
                return new IdItemFilterItemProperties(items.Select(x => x.Id).ToList());
            }

            return new FilterSettings
            {
                AgeRating = getIdFilter(GameDatabase.AgeRatings),
                Category = getIdFilter(GameDatabase.Categories),
                Developer = getIdFilter(GameDatabase.Companies),
                Publisher = getIdFilter(GameDatabase.Companies),
                Feature = getIdFilter(GameDatabase.Features),
                Genre = getIdFilter(GameDatabase.Genres),
                Platform = getIdFilter(GameDatabase.Platforms),
                Region = getIdFilter(GameDatabase.Regions),
                Series = getIdFilter(GameDatabase.Series),
                Tag = getIdFilter(GameDatabase.Tags),
            };
        }

        private static EnumFilterItemProperties GetTodayFilter() => new EnumFilterItemProperties((int)PastTimeSegment.Today);

        /// <summary>
        /// Assert that the items in two collections are the same, in any order
        /// </summary>
        /// <param name="actual"></param>
        /// <param name="expected"></param>
        /// <typeparam name="T"></typeparam>
        private static void AssertCollectionsContentsAreEqual<T>(List<T> actual, params T[] expected)
        {
            foreach (var x in expected)
            {
                Assert.Contains(x, actual);
            }

            Assert.AreEqual(expected.Length, actual.Count);
        }
    }
}