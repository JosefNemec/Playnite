using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Database;
using Playnite.Models;
using Playnite;
using Moq;
using Playnite.Providers.GOG;
using Playnite.Providers.Steam;
using Playnite.Providers.Origin;
using NUnit.Framework;
using LiteDB;

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
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "updatedb.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path))
            {
                db.AddGame(new Game()
                {
                    ProviderId = "testid",
                    Name = "Test Game"
                });

                db.AddGame(new Game()
                {
                    ProviderId = "testid2",
                    Name = "Test Game 2"
                });

                Assert.AreEqual(2, db.GamesCollection.Count());
            }

            db = new GameDatabase();
            using (db.OpenDatabase(path))
            {
                Assert.AreEqual(2, db.GamesCollection.Count());
                db.AddGame(new Game()
                {
                    ProviderId = "testid3",
                    Name = "Test Game 3"
                });

                var games = db.GamesCollection.FindAll().ToList();
                games[2].Name = "Changed Name";
                db.UpdateGameInDatabase(games[2]);
            }

            db = new GameDatabase();
            using (db.OpenDatabase(path))
            {
                var games = db.GamesCollection.FindAll().ToList();
                Assert.AreEqual(3, games.Count);
                Assert.AreEqual("Changed Name", games[2].Name);
                db.DeleteGame(games[1]);
            }

            db = new GameDatabase();
            using (db.OpenDatabase(path))
            {
                Assert.AreEqual(2, db.GamesCollection.Count());
            }
        }

        [Test]
        public void DeleteImageSafeTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "deleteimagetest.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path))
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
                    ProviderId = "testid2",
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
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "deleteimagecleanuptest.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path))
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

        [TestCase(Provider.GOG)]
        [TestCase(Provider.Steam)]
        [TestCase(Provider.Origin)]
        public void UpdateOwnedGamesTest(Provider provider)
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "ownedgames.db");
            FileSystem.DeleteFile(path);

            var libraryGames = new List<IGame>()
            {
                new Game()
                {
                    ProviderId = "testid",
                    Name = "Test Game",
                    Provider = provider
                },
                new Game()
                {
                    ProviderId = "testid2",
                    Name = "Test Game 2",
                    Provider = provider
                }
            };

            var gogLibrary = new Mock<IGogLibrary>();
            var steamLibrary = new Mock<ISteamLibrary>();
            var originLibrary = new Mock<IOriginLibrary>();
            gogLibrary.Setup(oc => oc.GetLibraryGames()).Returns(libraryGames);
            steamLibrary.Setup(oc => oc.GetLibraryGames(string.Empty)).Returns(libraryGames);
            originLibrary.Setup(oc => oc.GetLibraryGames()).Returns(libraryGames);

            var db = new GameDatabase(gogLibrary.Object, steamLibrary.Object, originLibrary.Object);
            using (db.OpenDatabase(path))
            {
                // Games are properly imported
                db.UpdateOwnedGames(provider);
                Assert.AreEqual(2, db.GamesCollection.Count());

                libraryGames.Add(new Game()
                {
                    ProviderId = "testid3",
                    Name = "Test Game 3",
                    Provider = provider
                });

                // New library game is added to DB
                db.UpdateOwnedGames(provider);
                Assert.AreEqual(3, db.GamesCollection.Count());

                // Game removed from library is removed from DB
                libraryGames.RemoveAt(0);
                db.UpdateOwnedGames(provider);
                Assert.AreEqual(2, db.GamesCollection.Count());

                // Game not in library but installed is not removed from DB
                libraryGames.Insert(0, new Game()
                {
                    ProviderId = "testid4",
                    Name = "Test Game 3",
                    Provider = provider,
                    PlayTask = new GameTask()
                });

                db.UpdateOwnedGames(provider);
                Assert.AreEqual(3, db.GamesCollection.Count());
                libraryGames.RemoveAt(0);
                db.UpdateOwnedGames(provider);
                Assert.AreEqual(3, db.GamesCollection.Count());
            }
        }

        [TestCase(Provider.GOG)]
        [TestCase(Provider.Steam)]
        [TestCase(Provider.Origin)]
        public void UpdateInstalledGamesTest(Provider provider)
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "installedgames.db");
            FileSystem.DeleteFile(path);

            var installedGames = CreateGameList(provider);

            var gogLibrary = new Mock<IGogLibrary>();
            var steamLibrary = new Mock<ISteamLibrary>();
            var originLibrary = new Mock<IOriginLibrary>();
            gogLibrary.Setup(oc => oc.GetInstalledGames()).Returns(installedGames);
            steamLibrary.Setup(oc => oc.GetInstalledGames()).Returns(installedGames);
            originLibrary.Setup(oc => oc.GetInstalledGames(false)).Returns(installedGames);
            originLibrary.Setup(oc => oc.GetInstalledGames(true)).Returns(installedGames);

            var db = new GameDatabase(gogLibrary.Object, steamLibrary.Object, originLibrary.Object);
            using (db.OpenDatabase(path))
            {
                // Games are imported
                db.UpdateInstalledGames(provider);
                var games = db.GamesCollection.FindAll().ToList();
                Assert.AreEqual(2, games.Count);
                Assert.IsTrue(games[0].IsInstalled);

                // Game is no longer installed and DB reflects that
                installedGames.Clear();
                installedGames.AddRange(CreateGameList(provider));
                installedGames.RemoveAt(0);
                db.UpdateInstalledGames(provider);
                games = db.GamesCollection.FindAll().ToList();
                Assert.IsFalse(games[0].IsInstalled);

                // Game is installed again
                installedGames.Clear();
                installedGames.AddRange(CreateGameList(provider));
                db.UpdateInstalledGames(provider);
                games = db.GamesCollection.FindAll().ToList();
                Assert.IsTrue(games[0].IsInstalled);

                // User tasks are not affected by import
                installedGames.Clear();
                installedGames.AddRange(CreateGameList(provider));

                games[0].OtherTasks = new System.Collections.ObjectModel.ObservableCollection<GameTask>()
                {
                    new GameTask()
                    {
                        IsBuiltIn = false,
                        Name = "User Task"
                    }
                };

                db.UpdateGameInDatabase(games[0]);
                db.UpdateInstalledGames(provider);
                games = db.GamesCollection.FindAll().ToList();
                Assert.AreEqual(3, games[0].OtherTasks.Count);

                // Changes made in built in task are preserved
                Assert.IsTrue(games[0].OtherTasks[0].IsBuiltIn);
                var task = games[0].OtherTasks[0].Name = "Changed name";
                db.UpdateGameInDatabase(games[0]);
                db.UpdateInstalledGames(provider);
                Assert.AreEqual(games[0].OtherTasks[0].Name, "Changed name");

                // Built in tasks are not removed when game is uninstalled
                installedGames.Clear();
                installedGames.AddRange(CreateGameList(provider));
                installedGames.RemoveAt(0);
                db.UpdateInstalledGames(provider);
                games = db.GamesCollection.FindAll().ToList();
                Assert.AreEqual(3, games[0].OtherTasks.Count);
                Assert.AreEqual("Changed name", games[0].OtherTasks[0].Name);
            }
        }

        private List<IGame> CreateGameList(Provider provider)
        {
            return new List<IGame>()
            {
                new Game()
                {
                    ProviderId = "testid",
                    Name = "Test Game",
                    Provider = provider,
                    PlayTask = new GameTask()
                    {
                        IsBuiltIn = true,
                        Type = GameTaskType.File
                    },
                    OtherTasks = new System.Collections.ObjectModel.ObservableCollection<GameTask>()
                    {
                        new GameTask()
                        {
                            IsBuiltIn = true,
                            Name = "Task 1"
                        },
                        new GameTask()
                        {
                            IsBuiltIn = true,
                            Name = "Task 2"
                        }
                    }
                },
                new Game()
                {
                    ProviderId = "testid2",
                    Name = "Test Game 2",
                    Provider = provider,
                    PlayTask = new GameTask()
                    {
                        IsBuiltIn = true,
                        Type = GameTaskType.File
                    },
                    OtherTasks = new System.Collections.ObjectModel.ObservableCollection<GameTask>()
                    {
                        new GameTask()
                        {
                            IsBuiltIn = true,
                            Name = "Task 1"
                        },
                        new GameTask()
                        {
                            IsBuiltIn = true,
                            Name = "Task 2"
                        }
                    }
                }
            };
        }

        #region GOG

        [Test]
        public void UpdateGogInstalledGamesCleanImportTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "goginstalledimportclean.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path))
            {
                db.UpdateInstalledGames(Provider.GOG);
                Assert.AreNotEqual(0, db.GamesCollection.Count());
            }
        }

        [Test]
        public void UpdateGogInstalledGamesUpdateImportTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "goginstalledimportupdate.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path))
            {
                db.UpdateInstalledGames(Provider.GOG);
                var game = db.GamesCollection.FindOne(Query.All());
                game.PlayTask = null;
                game.InstallDirectory = @"c:\nonsense\directory\";
                db.UpdateGameInDatabase(game);
            }

            db = new GameDatabase();
            using (db.OpenDatabase(path))
            {
                var game = db.GamesCollection.FindOne(Query.All());
                Assert.IsNull(game.PlayTask);
                Assert.AreEqual(@"c:\nonsense\directory\", game.InstallDirectory);
                var gameCount = db.GamesCollection.Count();

                db.UpdateInstalledGames(Provider.GOG);
                Assert.AreEqual(gameCount, db.GamesCollection.Count());

                game = db.GamesCollection.FindOne(Query.All());
                Assert.IsNotNull(game.PlayTask);
                Assert.AreNotEqual(@"c:\nonsense\directory\", game.InstallDirectory);
            }
        }

        [Test]
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
            using (db.OpenDatabase(path))
            {
                db.AddGame(game);
                db.UpdateGameWithMetadata(game);
                Assert.AreNotEqual("Temp Name", game.Name);
                Assert.IsNotNull(game.ReleaseDate);
                Assert.IsNotNull(game.Genres);
                Assert.IsNotNull(game.Developers);
                Assert.IsTrue(!string.IsNullOrEmpty(game.Description));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Links.First(a => a.Name == "Forum").Url));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Links.First(a => a.Name == "Store").Url));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Links.First(a => a.Name == "Wiki").Url));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Icon));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Image));
                Assert.IsTrue(!string.IsNullOrEmpty(game.BackgroundImage));

                var files = db.Database.FileStorage.FindAll();
                Assert.AreEqual(2, files.Count());
            }
        }

        #endregion GOG

        #region Steam
        [Test]
        public void UpdateSteamInstalledGamesCleanImportTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "steaminstalledimportclean.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path))
            {
                db.UpdateInstalledGames(Provider.Steam);
                Assert.AreNotEqual(0, db.GamesCollection.Count());
            }
        }

        [Test]
        public void UpdateSteamInstalledGamesUpdateImportTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "steaminstalledimportupdate.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path))
            {
                db.UpdateInstalledGames(Provider.Steam);
                var game = db.GamesCollection.FindOne(Query.All());
                game.PlayTask = null;
                game.InstallDirectory = @"c:\nonsense\directory\";
                db.UpdateGameInDatabase(game);
            }

            db = new GameDatabase();
            using (db.OpenDatabase(path))
            {
                var game = db.GamesCollection.FindOne(Query.All());
                Assert.IsNull(game.PlayTask);
                Assert.AreEqual(@"c:\nonsense\directory\", game.InstallDirectory);
                var gameCount = db.GamesCollection.Count();

                db.UpdateInstalledGames(Provider.Steam);
                Assert.AreEqual(gameCount, db.GamesCollection.Count());

                game = db.GamesCollection.FindOne(Query.All());
                Assert.IsNotNull(game.PlayTask);
                Assert.AreNotEqual(@"c:\nonsense\directory\", game.InstallDirectory);
            }
        }

        [Test]
        public void UpdateSteamGameWithMetadataTest()
        {
            var game = new Game()
            {
                ProviderId = "289070",
                Name = "Temp Name",
                Provider = Provider.Steam
            };

            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "steammetaupdate.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path))
            {
                db.AddGame(game);
                db.UpdateGameWithMetadata(game);
                Assert.AreNotEqual("Temp Name", game.Name);
                Assert.IsNotNull(game.ReleaseDate);
                Assert.IsNotNull(game.Genres);
                Assert.IsNotNull(game.Developers);
                Assert.IsFalse(string.IsNullOrEmpty(game.Description));
                Assert.IsFalse(string.IsNullOrEmpty(game.Links.First(a => a.Name == "Forum").Url));
                Assert.IsFalse(string.IsNullOrEmpty(game.Links.First(a => a.Name == "Store").Url));
                Assert.IsFalse(string.IsNullOrEmpty(game.Links.First(a => a.Name == "Wiki").Url));
                Assert.IsFalse(string.IsNullOrEmpty(game.Links.First(a => a.Name == "Workshop").Url));
                Assert.IsFalse(string.IsNullOrEmpty(game.Icon));
                Assert.IsFalse(string.IsNullOrEmpty(game.Image));
                Assert.IsFalse(string.IsNullOrEmpty(game.BackgroundImage));

                var files = db.Database.FileStorage.FindAll();
                Assert.AreEqual(2, files.Count());
            }
        }
        #endregion Steam

        #region Origin
        [Test]
        public void UpdateOriginInstalledGamesCleanImportTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "origininstalledimportclean.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path))
            {
                db.UpdateInstalledGames(Provider.Origin);
                Assert.AreNotEqual(0, db.GamesCollection.Count());
            }
        }

        [Test]
        public void UpdateOriginInstalledGamesUpdateImportTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "origininstalledimportupdate.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path))
            {
                db.UpdateInstalledGames(Provider.Origin);
                var game = db.GamesCollection.FindOne(Query.All());
                game.PlayTask = null;
                game.InstallDirectory = @"c:\nonsense\directory\";
                db.UpdateGameInDatabase(game);
            }

            db = new GameDatabase();
            using (db.OpenDatabase(path))
            {
                var game = db.GamesCollection.FindOne(Query.All());
                Assert.IsNull(game.PlayTask);
                Assert.AreEqual(@"c:\nonsense\directory\", game.InstallDirectory);
                var gameCount = db.GamesCollection.Count();

                db.UpdateInstalledGames(Provider.Origin);
                Assert.AreEqual(gameCount, db.GamesCollection.Count());

                game = db.GamesCollection.FindOne(Query.All());
                Assert.IsNotNull(game.PlayTask);
                Assert.AreNotEqual(@"c:\nonsense\directory\", game.InstallDirectory);
            }
        }

        [Test]
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
            using (db.OpenDatabase(path))
            {
                db.AddGame(game);
                db.UpdateGameWithMetadata(game);
                Assert.AreNotEqual("Temp Name", game.Name);
                Assert.IsNotNull(game.ReleaseDate);
                Assert.IsNotNull(game.Developers);
                Assert.IsTrue(!string.IsNullOrEmpty(game.Description));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Links.First(a => a.Name == "Forum").Url));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Links.First(a => a.Name == "Store").Url));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Links.First(a => a.Name == "Wiki").Url));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Image));

                var files = db.Database.FileStorage.FindAll();
                Assert.AreEqual(1, files.Count());
            }
        }

        #endregion Origin

        [Test]
        public void Migration0toCurrentTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "migration_0_Current.db");
            FileSystem.DeleteFile(path);

            var games = new List<Playnite.Models.Old0.Game>()
            {
                new Playnite.Models.Old0.Game()
                {
                    Provider = Provider.Custom,
                    ProviderId = "TestId1",
                    Name = "Test Name 1",
                    CommunityHubUrl = @"http://communityurl.com",
                    StoreUrl = @"http://storeurl.com",
                    WikiUrl = @"http://wiki.com"
                },
                new Playnite.Models.Old0.Game()
                {
                    Provider = Provider.Custom,
                    ProviderId = "TestId2",
                    Name = "Test Name 2",
                    CommunityHubUrl = @"http://communityurl.com"
                },
                new Playnite.Models.Old0.Game()
                {
                    Provider = Provider.Custom,
                    ProviderId = "TestId3",
                    Name = "Test Name 3"
                }
            };

            using (var database = new LiteDatabase(path))
            {
                database.Engine.UserVersion = 0;
                var collection = database.GetCollection<Playnite.Models.Old0.Game>("games");
                foreach (var game in games)
                {
                    var id = collection.Insert(game);
                    var genericCollection = database.GetCollection("games");
                    var record = genericCollection.FindById(id);
                    record.AsDocument["_type"] = "Playnite.Models.Game, Playnite";
                    genericCollection.Update(record.AsDocument);
                }
            }

            var db = new GameDatabase();
            using (db.OpenDatabase(path))
            {
                Assert.IsTrue(db.GamesCollection.Count() == 3);
                var migratedGames = db.GamesCollection.FindAll().ToList();
                Assert.AreEqual(3, migratedGames[0].Links.Count);
                Assert.IsFalse(string.IsNullOrEmpty(migratedGames[0].Links.First(a => a.Name == "Store").Url));
                Assert.IsFalse(string.IsNullOrEmpty(migratedGames[0].Links.First(a => a.Name == "Wiki").Url));
                Assert.IsFalse(string.IsNullOrEmpty(migratedGames[0].Links.First(a => a.Name == "Forum").Url));
                Assert.AreEqual(1, migratedGames[1].Links.Count);
                Assert.IsNull(migratedGames[2].Links);
            }
        }
    }
}
