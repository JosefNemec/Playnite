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
            using (db.OpenDatabase(path, true))
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

                Assert.AreEqual(2, db.Games.Count);
            }

            db = new GameDatabase();
            using (db.OpenDatabase(path, true))
            {
                Assert.AreEqual(2, db.Games.Count);
                db.AddGame(new Game()
                {
                    ProviderId = "testid3",
                    Name = "Test Game 3"
                });

                db.Games[2].Name = "Changed Name";
                db.UpdateGameInDatabase(db.Games[2]);
            }

            db = new GameDatabase();
            using (db.OpenDatabase(path, true))
            {
                Assert.AreEqual(3, db.Games.Count);
                Assert.AreEqual("Changed Name", db.Games[2].Name);
                db.DeleteGame(db.Games[1]);
            }

            db = new GameDatabase();
            using (db.OpenDatabase(path, true))
            {
                Assert.AreEqual(2, db.Games.Count);
            }
        }

        [Test]
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
                    ProviderId = "testid2",
                    Name = "Test Game 2",
                    Icon = "testimage"
                });

                // Doesn't remove image in use
                db.DeleteImageSafe("testimage", db.Games[0]);

                // Removes image
                db.Games[1].Icon = string.Empty;
                db.UpdateGameInDatabase(db.Games[1]);
                db.DeleteImageSafe("testimage", db.Games[0]);
                Assert.AreEqual(0, db.Database.FileStorage.FindAll().Count());
            }
        }

        [Test]
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

        [Test]
        public void UnloadNotInstalledGamesTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "unloadntoinstalled.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path, true))
            {
                db.AddGame(new Game()
                {
                    ProviderId = "testid",
                    Name = "Test Game",
                    PlayTask = new GameTask()
                });

                db.AddGame(new Game()
                {
                    ProviderId = "testid2",
                    Name = "Test Game 2",
                    PlayTask = new GameTask()
                });

                Assert.AreEqual(2, db.Games.Count);

                db.Games[0].PlayTask = null;
                db.UpdateGameInDatabase(db.Games[0]);
                db.UnloadNotInstalledGames(Provider.Custom);

                Assert.AreEqual(1, db.Games.Count);
            }

            db = new GameDatabase();
            using (db.OpenDatabase(path, true))
            {
                Assert.AreEqual(2, db.Games.Count);
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
            using (db.OpenDatabase(path, true))
            {
                // Games are properly imported
                db.UpdateOwnedGames(provider);
                Assert.AreEqual(2, db.Games.Count);

                libraryGames.Add(new Game()
                {
                    ProviderId = "testid3",
                    Name = "Test Game 3",
                    Provider = provider
                });

                // New library game is added to DB
                db.UpdateOwnedGames(provider);
                Assert.AreEqual(3, db.Games.Count);

                // Game removed from library is removed from DB
                libraryGames.RemoveAt(0);
                db.UpdateOwnedGames(provider);
                Assert.AreEqual(2, db.Games.Count);

                // Game not in library but installed is not removed from DB
                libraryGames.Insert(0, new Game()
                {
                    ProviderId = "testid4",
                    Name = "Test Game 3",
                    Provider = provider,
                    PlayTask = new GameTask()
                });

                db.UpdateOwnedGames(provider);
                Assert.AreEqual(3, db.Games.Count);
                libraryGames.RemoveAt(0);
                db.UpdateOwnedGames(provider);
                Assert.AreEqual(3, db.Games.Count);
            }
        }

        [TestCase(Provider.GOG)]
        [TestCase(Provider.Steam)]
        [TestCase(Provider.Origin)]
        public void UpdateInstalledLoadOnlyGamesTest(Provider provider)
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "installedloadonlygames.db");
            FileSystem.DeleteFile(path);

            var installedGames = CreateGameList(provider);

            var gogLibrary = new Mock<IGogLibrary>();
            var steamLibrary = new Mock<ISteamLibrary>();
            var originLibrary = new Mock<IOriginLibrary>();
            gogLibrary.Setup(oc => oc.GetInstalledGames()).Returns(installedGames);
            steamLibrary.Setup(oc => oc.GetInstalledGames()).Returns(installedGames);
            originLibrary.Setup(oc => oc.GetInstalledGames(false)).Returns(installedGames);
            originLibrary.Setup(oc => oc.GetInstalledGames(true)).Returns(installedGames);

            var loadSettings = new Settings()
            {
                GOGSettings = new GogSettings()
                {
                    IntegrationEnabled = true,
                    LibraryDownloadEnabled = false
                },
                SteamSettings = new SteamSettings()
                {
                    IntegrationEnabled = true,
                    LibraryDownloadEnabled = false
                },
                OriginSettings = new OriginSettings()
                {
                    IntegrationEnabled = true,
                    LibraryDownloadEnabled = false
                }                
            };

            var db = new GameDatabase(gogLibrary.Object, steamLibrary.Object, originLibrary.Object);
            using (db.OpenDatabase(path, true))
            {
                // Games are imported
                db.UpdateInstalledGames(provider);
                Assert.AreEqual(2, db.Games.Count);
                Assert.IsTrue(db.Games[0].IsInstalled);
            }

            db = new GameDatabase(gogLibrary.Object, steamLibrary.Object, originLibrary.Object);
            using (db.OpenDatabase(path, true))
            {
                // Game is no longer installed and DB reflects that
                installedGames.Clear();
                installedGames.AddRange(CreateGameList(provider));
                installedGames.RemoveAt(0);
                db.UpdateInstalledGames(provider);
                db.UnloadNotInstalledGames(provider);
                Assert.AreEqual(1, db.Games.Count);
            }

            db = new GameDatabase(gogLibrary.Object, steamLibrary.Object, originLibrary.Object);
            using (db.OpenDatabase(path, false))
            {
                db.LoadGamesFromDb(loadSettings);
                Assert.AreEqual(1, db.Games.Count);

                // Game is installed again
                installedGames.Clear();
                installedGames.AddRange(CreateGameList(provider));
                db.UpdateInstalledGames(provider);
                Assert.AreEqual(2, db.Games.Count);
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
            using (db.OpenDatabase(path, true))
            {
                // Games are imported
                db.UpdateInstalledGames(provider);
                Assert.AreEqual(2, db.Games.Count);
                Assert.IsTrue(db.Games[0].IsInstalled);

                // Game is no longer installed and DB reflects that
                installedGames.Clear();
                installedGames.AddRange(CreateGameList(provider));
                installedGames.RemoveAt(0);
                db.UpdateInstalledGames(provider);
                Assert.IsFalse(db.Games[0].IsInstalled);

                // Game is installed again
                installedGames.Clear();
                installedGames.AddRange(CreateGameList(provider));
                db.UpdateInstalledGames(provider);
                Assert.IsTrue(db.Games[0].IsInstalled);

                // User tasks are not affected by import
                installedGames.Clear();
                installedGames.AddRange(CreateGameList(provider));

                db.Games[0].OtherTasks = new System.Collections.ObjectModel.ObservableCollection<GameTask>()
                {
                    new GameTask()
                    {
                        IsBuiltIn = false,
                        Name = "User Task"
                    }
                };

                db.UpdateGameInDatabase(db.Games[0]);
                db.UpdateInstalledGames(provider);
                Assert.AreEqual(3, db.Games[0].OtherTasks.Count);

                installedGames.Clear();
                installedGames.AddRange(CreateGameList(provider));
                installedGames.RemoveAt(0);
                db.UpdateInstalledGames(provider);
                Assert.AreEqual(1, db.Games[0].OtherTasks.Count);
                Assert.AreEqual("User Task", db.Games[0].OtherTasks[0].Name);
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
            using (db.OpenDatabase(path, true))
            {
                db.UpdateInstalledGames(Provider.GOG);
                Assert.AreNotEqual(0, db.Games.Count);
            }
        }

        [Test]
        public void UpdateGogInstalledGamesUpdateImportTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "goginstalledimportupdate.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path, true))
            {
                db.UpdateInstalledGames(Provider.GOG);
                var game = db.Games[0];
                game.PlayTask = null;
                game.InstallDirectory = @"c:\nonsense\directory\";
                db.UpdateGameInDatabase(game);
            }

            db = new GameDatabase();
            using (db.OpenDatabase(path, true))
            {
                var game = db.Games[0];
                Assert.IsNull(game.PlayTask);
                Assert.AreEqual(@"c:\nonsense\directory\", game.InstallDirectory);
                var gameCount = db.Games.Count;

                db.UpdateInstalledGames(Provider.GOG);
                Assert.AreEqual(gameCount, db.Games.Count);

                game = db.Games[0];
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
            using (db.OpenDatabase(path, true))
            {
                db.AddGame(game);
                db.UpdateGameWithMetadata(game);
                Assert.AreNotEqual("Temp Name", game.Name);
                Assert.IsNotNull(game.ReleaseDate);
                Assert.IsNotNull(game.Genres);
                Assert.IsNotNull(game.Developers);
                Assert.IsTrue(!string.IsNullOrEmpty(game.Description));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Links["Forum"]));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Links["Store"]));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Links["Wiki"]));
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
            using (db.OpenDatabase(path, true))
            {
                db.UpdateInstalledGames(Provider.Steam);
                Assert.AreNotEqual(0, db.Games.Count);
            }
        }

        [Test]
        public void UpdateSteamInstalledGamesUpdateImportTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "steaminstalledimportupdate.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path, true))
            {
                db.UpdateInstalledGames(Provider.Steam);
                var game = db.Games[0];
                game.PlayTask = null;
                game.InstallDirectory = @"c:\nonsense\directory\";
                db.UpdateGameInDatabase(game);
            }

            db = new GameDatabase();
            using (db.OpenDatabase(path, true))
            {
                var game = db.Games[0];
                Assert.IsNull(game.PlayTask);
                Assert.AreEqual(@"c:\nonsense\directory\", game.InstallDirectory);
                var gameCount = db.Games.Count;

                db.UpdateInstalledGames(Provider.Steam);
                Assert.AreEqual(gameCount, db.Games.Count);

                game = db.Games[0];
                Assert.IsNotNull(game.PlayTask);
                Assert.AreNotEqual(@"c:\nonsense\directory\", game.InstallDirectory);
            }
        }

        [Test]
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
                Assert.IsTrue(!string.IsNullOrEmpty(game.Links["Forum"]));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Links["Store"]));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Links["Wiki"]));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Icon));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Image));
                Assert.IsTrue(!string.IsNullOrEmpty(game.BackgroundImage));

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
            using (db.OpenDatabase(path, true))
            {
                db.UpdateInstalledGames(Provider.Origin);
                Assert.AreNotEqual(0, db.Games.Count);
            }
        }

        [Test]
        public void UpdateOriginInstalledGamesUpdateImportTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "origininstalledimportupdate.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path, true))
            {
                db.UpdateInstalledGames(Provider.Origin);
                var game = db.Games[0];
                game.PlayTask = null;
                game.InstallDirectory = @"c:\nonsense\directory\";
                db.UpdateGameInDatabase(game);
            }

            db = new GameDatabase();
            using (db.OpenDatabase(path, true))
            {
                var game = db.Games[0];
                Assert.IsNull(game.PlayTask);
                Assert.AreEqual(@"c:\nonsense\directory\", game.InstallDirectory);
                var gameCount = db.Games.Count;

                db.UpdateInstalledGames(Provider.Origin);
                Assert.AreEqual(gameCount, db.Games.Count);

                game = db.Games[0];
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
            using (db.OpenDatabase(path, true))
            {
                db.AddGame(game);
                db.UpdateGameWithMetadata(game);
                Assert.AreNotEqual("Temp Name", game.Name);
                Assert.IsNotNull(game.ReleaseDate);
                Assert.IsNotNull(game.Developers);
                Assert.IsTrue(!string.IsNullOrEmpty(game.Description));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Links["Forum"]));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Links["Store"]));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Links["Wiki"]));
                Assert.IsTrue(!string.IsNullOrEmpty(game.Image));

                var files = db.Database.FileStorage.FindAll();
                Assert.AreEqual(1, files.Count());
            }
        }

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
            using (db.OpenDatabase(path, true))
            {
                Assert.IsTrue(db.Games.Count == 3);
                Assert.AreEqual(3, db.Games[0].Links.Count);
                Assert.IsFalse(string.IsNullOrEmpty(db.Games[0].Links["Store"]));
                Assert.IsFalse(string.IsNullOrEmpty(db.Games[0].Links["Wiki"]));
                Assert.IsFalse(string.IsNullOrEmpty(db.Games[0].Links["Forum"]));
                Assert.AreEqual(1, db.Games[1].Links.Count);
                Assert.IsNull(db.Games[2].Links);
            }
        }

        #endregion Origin
    }
}
