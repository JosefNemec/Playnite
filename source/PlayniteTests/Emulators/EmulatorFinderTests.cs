using NUnit.Framework;
using Playnite.Emulators;
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
    }
}
