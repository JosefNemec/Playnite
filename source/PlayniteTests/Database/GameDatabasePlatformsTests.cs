using Moq;
using NUnit.Framework;
using Playnite;
using Playnite.Database;
using Playnite.Models;
using Playnite.Providers.Steam;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.Database
{
    [TestFixture]
    public class GameDatabasePlatformsTests
    {
        private string dbPath = Path.Combine(Playnite.PlayniteTests.TempPath, "platformstest.db");

        [SetUp]
        public void Init()
        {
            FileSystem.DeleteFile(dbPath);
        }

        [Test]
        public void PlatformRemovalTest()
        {
            var db = new GameDatabase();
            using (db.OpenDatabase(dbPath))
            {
                var platform = new Platform("Test");
                db.AddPlatform(platform);
                var game = new Game("Test")
                {
                    PlatformId = platform.Id
                };

                db.AddGame(game);
                db.RemovePlatform(platform);
                var dbGame = db.GamesCollection.FindById(game.Id);
                Assert.IsNull(dbGame.PlatformId);
                CollectionAssert.IsEmpty(db.PlatformsCollection.FindAll());
            }
        }

        [Test]
        public void PcPlatformAutoAssignTest()
        {
            var libraryGames = new List<IGame>()
            {
                new Game()
                {
                    ProviderId = "testid",
                    Name = "Test Game",
                    Provider = Provider.Steam
                },
                new Game()
                {
                    ProviderId = "testid2",
                    Name = "Test Game 2",
                    Provider = Provider.Steam
                }
            };

            var steamLibrary = new Mock<ISteamLibrary>();
            steamLibrary.Setup(oc => oc.GetLibraryGames(string.Empty)).Returns(libraryGames);

            var db = new GameDatabase(null, steamLibrary.Object, null, null);
            using (db.OpenDatabase(dbPath))
            {
                db.UpdateOwnedGames(Provider.Steam);
                var platforms = db.PlatformsCollection.FindAll().ToList();
                Assert.AreEqual(1, platforms.Count());
                Assert.AreEqual("PC", platforms[0].Name);

                var games = db.GamesCollection.FindAll().ToList();
                Assert.AreEqual(platforms[0].Id, games[0].PlatformId);
                Assert.AreEqual(platforms[0].Id, games[1].PlatformId);
            }
        }
    }
}
