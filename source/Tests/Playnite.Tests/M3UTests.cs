using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    [TestFixture]
    public class M3UTests
    {
        [Test]
        public void GetEntriesTest()
        {
            var testm3uPath = Path.Combine(PlayniteTests.ResourcesPath, "test.m3u");
            var entires = M3U.GetEntries(testm3uPath);
            Assert.AreEqual(14, entires.Count);

            Assert.AreEqual(2, entires[0].Extensions.Count);
            Assert.AreEqual(@"111,The Orichalcon - ""megAsfear (Title)"" [http://doom2.ocremix.org - Delta-Q-Delta]", entires[0].Extensions["#EXTINF"]);
            Assert.AreEqual(@"1234", entires[0].Extensions["#EXTBYT"]);

            Assert.AreEqual(0, entires[2].Extensions.Count);
            Assert.AreEqual(1, entires[3].Extensions.Count);
        }
    }
}
