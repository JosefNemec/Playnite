using LiteDB;
using Moq;
using NUnit.Framework;
using Playnite;
using Playnite.Database;
using Playnite.SDK.Models;
using Playnite.Providers.Steam;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.Database
{
    [TestFixture]
    public class GameDatabaseMigrationTests
    {
        [Test]
        public void PlatformGeneratorTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "defaultplatforms.db");
            FileSystem.DeleteFile(path);
            var db = new GameDatabase(null);
            using (db.OpenDatabase(path))
            {
                var platforms = db.PlatformsCollection.FindAll().ToList();
                CollectionAssert.IsNotEmpty(platforms);                
                db.RemovePlatform(platforms);
                CollectionAssert.IsEmpty(db.PlatformsCollection.FindAll());
                db.AddPlatform(new Platform("Test"));
                db.AddPlatform(new Platform("Test2"));
            }
                        
            using (db.OpenDatabase(path))
            {
                var platforms = db.PlatformsCollection.FindAll().ToList();
                Assert.AreEqual(2, platforms.Count);
                Assert.AreEqual("Test", platforms[0].Name);
                Assert.AreEqual("Test2", platforms[1].Name);
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

            GameDatabase.MigrateDatabase(path);
            var db = new GameDatabase(null, path);
            using (db.OpenDatabase())
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

        [Test]
        public void Migration1toCurrentTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "migration_1_Current.db");
            FileSystem.DeleteFile(path);

            var emulators = new List<Playnite.Models.Old1.Emulator>()
            {
                new Playnite.Models.Old1.Emulator("TestEmu")
                {
                    Arguments = "Test Arguments",
                    Executable = "Test Executable",
                    ImageExtensions = new List<string>() { ".ext1", ".ext2" },
                    Platforms = new List<int>() { 1, 2 },
                    WorkingDirectory = "Test Directory"
                },
                new Playnite.Models.Old1.Emulator("TestEmu2")
                {
                    Arguments = "Test Arguments2",
                    Executable = "Test Executable2",
                    ImageExtensions = new List<string>() { ".ext3", ".ext4" },
                    Platforms = new List<int>() { 3 },
                    WorkingDirectory = "Test Directory2"
                }
            };

            var platforms = new List<Playnite.Models.Old1.Platform>()
            {
                new Playnite.Models.Old1.Platform("TestPlat1"),
                new Playnite.Models.Old1.Platform("TestPlat2")
            };

            using (var database = new LiteDatabase(path))
            {
                database.Engine.UserVersion = 1;
                var emuCol = database.GetCollection<Playnite.Models.Old1.Emulator>("emulators");
                emuCol.InsertBulk(emulators);

                var platCol = database.GetCollection<Playnite.Models.Old1.Platform>("platforms");
                platCol.InsertBulk(platforms);

                var gamesCol = database.GetCollection<Playnite.Models.Old1.Game>("games");  
                var games = new List<Playnite.Models.Old1.Game>()
                {
                    new Playnite.Models.Old1.Game()
                    {
                        Provider = Provider.Custom,
                        ProviderId = "TestId1",
                        PlatformId = 1,
                        Name = "Test Name 1",
                        PlayTask = new Playnite.Models.Old1.GameTask()
                        {
                            Type = GameTaskType.Emulator,
                            EmulatorId = emuCol.FindAll().First().Id
                        },
                        OtherTasks = new ObservableCollection<Playnite.Models.Old1.GameTask>()
                        {
                            new Playnite.Models.Old1.GameTask()
                            {
                                Type = GameTaskType.Emulator,
                                EmulatorId = emuCol.FindAll().First().Id
                            }
                        }
                    },
                    new Playnite.Models.Old1.Game()
                    {
                        Provider = Provider.Custom,
                        ProviderId = "TestId2",
                        PlatformId = null,
                        Name = "Test Name 2",
                        PlayTask = new Playnite.Models.Old1.GameTask()
                        {
                            Type = GameTaskType.Emulator,
                            EmulatorId = 0
                        },
                        OtherTasks = new ObservableCollection<Playnite.Models.Old1.GameTask>()
                        {
                            new Playnite.Models.Old1.GameTask()
                            {
                                Type = GameTaskType.Emulator,
                                EmulatorId = 0
                            }
                        }
                    }
                };

                gamesCol.InsertBulk(games);
                var genericCollection = database.GetCollection("games");
                foreach (var game in genericCollection.FindAll().ToList())
                {
                    game.AsDocument["_type"] = "Playnite.Models.Game, Playnite";
                    genericCollection.Update(game.AsDocument);
                }

                var file = Playnite.PlayniteTests.CreateFakeFile();
                database.FileStorage.Upload(file.Name, file.Path);
                file = Playnite.PlayniteTests.CreateFakeFile();
                database.FileStorage.Upload(file.Name, file.Path);
            }

            GameDatabase.MigrateDatabase(path);
            var db = new GameDatabase(null, path);
            using (db.OpenDatabase())
            {
                var plats = db.PlatformsCollection.FindAll().ToList();
                Assert.IsNotNull(plats[0].Id);
                Assert.IsNotNull(plats[1].Id);

                var emus = db.EmulatorsCollection.FindAll().ToList();
                Assert.AreEqual(2, emus.Count());
                Assert.AreEqual(plats[0].Id, emus[0].Profiles[0].Platforms[0]);
                Assert.AreEqual(plats[1].Id, emus[0].Profiles[0].Platforms[1]);
                CollectionAssert.IsEmpty(emus[1].Profiles[0].Platforms);
                Assert.IsNotNull(emus[0].Id);
                Assert.IsNotNull(emus[1].Id);

                var emu = emus.First();
                var emuConf = emu.Profiles.First();
                Assert.AreEqual(1, emu.Profiles.Count);
                Assert.IsNotNull(emu.Profiles);
                Assert.AreEqual("Test Arguments", emuConf.Arguments);
                Assert.AreEqual("Test Executable", emuConf.Executable);
                Assert.AreEqual("Test Directory", emuConf.WorkingDirectory);
                Assert.AreEqual(2, emuConf.Platforms.Count);
                Assert.AreEqual(2, emuConf.ImageExtensions.Count);
                Assert.AreEqual("ext1", emuConf.ImageExtensions[0]);
                Assert.AreEqual("ext2", emuConf.ImageExtensions[1]);

                var games = db.GamesCollection.FindAll().ToList();
                var game = games[0];
                Assert.AreEqual(plats[0].Id, game.PlatformId);
                Assert.AreEqual(emu.Profiles.First().Id, game.PlayTask.EmulatorProfileId);
                Assert.AreEqual(emu.Id, game.PlayTask.EmulatorId);
                Assert.AreEqual(emu.Profiles.First().Id, game.OtherTasks[0].EmulatorProfileId);
                Assert.AreEqual(emu.Id, game.OtherTasks[0].EmulatorId);

                Assert.IsNull(games[1].PlatformId);
                Assert.IsNull(games[1].PlayTask.EmulatorId);
                Assert.IsNull(games[1].PlayTask.EmulatorProfileId);
                Assert.IsNull(games[1].OtherTasks[0].EmulatorId);
                Assert.IsNull(games[1].OtherTasks[0].EmulatorProfileId);

                var files = db.Database.FileStorage.FindAll().ToList();
                Assert.AreEqual(2, files.Count);
                foreach (var file in files)
                {
                    Assert.IsTrue(file.Metadata.ContainsKey("checksum"));
                    Assert.IsFalse(string.IsNullOrEmpty(file.Metadata["checksum"].AsString));
                }
            }
        }

        [Test]
        public void GameStatesFixTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "gamestatefixes.db");
            FileSystem.DeleteFile(path);

            using (var database = new LiteDatabase(path))
            {
                database.Engine.UserVersion = GameDatabase.DBVersion;
                var games = new List<Game>()
                {
                    new Game()
                    {
                        Provider = Provider.Custom,
                        Name = "Test Name 1"
                    },
                    new Game()
                    {
                        Provider = Provider.Custom,
                        Name = "Test Name 2",
                        InstallDirectory = "installdir"
                    },
                    new Game()
                    {
                        Provider = Provider.Custom,
                        Name = "Test Name 3",
                        IsoPath = "isopath"
                    },
                    new Game()
                    {
                        Provider = Provider.Custom,
                        Name = "Test Name 4",
                        IsoPath = "isopath",
                        InstallDirectory = "installdir"
                    }
                };

                var collection = database.GetCollection<Game>("games");
                collection.Insert(games);
            }

            var db = new GameDatabase();
            using (db.OpenDatabase(path))
            {
                var games = db.GetGames();
                Assert.IsFalse(games[0].State.Installed);
                Assert.IsTrue(games[1].State.Installed);
                Assert.IsTrue(games[2].State.Installed);
                Assert.IsTrue(games[3].State.Installed);
                Assert.IsTrue(db.GetDatabaseSettings().InstStatesFixed);
            }
        }
    }
}
