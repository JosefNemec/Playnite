using NUnit.Framework;
using Playnite;
using Playnite.Database;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.Database
{
    [TestFixture]
    public class GameDatabaseEventTests
    {
        private string dbPath = Path.Combine(PlayniteTests.TempPath, "events.db");
        private GameDatabase db;
        [SetUp]
        public void Init()
        {
            FileSystem.DeleteFile(dbPath);
            db = new GameDatabase(null);
            db.OpenDatabase(dbPath);
        }

        [TearDown]
        public void Dispose()
        {
            db.CloseDatabase();
        }

        [Test]
        public void EventsArgsNonBufferedTest()
        {
            GamesCollectionChangedEventArgs gameColArgs = null;
            GameUpdatedEventArgs gameUpdateArgs = null;
            PlatformsCollectionChangedEventArgs platformColArgs = null;
            PlatformUpdatedEventArgs platformUpdateArgs = null;
            db.GamesCollectionChanged += (e, args) => { gameColArgs = args; };
            db.GameUpdated += (e, args) => { gameUpdateArgs = args; };
            db.PlatformsCollectionChanged += (e, args) => { platformColArgs = args; };
            db.PlatformUpdated += (e, args) => { platformUpdateArgs = args; };

            var game = new Game("test game");
            db.AddGame(game);
            Assert.AreEqual(1, gameColArgs.AddedGames.Count);
            Assert.AreEqual(game, gameColArgs.AddedGames[0]);
            Assert.AreEqual(0, gameColArgs.RemovedGames.Count);
            db.UpdateGameInDatabase(game);
            Assert.AreEqual(1, gameUpdateArgs.UpdatedGames.Count);
            Assert.AreEqual(game, gameUpdateArgs.UpdatedGames[0].NewData);
            Assert.AreNotEqual(game, gameUpdateArgs.UpdatedGames[0].OldData);
            db.DeleteGame(game);
            Assert.AreEqual(0, gameColArgs.AddedGames.Count);
            Assert.AreEqual(1, gameColArgs.RemovedGames.Count);
            Assert.AreEqual(game, gameColArgs.RemovedGames[0]);

            var platform = new Platform("test platform");
            db.AddPlatform(platform);
            Assert.AreEqual(1, platformColArgs.AddedPlatforms.Count);
            Assert.AreEqual(platform, platformColArgs.AddedPlatforms[0]);
            var platform2 = new Platform("test platform2");
            db.AddPlatform(new List<Platform> { platform2 });
            Assert.AreEqual(1, platformColArgs.AddedPlatforms.Count);
            Assert.AreEqual(platform2, platformColArgs.AddedPlatforms[0]);
            db.UpdatePlatform(platform);
            Assert.AreEqual(1, platformUpdateArgs.UpdatedPlatforms.Count);
            Assert.AreEqual(platform, platformUpdateArgs.UpdatedPlatforms[0].NewData);
            Assert.AreNotEqual(platform, platformUpdateArgs.UpdatedPlatforms[0].OldData);
            db.RemovePlatform(platform);
            Assert.AreEqual(1, platformColArgs.RemovedPlatforms.Count);
            Assert.AreEqual(platform, platformColArgs.RemovedPlatforms[0]);
        }

        [Test]
        public void EventsArgsBufferedTest()
        {
            GamesCollectionChangedEventArgs gameColArgs = null;
            GameUpdatedEventArgs gameUpdateArgs = null;
            PlatformsCollectionChangedEventArgs platformColArgs = null;
            PlatformUpdatedEventArgs platformUpdateArgs = null;
            db.GamesCollectionChanged += (e, args) => { gameColArgs = args; };
            db.GameUpdated += (e, args) => { gameUpdateArgs = args; };
            db.PlatformsCollectionChanged += (e, args) => { platformColArgs = args; };
            db.PlatformUpdated += (e, args) => { platformUpdateArgs = args; };

            using (db.BufferedUpdate())
            {
                var game = new Game("test game");
                db.AddGame(game);
                db.UpdateGameInDatabase(game);
                db.UpdateGameInDatabase(game);
                db.DeleteGame(game);
                Assert.IsNull(gameColArgs);
                Assert.IsNull(gameUpdateArgs);

                var platform = new Platform("test platform");
                var platform2 = new Platform("test platform2");
                db.AddPlatform(platform);
                db.AddPlatform(new List<Platform> { platform2 });
                db.UpdatePlatform(platform);
                db.UpdatePlatform(platform2);
                db.UpdatePlatform(platform2);
                db.RemovePlatform(platform);
                Assert.IsNull(platformColArgs);
                Assert.IsNull(platformUpdateArgs);
            }

            Assert.AreEqual(1, gameColArgs.AddedGames.Count);
            Assert.AreEqual(1, gameColArgs.RemovedGames.Count);
            Assert.AreEqual(2, gameUpdateArgs.UpdatedGames.Count);

            Assert.AreEqual(2, platformColArgs.AddedPlatforms.Count);
            Assert.AreEqual(1, platformColArgs.RemovedPlatforms.Count);
            Assert.AreEqual(3, platformUpdateArgs.UpdatedPlatforms.Count);
        }

        [Test]
        public void EventsInvokeCountNonBufferedTest()
        {
            var gameUpdates = 0;
            var gameColUpdates = 0;
            var platformUpdates = 0;
            var platformColUpdates = 0;
            db.GamesCollectionChanged += (e, args) => { gameColUpdates++; };
            db.GameUpdated += (e, args) => { gameUpdates++; };
            db.PlatformsCollectionChanged += (e, args) => { platformColUpdates++; };
            db.PlatformUpdated += (e, args) => { platformUpdates++; };

            var game = new Game("test game");
            db.AddGame(game);
            Assert.AreEqual(0, gameUpdates);
            Assert.AreEqual(1, gameColUpdates);
            db.UpdateGameInDatabase(game);
            Assert.AreEqual(1, gameUpdates);
            Assert.AreEqual(1, gameColUpdates);
            db.UpdateGameInDatabase(game);
            db.UpdateGameInDatabase(game);
            Assert.AreEqual(3, gameUpdates);
            Assert.AreEqual(1, gameColUpdates);
            db.DeleteGame(game);
            Assert.AreEqual(3, gameUpdates);
            Assert.AreEqual(2, gameColUpdates);

            var platform = new Platform("test platform");
            var platform2 = new Platform("test platform2");
            db.AddPlatform(platform);
            Assert.AreEqual(1, platformColUpdates);
            Assert.AreEqual(0, platformUpdates);
            db.AddPlatform(new List<Platform> { platform2 });
            Assert.AreEqual(2, platformColUpdates);
            Assert.AreEqual(0, platformUpdates);
            db.UpdatePlatform(platform);
            Assert.AreEqual(2, platformColUpdates);
            Assert.AreEqual(1, platformUpdates);
            db.UpdatePlatform(platform2);
            db.UpdatePlatform(platform2);
            Assert.AreEqual(2, platformColUpdates);
            Assert.AreEqual(3, platformUpdates);
            db.RemovePlatform(platform);
            Assert.AreEqual(3, platformColUpdates);
            Assert.AreEqual(3, platformUpdates);
        }

        [Test]
        public void EventsInvokeCountBufferedTest()
        {
            var gameUpdates = 0;
            var gameColUpdates = 0;
            var platformUpdates = 0;
            var platformColUpdates = 0;
            db.GamesCollectionChanged += (e, args) => { gameColUpdates++; };
            db.GameUpdated += (e, args) => { gameUpdates++; };
            db.PlatformsCollectionChanged += (e, args) => { platformColUpdates++; };
            db.PlatformUpdated += (e, args) => { platformUpdates++; };

            using (db.BufferedUpdate())
            {
                var game = new Game("test game");
                db.AddGame(game);
                db.UpdateGameInDatabase(game);
                db.UpdateGameInDatabase(game);
                db.DeleteGame(game);
                Assert.AreEqual(0, gameUpdates);
                Assert.AreEqual(0, gameColUpdates);

                var platform = new Platform("test platform");
                var platform2 = new Platform("test platform2");
                db.AddPlatform(platform);
                db.AddPlatform(new List<Platform> { platform2 });
                db.UpdatePlatform(platform);
                db.UpdatePlatform(platform2);
                db.UpdatePlatform(platform2);
                db.RemovePlatform(platform);
                Assert.AreEqual(0, platformColUpdates);
                Assert.AreEqual(0, platformUpdates);
            }

            Assert.AreEqual(1, gameUpdates);
            Assert.AreEqual(1, gameColUpdates);
            Assert.AreEqual(1, platformColUpdates);
            Assert.AreEqual(1, platformUpdates);
        }
    }
}
