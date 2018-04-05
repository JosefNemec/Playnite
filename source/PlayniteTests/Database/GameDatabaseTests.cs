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
using Playnite.Providers.GOG;
using Playnite.Providers.Steam;
using Playnite.Providers.Origin;
using NUnit.Framework;
using LiteDB;
using Playnite.Providers.Uplay;
using Playnite.Providers.BattleNet;

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

            var db = new GameDatabase(null);
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

            db = new GameDatabase(null);
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
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "deleteimagetest.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase(null);
            using (db.OpenDatabase(path))
            {                
                db.AddFile("testimage", "testimage.png", new byte[] { 0 });
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

            var db = new GameDatabase(null);
            using (db.OpenDatabase(path))
            {
                db.AddFile("testimage", "testimage.png", new byte[] { 0 });
                db.AddFile("testicon", "testicon.png", new byte[] { 0 });
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

            var settings = new Settings();
            var gogLibrary = new Mock<IGogLibrary>();
            var steamLibrary = new Mock<ISteamLibrary>();
            var originLibrary = new Mock<IOriginLibrary>();
            gogLibrary.Setup(oc => oc.GetLibraryGames()).Returns(libraryGames);
            steamLibrary.Setup(oc => oc.GetLibraryGames(settings.SteamSettings)).Returns(libraryGames);
            originLibrary.Setup(oc => oc.GetLibraryGames()).Returns(libraryGames);        

            var db = new GameDatabase(settings, gogLibrary.Object, steamLibrary.Object, originLibrary.Object, null, null);
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

                // Game removed from library was not removed from DB
                libraryGames.RemoveAt(0);
                db.UpdateOwnedGames(provider);
                Assert.AreEqual(3, db.GamesCollection.Count());

                // Game not in library but installed is not removed from DB
                libraryGames.Insert(0, new Game()
                {
                    ProviderId = "testid4",
                    Name = "Test Game 3",
                    Provider = provider,
                    PlayTask = new GameTask()
                });

                db.UpdateOwnedGames(provider);
                Assert.AreEqual(4, db.GamesCollection.Count());
                libraryGames.RemoveAt(0);
                db.UpdateOwnedGames(provider);
                Assert.AreEqual(4, db.GamesCollection.Count());
            }
        }

        [TestCase(Provider.GOG)]
        [TestCase(Provider.Steam)]
        [TestCase(Provider.Origin)]
        [TestCase(Provider.Uplay)]
        public void UpdateInstalledGamesTest(Provider provider)
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "installedgames.db");
            FileSystem.DeleteFile(path);

            var installedGames = CreateGameList(provider);

            var gogLibrary = new Mock<IGogLibrary>();
            var steamLibrary = new Mock<ISteamLibrary>();
            var originLibrary = new Mock<IOriginLibrary>();
            var uplayLibrary = new Mock<IUplayLibrary>();
            var battleNetLibrary = new Mock<IBattleNetLibrary>();
            gogLibrary.Setup(oc => oc.GetInstalledGames()).Returns(installedGames);
            steamLibrary.Setup(oc => oc.GetInstalledGames()).Returns(installedGames);
            originLibrary.Setup(oc => oc.GetInstalledGames(false)).Returns(installedGames);
            originLibrary.Setup(oc => oc.GetInstalledGames(true)).Returns(installedGames);
            uplayLibrary.Setup(oc => oc.GetInstalledGames()).Returns(installedGames);
            battleNetLibrary.Setup(oc => oc.GetInstalledGames()).Returns(installedGames);

            var settings = new Settings();
            var db = new GameDatabase(settings, gogLibrary.Object, steamLibrary.Object, originLibrary.Object, uplayLibrary.Object, battleNetLibrary.Object);
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
            var settings = new Settings();
            settings.GOGSettings.RunViaGalaxy = false;
            var db = new GameDatabase(settings);
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
            var settings = new Settings();
            settings.GOGSettings.RunViaGalaxy = false;

            var db = new GameDatabase(settings);
            using (db.OpenDatabase(path))
            {
                db.UpdateInstalledGames(Provider.GOG);
                var game = db.GamesCollection.FindOne(Query.All());
                game.PlayTask = null;
                game.InstallDirectory = @"c:\nonsense\directory\";
                db.UpdateGameInDatabase(game);
            }

            db = new GameDatabase(settings);
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

        #endregion GOG

        #region Steam
        [Test]
        public void UpdateSteamInstalledGamesCleanImportTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "steaminstalledimportclean.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase(null);
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

            var db = new GameDatabase(null);
            using (db.OpenDatabase(path))
            {
                db.UpdateInstalledGames(Provider.Steam);
                var game = db.GamesCollection.FindOne(Query.All());
                game.PlayTask = null;
                game.InstallDirectory = @"c:\nonsense\directory\";
                db.UpdateGameInDatabase(game);
            }

            db = new GameDatabase(null);
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
        #endregion Steam

        #region Origin
        [Test]
        public void UpdateOriginInstalledGamesCleanImportTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "origininstalledimportclean.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase(null);
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

            var db = new GameDatabase(null);
            using (db.OpenDatabase(path))
            {
                db.UpdateInstalledGames(Provider.Origin);
                var game = db.GamesCollection.FindOne(Query.All());
                game.PlayTask = null;
                game.InstallDirectory = @"c:\nonsense\directory\";
                db.UpdateGameInDatabase(game);
            }

            db = new GameDatabase(null);
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
        #endregion Origin

        #region Uplay
        [Test]
        public void UpdateUplayInstalledGamesCleanImportTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "uplayinstalledimportclean.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase(null);
            using (db.OpenDatabase(path))
            {
                db.UpdateInstalledGames(Provider.Uplay);
                Assert.AreNotEqual(0, db.GamesCollection.Count());
            }
        }

        [Test]
        public void UpdateUplayInstalledGamesUpdateImportTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "uplayinstalledimportupdate.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase(null);
            using (db.OpenDatabase(path))
            {
                db.UpdateInstalledGames(Provider.Uplay);
                var game = db.GamesCollection.FindOne(Query.All());
                game.PlayTask = null;
                game.InstallDirectory = @"c:\nonsense\directory\";
                db.UpdateGameInDatabase(game);
            }

            db = new GameDatabase(null);
            using (db.OpenDatabase(path))
            {
                var game = db.GamesCollection.FindOne(Query.All());
                Assert.IsNull(game.PlayTask);
                Assert.AreEqual(@"c:\nonsense\directory\", game.InstallDirectory);
                var gameCount = db.GamesCollection.Count();

                db.UpdateInstalledGames(Provider.Uplay);
                Assert.AreEqual(gameCount, db.GamesCollection.Count());

                game = db.GamesCollection.FindOne(Query.All());
                Assert.IsNotNull(game.PlayTask);
                Assert.AreNotEqual(@"c:\nonsense\directory\", game.InstallDirectory);
            }
        }
        #endregion Uplay       
    }
}
