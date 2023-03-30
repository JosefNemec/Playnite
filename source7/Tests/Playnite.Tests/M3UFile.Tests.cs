using System.IO;

namespace Playnite.Tests;

[TestFixture]
public class M3UFileTests
{
    [Test]
    public void GetEntriesTest()
    {
        var testm3uPath = Path.Combine(TestVars.ResourcesDir, "test.m3u");
        var entires = M3UFile.GetEntries(testm3uPath);
        Assert.AreEqual(14, entires.Count);

        Assert.AreEqual(2, entires[0].Extensions.Count);
        Assert.AreEqual(@"111,The Orichalcon - ""megAsfear (Title)"" [http://doom2.ocremix.org - Delta-Q-Delta]", entires[0].Extensions["#EXTINF"]);
        Assert.AreEqual(@"1234", entires[0].Extensions["#EXTBYT"]);

        Assert.AreEqual(0, entires[2].Extensions.Count);
        Assert.AreEqual(1, entires[3].Extensions.Count);
    }
}
