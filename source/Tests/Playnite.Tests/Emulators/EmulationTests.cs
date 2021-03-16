using NUnit.Framework;
using Playnite.Emulation;
using Playnite.Emulators;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Playnite.Emulation.EmulationDatabase;

namespace Playnite.Tests.Emulators
{
    [TestFixture]
    public class EmulationTests
    {
        [Test]
        public void RomNameParsingTest()
        {
            var romName = new RomName("Final Fantasy VII (Europe) (Disc 1)[SCES-00867]");
            Assert.AreEqual("Disc 1", romName.DiscName);
            Assert.AreEqual("Final Fantasy VII", romName.SanitizedName);
            Assert.AreEqual("Europe", romName.Properties[0]);
            Assert.AreEqual("Disc 1", romName.Properties[1]);
            Assert.AreEqual("SCES-00867", romName.Properties[2]);
        }

        [Test]
        public void TempTest()
        {
            var emu = Scanner.Emulators.First(a => a.Name == "DuckStation").Profiles[0];
            var roms = Scanner.ScanDirectory(@"d:\EmulatedGames\PSX\", emu, @"d:\Downloads\dats\");
            Scanner.CleanupScanResult(roms, emu);
        }
    }
}
