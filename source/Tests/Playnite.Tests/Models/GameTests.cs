using NUnit.Framework;
using Playnite;
using Playnite.Database;
using Playnite.SDK.Models;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests.Models
{
    [TestFixture]
    public class GameTests
    {
        [Test]
        public void ExpandVariablesTest()
        {
            var database = new InMemoryGameDatabase();
            Game.DatabaseReference = database;
            GameDatabase.GenerateSampleData(database);

            var dir = @"c:\test\test2\";
            var game = new Game()
            {
                Name = "test game",
                InstallDirectory = dir,
                GameImagePath = Path.Combine(dir, "test.iso"),
                PlatformId = database.Platforms.First().Id,
                Version = "1.0",
                PluginId = Guid.NewGuid(),
                GameId = "game_id",
                Id = Guid.NewGuid()
            };

            Assert.AreEqual(string.Empty, game.ExpandVariables(string.Empty));
            Assert.AreEqual("teststring", game.ExpandVariables("teststring"));
            Assert.AreEqual(dir + "teststring", game.ExpandVariables("{InstallDir}teststring"));
            Assert.AreEqual(game.InstallDirectory, game.ExpandVariables("{InstallDir}"));
            Assert.AreEqual(game.GameImagePath, game.ExpandVariables("{ImagePath}"));
            Assert.AreEqual("test", game.ExpandVariables("{ImageNameNoExt}"));
            Assert.AreEqual("test.iso", game.ExpandVariables("{ImageName}"));
            Assert.AreEqual(PlaynitePaths.ProgramPath, game.ExpandVariables("{PlayniteDir}"));
            Assert.AreEqual("test game", game.ExpandVariables("{Name}"));
            Assert.AreEqual("test2", game.ExpandVariables("{InstallDirName}"));
            Assert.AreEqual(game.Platform.Name, game.ExpandVariables("{Platform}"));
            Assert.AreEqual(game.PluginId.ToString(), game.ExpandVariables("{PluginId}"));
            Assert.AreEqual(game.GameId, game.ExpandVariables("{GameId}"));
            Assert.AreEqual(game.Id.ToString(), game.ExpandVariables("{DatabaseId}"));
            Assert.AreEqual(game.Version, game.ExpandVariables("{Version}"));
        }

        [Test]
        public void ExpandVariablesEmptyTest()
        {
            // Should not throw
            var game = new Game();
            game.ExpandVariables(string.Empty);
            game.ExpandVariables(null);
        }

        [Test]
        public void GameIdTest()
        {
            var game1 = new Game();
            Assert.IsFalse(string.IsNullOrEmpty(game1.GameId));
            Assert.AreNotEqual(game1.GameId, new Game().GameId);
        }

        [Test]
        public void CopyDiffToTest()
        {
            var game = new Game()
            {
                Name = "Name",
                InstallDirectory = "InstallDirectory",
                GameImagePath = "GameImagePath",
                PlatformId = new Guid(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1),
                Version = "Version",
                PluginId = new Guid(2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2),
                GameId = "GameId",
                Id = new Guid(3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3),
                Added = new DateTime(10),
                AgeRatingId = new Guid(4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4),
                BackgroundImage = "BackgroundImage",
                CategoryIds = new List<Guid> { new Guid(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1) },
                CommunityScore = 10,
                CompletionStatus = CompletionStatus.Beaten,
                CoverImage = "CoverImage",
                CriticScore = 20,
                Description = "Description",
                DeveloperIds = new List<Guid> { new Guid(2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2) },
                Favorite = false,
                GenreIds = new List<Guid> { new Guid(3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3) },
                Hidden = false,
                Icon = "Icon",
                IsInstalled = false,
                IsInstalling = false,
                IsLaunching = false,
                IsRunning = false,
                IsUninstalling = false,
                LastActivity = new DateTime(20),
                Links = new ObservableCollection<Link> { new Link("1", "2") },
                Modified = new DateTime(30),
                PlayCount = 1,
                Playtime = 10,
                PublisherIds = new List<Guid> { new Guid(4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4) },
                RegionId = new Guid(5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5),
                ReleaseDate = new DateTime(40),
                SeriesId = new Guid(6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6),
                SortingName = "SortingName",
                SourceId = new Guid(7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7),
                TagIds = new List<Guid> { new Guid(5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5) },
                FeatureIds = new List<Guid> { new Guid(5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5) },
                UserScore = 30,
                PlayAction = new GameAction(),
                OtherActions = new ObservableCollection<GameAction> { new GameAction() }
            };

            Assert.Fail();
        }
    }
}
