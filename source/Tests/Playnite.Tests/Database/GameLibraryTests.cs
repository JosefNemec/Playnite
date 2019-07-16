using Moq;
using NUnit.Framework;
using Playnite.Common;
using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Playnite.Tests.Database
{
    [TestFixture]
    public class GameLibraryTests
    {
        private Mock<LibraryPlugin> libraryPluginMock;

        [SetUp]
        public void SetUp()
        {
            var playniteApiMock = new Mock<IPlayniteAPI>();
            libraryPluginMock = new Mock<LibraryPlugin>(playniteApiMock.Object);
            libraryPluginMock.Setup(a => a.Id).Returns(Guid.NewGuid());
        }

        [Test]
        public void PlaytimeImport_Test()
        {
            var gameId = "testId";
            var timeToImport = 500;
            libraryPluginMock.Setup(a => a.GetGames()).Returns(() => new List<GameInfo>
            {
                new GameInfo
                {
                    GameId = gameId,
                    Playtime = timeToImport
                }
            });

            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                db.ImportGames(libraryPluginMock.Object, true);
                Assert.AreEqual(timeToImport, db.Games.First().Playtime);

                timeToImport = 600;
                db.ImportGames(libraryPluginMock.Object, false);
                Assert.AreEqual(500, db.Games.First().Playtime);
                db.ImportGames(libraryPluginMock.Object, true);
                Assert.AreEqual(timeToImport, db.Games.First().Playtime);

                var g = db.Games.First();
                g.Playtime = 0;
                db.Games.Update(g);
                Assert.AreEqual(0, db.Games.First().Playtime);
                db.ImportGames(libraryPluginMock.Object, false);
                Assert.AreEqual(timeToImport, db.Games.First().Playtime);
            }
        }

        [Test]
        public void SetUninstalledTagOnGames_InstalledGamesSync_Test()
        {
            const string installedGameId = "installedGame";
            libraryPluginMock.Setup(a => a.GetGames()).Returns(() => new List<GameInfo>
            {
                new GameInfo { GameId = installedGameId, IsInstalled = true }
            });

            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                db.Games.Add(new Game { GameId = installedGameId, PluginId = libraryPluginMock.Object.Id, IsInstalled = true });
                db.Games.Add(new Game { GameId = "uninstalledGame", PluginId = libraryPluginMock.Object.Id, IsInstalled = true });
                db.ImportGames(libraryPluginMock.Object, false);
                Assert.AreEqual(1, db.Games.Count(g => g.IsInstalled));
                Assert.AreEqual(db.Games.First(g => g.IsInstalled).GameId, installedGameId);
            }
        }

        [Test]
        public void SetUninstalledTagOnGames_UninstalledGamesSync_Test()
        {
            const string installedGameId = "installedGame";
            const string uninstalledGameId = "uninstalledGame";
            libraryPluginMock.Setup(a => a.GetGames()).Returns(() => new List<GameInfo>
            {
                new GameInfo { GameId = installedGameId, IsInstalled = true },
                new GameInfo { GameId = uninstalledGameId, IsInstalled = false }
            });

            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                db.Games.Add(new Game { GameId = installedGameId, PluginId = libraryPluginMock.Object.Id, IsInstalled = true });
                db.Games.Add(new Game { GameId = uninstalledGameId, PluginId = libraryPluginMock.Object.Id, IsInstalled = true });
                db.ImportGames(libraryPluginMock.Object, false);
                Assert.AreEqual(1, db.Games.Count(g => g.IsInstalled));
                Assert.AreEqual(db.Games.First(g => g.IsInstalled).GameId, installedGameId);
            }
        }

        [Test]
        public void ReinstalledGamesSetAsInstalled_Test()
        {
            const string installedGame1Id = "installedGame1";
            const string installedGame2Id = "installedGame2";
            libraryPluginMock.Setup(a => a.GetGames()).Returns(() => new List<GameInfo>
            {
                new GameInfo { GameId = installedGame1Id, IsInstalled = true },
                new GameInfo { GameId = installedGame2Id, IsInstalled = true }
            });

            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                db.Games.Add(new Game { GameId = installedGame1Id, PluginId = libraryPluginMock.Object.Id, IsInstalled = true });
                db.Games.Add(new Game { GameId = installedGame2Id, PluginId = libraryPluginMock.Object.Id, IsInstalled = false });
                db.ImportGames(libraryPluginMock.Object, false);
                Assert.AreEqual(2, db.Games.Count(g => g.IsInstalled));
                Assert.AreEqual(db.Games.First(g => g.GameId == installedGame2Id).IsInstalled, true);
            }
        }
    }
}