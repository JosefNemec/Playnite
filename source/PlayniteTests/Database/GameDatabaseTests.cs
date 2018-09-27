using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Database;
using Playnite.SDK.Models;
using Playnite;
using Moq;
using NUnit.Framework;

namespace PlayniteTests.Database
{
    [TestFixture]
    public class GameDatabaseTests
    {
        [OneTimeSetUp]
        public void Init()
        {
            // Some test are reading resources, which cannot be access until pack:// namespace is initialized
            // http://stackoverflow.com/questions/6005398/uriformatexception-invalid-uri-invalid-port-specified
            string s = System.IO.Packaging.PackUriHelper.UriSchemePack;
        }

        [Test]
        public void ListUpdateTest()
        {
            var path = Path.Combine(PlayniteTests.TempPath, "updatedb.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase(null);
            using (db.OpenDatabase(path))
            {
                db.AddGame(new Game()
                {
                    GameId = "testid",
                    Name = "Test Game"
                });

                db.AddGame(new Game()
                {
                    GameId = "testid2",
                    Name = "Test Game 2"
                });

                Assert.AreEqual(2, db.GamesCollection.Count());
            }

            db = new GameDatabase(null);
            using (db.OpenDatabase(path))
            {
                Assert.AreEqual(2, db.GamesCollection.Count());
                db.AddGame(new Game()
                {
                    GameId = "testid3",
                    Name = "Test Game 3"
                });

                var games = db.GamesCollection.FindAll().ToList();
                games[2].Name = "Changed Name";
                db.UpdateGameInDatabase(games[2]);
            }

            db = new GameDatabase(null);
            using (db.OpenDatabase(path))
            {
                var games = db.GamesCollection.FindAll().ToList();
                Assert.AreEqual(3, games.Count);
                Assert.AreEqual("Changed Name", games[2].Name);
                db.DeleteGame(games[1]);
            }

            db = new GameDatabase(null);
            using (db.OpenDatabase(path))
            {
                Assert.AreEqual(2, db.GamesCollection.Count());
            }
        }

        [Test]
        public void DeleteImageSafeTest()
        {
            var path = Path.Combine(PlayniteTests.TempPath, "deleteimagetest.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase(null);
            using (db.OpenDatabase(path))
            {
                db.AddFile("testimage", "testimage.png", new byte[] { 0 });
                Assert.AreEqual(1, db.Database.FileStorage.FindAll().Count());

                db.AddGame(new Game()
                {
                    GameId = "testid",
                    Name = "Test Game",
                    Icon = "testimage"
                });

                db.AddGame(new Game()
                {
                    GameId = "testid2",
                    Name = "Test Game 2",
                    Icon = "testimage"
                });

                // Doesn't remove image in use
                var games = db.GamesCollection.FindAll().ToList();
                db.DeleteImageSafe("testimage", games[0]);
                Assert.AreEqual(1, db.Database.FileStorage.FindAll().Count());

                // Removes image
                games[1].Icon = string.Empty;
                db.UpdateGameInDatabase(games[1]);
                db.DeleteImageSafe("testimage", games[0]);
                Assert.AreEqual(0, db.Database.FileStorage.FindAll().Count());
            }
        }

        [Test]
        public void DeleteGameImageCleanupTest()
        {
            var path = Path.Combine(PlayniteTests.TempPath, "deleteimagecleanuptest.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase(null);
            using (db.OpenDatabase(path))
            {
                db.AddFile("testimage", "testimage.png", new byte[] { 0 });
                db.AddFile("testicon", "testicon.png", new byte[] { 0 });
                Assert.AreEqual(2, db.Database.FileStorage.FindAll().Count());

                var game = new Game()
                {
                    GameId = "testid",
                    Name = "Test Game",
                    Icon = "testicon",
                    CoverImage = "testimage"
                };

                db.AddGame(game);
                db.DeleteGame(game);

                Assert.AreEqual(0, db.Database.FileStorage.FindAll().Count());
            }
        }          
    }
}
