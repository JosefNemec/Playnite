using NUnit.Framework;
using Playnite;
using Playnite.Database;
using Playnite.SDK.Models;
using Playnite.Settings;
using System;
using System.Collections.Generic;
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
    }
}
