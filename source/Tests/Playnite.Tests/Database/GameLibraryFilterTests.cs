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
            GameDatabase = new GameDatabase(TempDirectory.TempPath);
            GameDatabase.OpenDatabase();
            GameDatabase.AgeRatings.Add("Everyone");
            GameDatabase.AgeRatings.Add("Mature");
            GameDatabase.Categories.Add("Has cats");
            GameDatabase.Categories.Add("Has dogs");
            GameDatabase.Companies.Add("iD Software");
            GameDatabase.Companies.Add("Bethesda");
            GameDatabase.CompletionStatuses.Add("Not played");
            GameDatabase.CompletionStatuses.Add("Played");
            GameDatabase.CompletionStatuses.Add("Beaten");
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
            };

            GameA = new Game("Game Alpha")
            {
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
                AgeRatingIds = new List<Guid> { GameDatabase.AgeRatings.First().Id },
                CategoryIds = new List<Guid> { GameDatabase.Categories.First().Id },
                DeveloperIds = new List<Guid> { GameDatabase.Companies.First().Id },
                PublisherIds = new List<Guid> { GameDatabase.Companies.First().Id },
                FeatureIds = new List<Guid> { GameDatabase.Features.First().Id },
                GenreIds = new List<Guid> { GameDatabase.Genres.First().Id },
                PlatformIds = new List<Guid> { GameDatabase.Platforms.First().Id },
                RegionIds = new List<Guid> { GameDatabase.Regions.First().Id },
                SeriesIds = new List<Guid> { GameDatabase.Series.First().Id },
                TagIds = new List<Guid> { GameDatabase.Tags.First().Id }
            };

            GameB = new Game("Game Beta")
            {
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
            GameDatabase.Dispose();
            TempDirectory.Dispose();
        }

        [Test]
        public void SingleValueFieldsEmpty()
        {
            var filterSettings = new FilterSettings
            {
                ReleaseYear = new StringFilterItemProperties(FilterSettings.MissingFieldString),
                Added = new EnumFilterItemProperties((int)PastTimeSegment.Today), //Added, Modified, and RecentActivity are automatically set
                RecentActivity = new EnumFilterItemProperties((int)PastTimeSegment.Today),
                Modified = new EnumFilterItemProperties((int)PastTimeSegment.Today),
                LastActivity = new EnumFilterItemProperties((int)PastTimeSegment.Never),
                PlayTime = new EnumFilterItemProperties((int)PlaytimeCategory.NotPlayed),
                InstallSize = new EnumFilterItemProperties((int)InstallSizeGroup.None),
                CommunityScore = new EnumFilterItemProperties((int)ScoreGroup.None),
                CriticScore = new EnumFilterItemProperties((int)ScoreGroup.None),
                UserScore = new EnumFilterItemProperties((int)ScoreGroup.None),
                Library = new IdItemFilterItemProperties(Guid.Empty),
                Source = new IdItemFilterItemProperties(Guid.Empty),
            };

            var filteredGames = GameDatabase.GetFilteredGames(filterSettings, useFuzzyNameMatch: false).ToList();

            Assert.AreEqual(1, filteredGames.Count);
            Assert.AreEqual(GameNone, filteredGames[0]);
        }
    }
}