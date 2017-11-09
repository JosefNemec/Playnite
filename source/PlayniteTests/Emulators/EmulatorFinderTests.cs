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

        // TODO mock this, don't use real file system
        [Test]
        public void SearchForGamesTest()
        {
            var def = EmulatorDefinition.GetDefinitions().First(a => a.Name == "PCSX2");
            var games = EmulatorFinder.SearchForGames(@"d:\EmulatedGames\PS2\", def.Profiles.First().ToEmulatorConfig());
            CollectionAssert.IsNotEmpty(games);
        }
    }
}
