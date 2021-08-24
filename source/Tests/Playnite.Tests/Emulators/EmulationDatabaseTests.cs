using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Playnite.Emulators.EmulationDatabase;

namespace Playnite.Tests.Emulators
{
    [TestFixture]
    public class EmulationDatabaseTests
    {
        [Test]
        public void GetByTests()
        {
            var dbFile = Path.Combine(PlayniteTests.ResourcesPath, "Sony - PlayStation Vita.db");
            using (var reader = new EmulationDatabaseReader(dbFile))
            {
                var games = reader.GetByCrc("FDDB3297").ToList();
                Assert.AreEqual(1, games.Count);
                Assert.AreEqual("Terraria (Europe)", games[0].Name);

                games = reader.GetBySerial("PCSE-00244").ToList();
                Assert.AreEqual(1, games.Count);
                Assert.AreEqual("Valhalla Knights 3 (USA)", games[0].Name);

                games = reader.GetByRomName("Unit 13 (Europe) (En,Fr,De,It).vpk").ToList();
                Assert.AreEqual(1, games.Count);
                Assert.AreEqual("PCSF-00068", games[0].Serial);

                games = reader.GetByRomNamePartial("zero escape - the nonary games").ToList();
                Assert.AreEqual(1, games.Count);
                Assert.AreEqual("Zero Escape - The Nonary Games (USA)", games[0].Name);
            }
        }
    }
}
