using NUnit.Framework;
using Playnite.Emulators;
using Playnite.Models;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests.Emulators
{
    [TestFixture]
    public class EmulatorFinderTest
    {
        [Test]
        public async Task SearchForEmulatorsTest()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\Emulators\WiiU\cemu_1.10.0\Cemu.exe", MockFileData.NullObject },
                { @"c:\Emulators\Genesis\Fusion364\Fusion.exe", MockFileData.NullObject },
                { @"c:\Emulators\PSP\PPSSPP\PPSSPPWindows.exe", MockFileData.NullObject },
                { @"c:\Emulators\GameCube_Wii\Dolphin\Dolphin.exe", MockFileData.NullObject }
            });

            var dirInfo = new MockDirectoryInfo(fileSystem, @"c:\Emulators\");
            var defs = EmulatorDefinition.GetDefinitions();
            var emulators = await EmulatorFinder.SearchForEmulators(dirInfo, defs);
            Assert.AreEqual(4, emulators.Count);
        }

        [Test]
        public async Task SearchForGamesTest()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\EmulatedGames\PS2\Devil May Cry 3.iso", MockFileData.NullObject },
                { @"c:\EmulatedGames\PS2\Silen Hill 2.iso", MockFileData.NullObject },
                { @"c:\EmulatedGames\PS2\Summoner 2.iso", MockFileData.NullObject },
                { @"c:\EmulatedGames\PS2\WRC II Extreme.ISO", MockFileData.NullObject },
                { @"c:\EmulatedGames\PS2\Xenosaga Episode III - Also sprach Zarathustra (USA) (Disc 1).ISO.gz", MockFileData.NullObject }
            });

            var dirInfo = new MockDirectoryInfo(fileSystem, @"c:\EmulatedGames\");
            var def = EmulatorDefinition.GetDefinitions().First(a => a.Name == "PCSX2");
            var games = await EmulatorFinder.SearchForGames(dirInfo, def.Profiles.First().ToEmulatorConfig());
            Assert.AreEqual(5, games.Count);
            Assert.IsTrue(games[3].Name.EndsWith("Extreme"));
            Assert.IsTrue(games[4].Name.EndsWith("Zarathustra"));
        }
    }
}
