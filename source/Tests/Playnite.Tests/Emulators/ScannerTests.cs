using NUnit.Framework;
using Playnite.Common;
using Playnite.Emulators;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.Tests.Emulators
{
    [TestFixture]
    public class ScannerTests
    {
        [Test]
        public void DoubleExtensionTest() // #2768 bug
        {
            using (var tempPath = TempDirectory.Create())
            {
                var path1 = Path.Combine(tempPath.TempPath, "game 1.p8");
                var path2 = Path.Combine(tempPath.TempPath, "game 2.p8.png");
                FileSystem.CreateFile(path1);
                FileSystem.CreateFile(path2);
                FileSystem.CreateFile(Path.Combine(tempPath.TempPath, "game 3.png"));

                var scanner = new GameScanner(new SDK.Models.GameScannerConfig(), null);
                var scanResults = new Dictionary<string, List<ScannedRom>>();
                scanner.ScanDirectoryBase(
                    tempPath.TempPath,
                    new List<string> { "p8", "p8.png" },
                    null,
                    scanResults,
                    new System.Threading.CancellationTokenSource().Token,
                    null,
                    true,
                    true);

                Assert.AreEqual(2, scanResults.Count);
                Assert.AreEqual("game 1", scanResults["game 1"][0].Name.Name);
                Assert.AreEqual(path1, scanResults["game 1"][0].Path);
                Assert.AreEqual("game 2", scanResults["game 2"][0].Name.Name);
                Assert.AreEqual(path2, scanResults["game 2"][0].Path);
            }
        }

        [Test]
        public void MultiDiskRegionTest() // #2573 bug
        {
            using (var tempPath = TempDirectory.Create())
            using (var db = new GameDbTestWrapper(tempPath))
            {
                var isos = new List<string>
                {
                    Path.Combine(tempPath.TempPath, "test game (Europe) (Disc 1).iso"),
                    Path.Combine(tempPath.TempPath, "test game (Europe) (Disc 2).iso"),
                    Path.Combine(tempPath.TempPath, "test game (USA) (Disc 1).iso"),
                    Path.Combine(tempPath.TempPath, "test game (USA) (Disc 2).iso"),
                    Path.Combine(tempPath.TempPath, "test game (JP) (EE) (Disc 1).iso"),
                    Path.Combine(tempPath.TempPath, "test game (JP) (EE) (Disc 2).iso"),
                };

                isos.ForEach(a => FileSystem.CreateFile(a));
                var emu = TestAppTools.GetEmulatorObj();
                db.DB.Emulators.Add(emu);
                var config = new GameScannerConfig
                {
                    EmulatorId = emu.Id,
                    EmulatorProfileId = emu.CustomProfiles[0].Id,
                    Directory = tempPath.TempPath
                };

                var scanner = new GameScanner(config, db.DB);
                var games = scanner.Scan(CancellationToken.None, out var newPlatforms, out var newRegions);

                Assert.AreEqual(1, games.Count);
                var game = games[0].ToGame();
                Assert.AreEqual(6, game.Roms.Count);
                Assert.AreEqual("Disc 1 - Europe", game.Roms[0].Name);
                Assert.AreEqual("Disc 2 - Europe", game.Roms[1].Name);
                Assert.AreEqual("Disc 1 - JP - EE", game.Roms[2].Name);
                Assert.AreEqual("Disc 2 - JP - EE", game.Roms[3].Name);
                Assert.AreEqual("Disc 1 - USA", game.Roms[4].Name);
                Assert.AreEqual("Disc 2 - USA", game.Roms[5].Name);
            }
        }

        [Test]
        public void SubfolderScanOptionTest()
        {
            using (var tempPath = TempDirectory.Create())
            using (var db = new GameDbTestWrapper(tempPath))
            {
                var isos = new List<string>
                {
                    Path.Combine(tempPath.TempPath, "test_root.iso"),
                    Path.Combine(tempPath.TempPath, "sub", "test_sub.iso"),
                    Path.Combine(tempPath.TempPath, "sub", "sub2", "test_sub2.iso")
                };

                isos.ForEach(a => FileSystem.CreateFile(a));
                var emu = TestAppTools.GetEmulatorObj();
                db.DB.Emulators.Add(emu);
                var config = new GameScannerConfig
                {
                    EmulatorId = emu.Id,
                    EmulatorProfileId = emu.CustomProfiles[0].Id,
                    Directory = tempPath.TempPath,
                    ScanSubfolders = true
                };

                var scanner = new GameScanner(config, db.DB);
                var games = scanner.Scan(CancellationToken.None, out var newPlatforms, out var newRegions);
                Assert.AreEqual(3, games.Count);

                config.ScanSubfolders = false;
                games = scanner.Scan(CancellationToken.None, out newPlatforms, out newRegions);
                Assert.AreEqual(1, games.Count);
                Assert.AreEqual("test root", games[0].Name);
            }
        }

        [Test]
        public void ScanInsideArchivesOptionTest()
        {
            using (var tempPath = TempDirectory.Create())
            using (var db = new GameDbTestWrapper(tempPath))
            {
                var archFile = Path.Combine(tempPath.TempPath, "archive", "test.mp3");
                FileSystem.WriteStringToFile(archFile, "AAA");
                ZipFile.CreateFromDirectory(Path.Combine(tempPath.TempPath, "archive"), Path.Combine(tempPath.TempPath, "archive.zip"), CompressionLevel.Fastest, false);
                FileSystem.DeleteFile(archFile);
                FileSystem.CreateFile(Path.Combine(tempPath.TempPath, "test2.iso"));

                var platform = TestAppTools.GetPlatformObj();
                db.DB.Platforms.Add(platform);
                var emu = TestAppTools.GetEmulatorObj();
                emu.CustomProfiles[0].Platforms = new List<Guid> { platform.Id };
                db.DB.Emulators.Add(emu);

                var config = new GameScannerConfig
                {
                    EmulatorId = emu.Id,
                    EmulatorProfileId = emu.CustomProfiles[0].Id,
                    Directory = tempPath.TempPath,
                    ScanSubfolders = true,
                    ScanInsideArchives = true
                };

                var scanner = new GameScanner(config, db.DB,
                    (platformIds) => platformIds?.Contains(TestAppTools.PlatformSpecId) == true ? new List<EmulationDatabase.IEmulationDatabaseReader>
                    {
                        new TestEmulationDatabaseReader(
                            "test",
                            // 66A031A7 matches generated test.mp3
                            (romCrc) => romCrc == "66A031A7" ? new List<DatGame>() { new DatGame { Name = "crc match", RomName = "crc match" } } : new List<DatGame>(),
                            (__) => new List<DatGame>(),
                            (__) => new List<DatGame>(),
                            (__) => new List<DatGame>())
                    } : new List<EmulationDatabase.IEmulationDatabaseReader>());

                var games = scanner.Scan(CancellationToken.None, out var newPlatforms, out var newRegions);
                Assert.AreEqual(2, games.Count);
                Assert.AreEqual("crc match", games[0].Name);
                Assert.AreEqual("test2", games[1].Name);

                config.ScanInsideArchives = false;
                games = scanner.Scan(CancellationToken.None, out newPlatforms, out newRegions);
                Assert.AreEqual(2, games.Count);
                Assert.AreEqual("archive", games[0].Name);
                Assert.AreEqual("test2", games[1].Name);
            }
        }

        [Test]
        public void PlaylistImportTest()
        {
            using (var tempPath = TempDirectory.Create())
            using (var db = new GameDbTestWrapper(tempPath))
            {
                var scanner = new GameScanner(new GameScannerConfig(), null);
                var cuePath = Path.Combine(tempPath.TempPath, "cuefile.cue");
                var binPath = Path.Combine(tempPath.TempPath, "binfile.bin");

                // Normal import
                FileSystem.WriteStringToFile(cuePath, @"FILE ""binfile.bin"" BINARY");
                FileSystem.CreateFile(binPath);

                var scanResults = new Dictionary<string, List<ScannedRom>>();
                scanner.ScanDirectoryBase(
                    tempPath.TempPath,
                    new List<string> { "bin", "cue" },
                    null,
                    scanResults,
                    CancellationToken.None,
                    null,
                    true,
                    true);

                Assert.AreEqual(1, scanResults.Count);
                Assert.IsTrue(scanResults.ContainsKey("cuefile"));

                // Case sensitivity test
                FileSystem.WriteStringToFile(cuePath, @"FILE ""BINFILE.BIN"" BINARY");
                scanResults = new Dictionary<string, List<ScannedRom>>();
                scanner.ScanDirectoryBase(
                    tempPath.TempPath,
                    new List<string> { "bin", "cue" },
                    null,
                    scanResults,
                    CancellationToken.None,
                    null,
                    true,
                    true);

                Assert.AreEqual(1, scanResults.Count);
                Assert.IsTrue(scanResults.ContainsKey("cuefile"));

                // Playlist already imported test
                scanResults = new Dictionary<string, List<ScannedRom>>();
                scanner.importedFiles = new HashSet<string> { cuePath };
                scanner.ScanDirectoryBase(
                  tempPath.TempPath,
                  new List<string> { "bin", "cue" },
                  null,
                  scanResults,
                  CancellationToken.None,
                  null,
                  true,
                  true);

                Assert.AreEqual(0, scanResults.Count);
            }
        }
    }
}
