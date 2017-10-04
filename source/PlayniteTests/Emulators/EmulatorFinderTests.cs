using NUnit.Framework;
using Playnite.Emulators;
using Playnite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.Emulators
{
    [TestFixture]
    public class EmulatorFinderTest
    {
        [Test]
        public void SearchForEmulatorsTest()
        {
            var defs = EmulatorDefinition.GetDefinitions();
            var emulators = EmulatorFinder.SearchForEmulators(@"d:\Emulators\", defs);
            CollectionAssert.IsNotEmpty(emulators);
        }

        [Test]
        public void SearchForGames()
        {
            var def = EmulatorDefinition.GetDefinitions().First(a => a.Name == "PCSX2");
            var emulator = new Emulator("Test")
            {
                ImageExtensions = def.ImageExtensions
            };

            var games = EmulatorFinder.SearchForGames(@"d:\Emulators\_Games\PS2\", emulator);
            CollectionAssert.IsNotEmpty(games);
        }
    }
}
