using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Playnite.Database;
using Playnite.Models;
using Playnite;

namespace PlayniteTests.Database
{
    [TestClass()]
    public class GameDatabaseTests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            // Some test are reading resources, which cannot be access until pack:// namespace is initialized
            // http://stackoverflow.com/questions/6005398/uriformatexception-invalid-uri-invalid-port-specified
            string s = System.IO.Packaging.PackUriHelper.UriSchemePack;
        }

        [TestMethod]
        public void ListUpdateTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "updatedb.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path, true))
            {
                db.AddGame(new Game()
                {
                    ProviderId = "testid",
                    Name = "Test Game"
                });

                db.AddGame(new Game()
                {
                    ProviderId = "testid",
                    Name = "Test Game 2"
                });

                Assert.AreEqual(2, db.Games.Count);
            }

            using (db.OpenDatabase(path, true))
            {
                Assert.AreEqual(2, db.Games.Count);
                db.AddGame(new Game()
                {
                    ProviderId = "testid",
                    Name = "Test Game 3"
                });

                db.Games[2].Name = "Changed Name";
                db.UpdateGame(db.Games[2]);
            }

            using (db.OpenDatabase(path, true))
            {
                Assert.AreEqual(3, db.Games.Count);
                Assert.AreEqual("Changed Name", db.Games[2].Name);
                db.DeleteGame(db.Games[1]);
            }

            using (db.OpenDatabase(path, true))
            {
                Assert.AreEqual(2, db.Games.Count);
            }
        }

        [TestMethod]
        public void DeleteImageSafeTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "deleteimagetest.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path, true))
            {
                db.AddImage("testimage", "testimage.png", new byte[] { 0 });
                Assert.AreEqual(1, db.Database.FileStorage.FindAll().Count());

                db.AddGame(new Game()
                {
                    ProviderId = "testid",
                    Name = "Test Game",
                    Icon = "testimage"
                });

                db.AddGame(new Game()
                {
                    ProviderId = "testid",
                    Name = "Test Game 2",
                    Icon = "testimage"
                });

                // Doesn't remove image in use
                db.DeleteImageSafe("testimage", db.Games[0]);

                // Removes image
                db.Games[1].Icon = string.Empty;
                db.UpdateGame(db.Games[1]);
                db.DeleteImageSafe("testimage", db.Games[0]);
                Assert.AreEqual(0, db.Database.FileStorage.FindAll().Count());
            }
        }

        [TestMethod]
        public void DeleteGameImageCleanupTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "deleteimagecleanuptest.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path, true))
            {
                db.AddImage("testimage", "testimage.png", new byte[] { 0 });
                db.AddImage("testicon", "testicon.png", new byte[] { 0 });
                Assert.AreEqual(2, db.Database.FileStorage.FindAll().Count());

                var game = new Game()
                {
                    ProviderId = "testid",
                    Name = "Test Game",
                    Icon = "testicon",
                    Image = "testimage"
                };

                db.AddGame(game);
                db.DeleteGame(game);

                Assert.AreEqual(0, db.Database.FileStorage.FindAll().Count());
            }
        }

        #region GOG
        [TestMethod]
        public void UpdateGogInstalledGames_CleanImportTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "goginstalledimportclean.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path, true))
            {
                db.UpdateGogInstalledGames();
                Assert.AreNotEqual(0, db.Games.Count);
            }
        }

        [TestMethod]
        public void UpdateGogInstalledGames_UpdateImportTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "goginstalledimportupdate.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path, true))
            {
                db.UpdateGogInstalledGames();
                var game = db.Games[0];
                game.PlayTask = null;
                game.InstallDirectory = @"c:\nonsense\directory\";
                db.UpdateGame(game);
            }

            using (db.OpenDatabase(path, true))
            {
                var game = db.Games[0];
                Assert.IsNull(game.PlayTask);
                Assert.AreEqual(@"c:\nonsense\directory\", game.InstallDirectory);
                var gameCount = db.Games.Count;

                db.UpdateGogInstalledGames();
                Assert.AreEqual(gameCount, db.Games.Count);

                game = db.Games[0];
                Assert.IsNotNull(game.PlayTask);
                Assert.AreNotEqual(@"c:\nonsense\directory\", game.InstallDirectory);
            }
        }

        [TestMethod]
        public void UpdateGogGameWithMetadataTest()
        {
            var game = new Game()
            {
                ProviderId = "1207658890",
                Name = "Temp Name",
                Provider = Provider.GOG
            };

            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "gogmetaupdate.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path, true))
            {
                db.AddGame(game);
                db.UpdateGameWithMetadata(game);
                Assert.AreNotEqual("Temp Name", game.Name);
                Assert.IsNotNull(game.ReleaseDate);
                Assert.IsNotNull(game.Genres);
                Assert.IsNotNull(game.Developers);
                Assert.IsTrue(!string.IsNullOrEmpty(game.Description));
                Assert.IsTrue(!string.IsNullOrEmpty(game.CommunityHubUrl));
                Assert.IsTrue(!string.IsNullOrEmpty(game.StoreUrl));
                Assert.IsTrue(!string.IsNullOrEmpty(game.WikiUrl));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Icon));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Image));
                Assert.IsTrue(!string.IsNullOrEmpty(game.BackgroundImage));

                var files = db.Database.FileStorage.FindAll();
                Assert.AreEqual(2, files.Count());
            }
        }
        #endregion GOG

        #region Steam
        [TestMethod]
        public void UpdateSteamInstalledGames_CleanImportTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "steaminstalledimportclean.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path, true))
            {
                db.UpdateSteamInstalledGames();
                Assert.AreNotEqual(0, db.Games.Count);
            }
        }

        [TestMethod]
        public void UpdateSteamInstalledGames_UpdateImportTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "steaminstalledimportupdate.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path, true))
            {
                db.UpdateSteamInstalledGames();
                var game = db.Games[0];
                game.PlayTask = null;
                game.InstallDirectory = @"c:\nonsense\directory\";
                db.UpdateGame(game);
            }

            using (db.OpenDatabase(path, true))
            {
                var game = db.Games[0];
                Assert.IsNull(game.PlayTask);
                Assert.AreEqual(@"c:\nonsense\directory\", game.InstallDirectory);
                var gameCount = db.Games.Count;

                db.UpdateSteamInstalledGames();
                Assert.AreEqual(gameCount, db.Games.Count);

                game = db.Games[0];
                Assert.IsNotNull(game.PlayTask);
                Assert.AreNotEqual(@"c:\nonsense\directory\", game.InstallDirectory);
            }
        }

        [TestMethod]
        public void UpdateSteamGameWithMetadataTest()
        {
            var game = new Game()
            {
                ProviderId = "12150",
                Name = "Temp Name",
                Provider = Provider.Steam
            };

            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "steammetaupdate.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path, true))
            {
                db.AddGame(game);
                db.UpdateGameWithMetadata(game);
                Assert.AreNotEqual("Temp Name", game.Name);
                Assert.IsNotNull(game.ReleaseDate);
                Assert.IsNotNull(game.Genres);
                Assert.IsNotNull(game.Developers);
                Assert.IsTrue(!string.IsNullOrEmpty(game.Description));
                Assert.IsTrue(!string.IsNullOrEmpty(game.CommunityHubUrl));
                Assert.IsTrue(!string.IsNullOrEmpty(game.StoreUrl));
                Assert.IsTrue(!string.IsNullOrEmpty(game.WikiUrl));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Icon));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Image));
                Assert.IsTrue(!string.IsNullOrEmpty(game.BackgroundImage));

                var files = db.Database.FileStorage.FindAll();
                Assert.AreEqual(2, files.Count());
            }
        }
        #endregion Steam

        #region Origin
        [TestMethod]
        public void UpdateOriginInstalledGames_CleanImportTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "origininstalledimportclean.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path, true))
            {
                db.UpdateOriginInstalledGames();
                Assert.AreNotEqual(0, db.Games.Count);
            }
        }

        [TestMethod]
        public void UpdateOriginGameWithMetadataTest()
        {
            var game = new Game()
            {
                ProviderId = "DR:198070800",
                Name = "Temp Name",
                Provider = Provider.Origin
            };

            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "originmetaupdate.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path, true))
            {
                db.AddGame(game);
                db.UpdateGameWithMetadata(game);
                Assert.AreNotEqual("Temp Name", game.Name);
                Assert.IsNotNull(game.ReleaseDate);
                Assert.IsNotNull(game.Developers);
                Assert.IsTrue(!string.IsNullOrEmpty(game.Description));
                Assert.IsTrue(!string.IsNullOrEmpty(game.CommunityHubUrl));
                Assert.IsTrue(!string.IsNullOrEmpty(game.StoreUrl));
                Assert.IsTrue(!string.IsNullOrEmpty(game.WikiUrl));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Image));

                var files = db.Database.FileStorage.FindAll();
                Assert.AreEqual(1, files.Count());
            }
        }

        #endregion Origin
    }
}
