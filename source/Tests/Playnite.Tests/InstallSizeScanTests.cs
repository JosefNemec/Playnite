using NUnit.Framework;
using Playnite.Common;
using Playnite.Controllers;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    [TestFixture]
    public class InstallSizeScanTests
    {
        [Test]
        public void UpdateGameInstallSizeTest()
        {
            // Thread.Sleep(50) is there because the scan can be sometimes so fast that the date would not change
            // despite scan being actually done.

            var resourcesRootDir = Path.Combine(PlayniteTests.ResourcesPath, "SizeScan");
            var playniteSettings = new PlayniteSettings()
            {
                // We don't use size on disk because the value will vary depending
                // on PC drive cluster size
                InstallSizeScanUseSizeOnDisk = false
            };

            var gamesEditor = new GamesEditor(null, new GameControllerFactory(null), playniteSettings, null, null, new TestPlayniteApplication(), null);
            var game = new Game("Test game")
            {
                IsInstalled = true,
                InstallDirectory = resourcesRootDir,
                Roms = new System.Collections.ObjectModel.ObservableCollection<GameRom>()
            };

            game.Roms.Add(new GameRom("RomName", @"{InstallDir}\CueNonExistingFiles.cue"));
            Assert.AreEqual(null, game.InstallSize);
            Assert.AreEqual(null, game.LastSizeScanDate);

            var onlyIfDataMissing = true;
            var updateGameOnLibrary = false;
            var checkLastScanDate = false;

            // Size scan shouldn't happen if rom playlist files do not exist and final rom list is empty
            gamesEditor.UpdateGameSize(game, false, updateGameOnLibrary, checkLastScanDate);
            Assert.AreEqual(null, game.InstallSize);
            Assert.AreEqual(null, game.LastSizeScanDate);

            // Not installed games shouldn't be scanned
            game.IsInstalled = false;
            game.Roms[0].Path = @"{InstallDir}\CueTestFiles.cue";
            Thread.Sleep(50);
            gamesEditor.UpdateGameSize(game, onlyIfDataMissing, updateGameOnLibrary, checkLastScanDate);
            Assert.AreEqual(null, game.InstallSize);
            Assert.AreEqual(null, game.LastSizeScanDate);

            // Installed games should be scanned
            game.IsInstalled = true;
            Thread.Sleep(50);
            gamesEditor.UpdateGameSize(game, onlyIfDataMissing, updateGameOnLibrary, checkLastScanDate);
            Assert.AreEqual(1024, game.InstallSize);
            Assert.AreNotEqual(null, game.LastSizeScanDate);

            // Size scan shouldn't happen if data is not missing and "onlyIfDataMissing" is true
            var previousLastSizeScanDate = game.LastSizeScanDate;
            game.Roms.Add(new GameRom("RomName", @"{InstallDir}\Empty2KbFile.bin"));
            Thread.Sleep(50);
            gamesEditor.UpdateGameSize(game, onlyIfDataMissing, updateGameOnLibrary, checkLastScanDate);
            Assert.AreEqual(1024, game.InstallSize);
            Assert.AreEqual(previousLastSizeScanDate, game.LastSizeScanDate);

            // Size scan should happen if "onlyIfDataMissing" is false
            onlyIfDataMissing = false;
            previousLastSizeScanDate = game.LastSizeScanDate;
            Thread.Sleep(50);
            gamesEditor.UpdateGameSize(game, onlyIfDataMissing, updateGameOnLibrary, checkLastScanDate);
            Assert.AreEqual(3072, game.InstallSize);
            Assert.AreNotEqual(previousLastSizeScanDate, game.LastSizeScanDate);

            // If the game doesn't have roms, the Installation Directory will be used to scan the size
            // the size must be greater than 0 and the previous size, since it will be adding the size
            // of the cue file
            var previousInstallSize = game.InstallSize;
            game.Roms = null;
            game.InstallSize = null;
            game.LastSizeScanDate = null;
            Thread.Sleep(50);
            gamesEditor.UpdateGameSize(game, onlyIfDataMissing, updateGameOnLibrary, checkLastScanDate);
            Assert.AreNotEqual(null, game.InstallSize);
            Assert.Greater(game.InstallSize, previousInstallSize);
            Assert.AreNotEqual(null, game.LastSizeScanDate);

            // Scan shouldn't be done if installation directory does not exists
            game.InstallSize = null;
            game.LastSizeScanDate = null;
            game.InstallDirectory = Path.Combine(resourcesRootDir, "PathThatDoesNotExists");
            Thread.Sleep(50);
            gamesEditor.UpdateGameSize(game, onlyIfDataMissing, updateGameOnLibrary, checkLastScanDate);
            Assert.AreEqual(null, game.InstallSize);
            Assert.AreEqual(null, game.LastSizeScanDate);
        }
    }
}