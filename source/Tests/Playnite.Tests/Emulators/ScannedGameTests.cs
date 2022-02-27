using NUnit.Framework;
using Playnite.Common;
using Playnite.Emulators;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests.Emulators
{
    [TestFixture]
    public class ScannedGameTests
    {
        [Test]
        public void ToGameImportWithRelativeEmuPathsTest()
        {
            var emulator = new Emulator
            {
                InstallDir = @"c:\emudir\"
            };

            var config = new GameScannerConfig
            {
                ImportWithRelativePaths = true
            };

            var scannedGame = new ScannedGame
            {
                Roms = new ObservableCollection<ScannedRom>
                {
                    new ScannedRom(@"c:\emudir\rom1name.iso"),
                    new ScannedRom(@"c:\emudir\games\rom2name.iso")
                },
                SourceEmulator = emulator,
                SourceConfig = config
            };

            var newGame = scannedGame.ToGame();
            Assert.AreEqual(@"{EmulatorDir}\", newGame.InstallDirectory);
            Assert.AreEqual(@"{InstallDir}\rom1name.iso", newGame.Roms[0].Path);
            Assert.AreEqual(@"{InstallDir}\games\rom2name.iso", newGame.Roms[1].Path);

            config.ImportWithRelativePaths = false;
            newGame = scannedGame.ToGame();
            Assert.AreEqual(@"c:\emudir\", newGame.InstallDirectory);
            Assert.AreEqual(@"{InstallDir}\rom1name.iso", newGame.Roms[0].Path);
            Assert.AreEqual(@"{InstallDir}\games\rom2name.iso", newGame.Roms[1].Path);
        }

        [Test]
        public void ToGameImportWithRelativeProgramDirPathsTest()
        {
            var emulator = new Emulator
            {
                InstallDir = @"c:\emudir\"
            };

            var config = new GameScannerConfig
            {
                ImportWithRelativePaths = true
            };

            var programDir = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            var scannedGame = new ScannedGame
            {
                Roms = new ObservableCollection<ScannedRom>
                {
                    new ScannedRom(Path.Combine(programDir, "rom1name.iso")),
                    new ScannedRom(Path.Combine(programDir, @"games\rom2name.iso"))
                },
                SourceEmulator = emulator,
                SourceConfig = config
            };

            var newGame = scannedGame.ToGame();
            Assert.AreEqual(@"{PlayniteDir}\", newGame.InstallDirectory);
            Assert.AreEqual(@"{InstallDir}\rom1name.iso", newGame.Roms[0].Path);
            Assert.AreEqual(@"{InstallDir}\games\rom2name.iso", newGame.Roms[1].Path);

            config.ImportWithRelativePaths = false;
            newGame = scannedGame.ToGame();
            Assert.AreEqual(programDir.TrimEnd(Path.DirectorySeparatorChar), newGame.InstallDirectory.TrimEnd(Path.DirectorySeparatorChar));
            Assert.AreEqual(@"{InstallDir}\rom1name.iso", newGame.Roms[0].Path);
            Assert.AreEqual(@"{InstallDir}\games\rom2name.iso", newGame.Roms[1].Path);

            emulator.InstallDir = @"{PlayniteDir}";
            config.ImportWithRelativePaths = true;
            newGame = scannedGame.ToGame();
            Assert.AreEqual(@"{EmulatorDir}\", newGame.InstallDirectory);
            Assert.AreEqual(@"{InstallDir}\rom1name.iso", newGame.Roms[0].Path);
            Assert.AreEqual(@"{InstallDir}\games\rom2name.iso", newGame.Roms[1].Path);
        }
    }
}
