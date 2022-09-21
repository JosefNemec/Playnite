using NUnit.Framework;
using Playnite;
using Playnite.Database;
using Playnite.SDK.Models;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                Roms = new ObservableCollection<GameRom> { new GameRom("test", Path.Combine(dir, "test.iso")) },
                PlatformIds = new List<Guid> { database.Platforms.First().Id },
                Version = "1.0",
                PluginId = Guid.NewGuid(),
                GameId = "game_id",
                Id = Guid.NewGuid()
            };

            Assert.AreEqual(string.Empty, game.ExpandVariables(string.Empty));
            Assert.AreEqual("teststring", game.ExpandVariables("teststring"));
            Assert.AreEqual(dir + "teststring", game.ExpandVariables("{InstallDir}teststring"));
            Assert.AreEqual(game.InstallDirectory, game.ExpandVariables("{InstallDir}"));
            Assert.AreEqual("test", game.ExpandVariables("{ImageNameNoExt}"));
            Assert.AreEqual("test.iso", game.ExpandVariables("{ImageName}"));
            Assert.AreEqual(PlaynitePaths.ProgramPath, game.ExpandVariables("{PlayniteDir}"));
            Assert.AreEqual("test game", game.ExpandVariables("{Name}"));
            Assert.AreEqual("test2", game.ExpandVariables("{InstallDirName}"));
            Assert.AreEqual(game.Platforms[0].Name, game.ExpandVariables("{Platform}"));
            Assert.AreEqual(game.PluginId.ToString(), game.ExpandVariables("{PluginId}"));
            Assert.AreEqual(game.GameId, game.ExpandVariables("{GameId}"));
            Assert.AreEqual(game.Id.ToString(), game.ExpandVariables("{DatabaseId}"));
            Assert.AreEqual(game.Version, game.ExpandVariables("{Version}"));
            Assert.AreEqual(Path.Combine(dir, "test.iso"), game.ExpandVariables("{ImagePath}"));

            game.InstallDirectory = @"c:\test\test2";
            Assert.AreEqual("test2", game.ExpandVariables("{InstallDirName}"));
        }

        [Test]
        public void ExpandVariablesReferenceTest()
        {
            var database = new InMemoryGameDatabase();
            Game.DatabaseReference = database;
            GameDatabase.GenerateSampleData(database);

            var game = new Game()
            {
                Name = "test game",
                InstallDirectory = @"{PlayniteDir}\test\test2\",
                Roms = new ObservableCollection<GameRom> { new GameRom("test", @"{InstallDir}\test.iso") }
            };

            var expanded = game.ExpandVariables("{ImagePath}");
            StringAssert.DoesNotContain("{ImagePath}", expanded);
            StringAssert.DoesNotContain("{PlayniteDir}", expanded);
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

        [Test]
        public void GetCompatibleEmulatorsTest()
        {
            using (var db = new GameDbTestWrapper())
            {
                db.DB.Emulators.Add(new List<Emulator>
                {
                    new Emulator("emu1")
                    {
                        CustomProfiles = new ObservableCollection<CustomEmulatorProfile>
                        {
                            new CustomEmulatorProfile
                            {
                                Name = "ps",
                                Platforms = db.DB.Platforms.Where(a => a.SpecificationId == "sony_playstation" || a.SpecificationId == "sony_playstation2").Select(a => a.Id).ToList()
                            },
                            new CustomEmulatorProfile
                            {
                                Platforms = db.DB.Platforms.Where(a => a.SpecificationId == "xbox").Select(a => a.Id).ToList()
                            }
                        }
                    },
                    new Emulator("emu2")
                    {
                        CustomProfiles = new ObservableCollection<CustomEmulatorProfile>
                        {
                            new CustomEmulatorProfile
                            {
                                Platforms = db.DB.Platforms.Where(a => a.SpecificationId == "xbox").Select(a => a.Id).ToList()
                            }
                        }
                    },
                    new Emulator("emu3")
                    {
                        CustomProfiles = new ObservableCollection<CustomEmulatorProfile>
                        {
                            new CustomEmulatorProfile
                            {
                                Name = "test profile"
                            }
                        }
                    },
                    new Emulator("emu4")
                    {
                        BuiltInConfigId = "duckstation",
                        BuiltinProfiles = new ObservableCollection<BuiltInEmulatorProfile>
                        {
                            new BuiltInEmulatorProfile
                            {
                                Name = "test",
                                BuiltInProfileName = "Default"
                            },
                        }
                    },
                    new Emulator("emu5")
                    {
                        BuiltInConfigId = "duckstation",
                        BuiltinProfiles = new ObservableCollection<BuiltInEmulatorProfile>
                        {
                            new BuiltInEmulatorProfile
                            {
                                Name = "test",
                                BuiltInProfileName = "Default"
                            },
                        },
                        CustomProfiles = new ObservableCollection<CustomEmulatorProfile>
                        {
                            new CustomEmulatorProfile
                            {
                                Platforms = db.DB.Platforms.Where(a => a.SpecificationId == "sony_playstation").Select(a => a.Id).ToList()
                            }
                        }
                    },
                    new Emulator("emu6")
                    {
                        BuiltInConfigId = "melonds",
                        BuiltinProfiles = new ObservableCollection<BuiltInEmulatorProfile>
                        {
                            new BuiltInEmulatorProfile
                            {
                                Name = "test",
                                BuiltInProfileName = "Default"
                            }
                        }
                    }
                });

                var game = new Game()
                {
                    PlatformIds = db.DB.Platforms.Where(a => a.SpecificationId == "sony_playstation").Select(a => a.Id).ToList()
                };

                var comEmus = game.GetCompatibleEmulators(db.DB).OrderBy(a => a.Key.Name).ToList();
                Assert.AreEqual(3, comEmus.Count);

                Assert.AreEqual("emu1", comEmus[0].Key.Name);
                Assert.AreEqual(1, comEmus[0].Value.Count);
                Assert.AreEqual("ps", comEmus[0].Value[0].Name);

                Assert.AreEqual("emu4", comEmus[1].Key.Name);
                Assert.AreEqual(1, comEmus[1].Value.Count);

                Assert.AreEqual("emu5", comEmus[2].Key.Name);
                Assert.AreEqual(2, comEmus[2].Value.Count);
            }
        }

        [Test]
        public void InstallDriveTest()
        {
            var database = new InMemoryGameDatabase();
            Game.DatabaseReference = database;
            GameDatabase.GenerateSampleData(database);

            var dir = @"C:\test\test2\";
            var game = new Game()
            {
                Name = "test game",
                InstallDirectory = dir,
                IsInstalled = true
            };

            Assert.AreEqual(@"C:\", game.GetInstallDrive());
            game.InstallDirectory = @"c:\test\test2\";
            Assert.AreEqual(@"C:\", game.GetInstallDrive());
            game.IsInstalled = false;
            Assert.AreEqual(string.Empty, game.GetInstallDrive());

            game.IsInstalled = true;
            game.InstallDirectory = @" ";
            Assert.AreEqual(string.Empty, game.GetInstallDrive());
            game.InstallDirectory = @"";
            Assert.AreEqual(string.Empty, game.GetInstallDrive());
            game.InstallDirectory = null;
            Assert.AreEqual(string.Empty, game.GetInstallDrive());
            game.InstallDirectory = @"c:\test\<>:""/\|?*\test2\";
            Assert.AreEqual(string.Empty, game.GetInstallDrive());
            game.InstallDirectory = @"SomeString";
            Assert.AreEqual(string.Empty, game.GetInstallDrive());
        }
    }
}