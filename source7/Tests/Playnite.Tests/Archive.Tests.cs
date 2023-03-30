using System.IO;

namespace Playnite.Tests;

[TestFixture]
public class ArchiveTests
{
    [Test]
    public void GetArchiveFilesTest()
    {
        var files = Archive.GetArchiveFiles(Path.Combine(TestVars.ResourcesDir, "TestZip.zip"));
        Assert.AreEqual(14, files.Count);
        files = Archive.GetArchiveFiles(Path.Combine(TestVars.ResourcesDir, "Test7zip.7z"));
        Assert.AreEqual(14, files.Count);
    }

    [Test]
    public void GetEntryStreamTest()
    {
        var entry = Archive.GetEntryStream(Path.Combine(TestVars.ResourcesDir, "TestZip.zip"), "Archive.cs");
        Assert.IsNotNull(entry);
        using (entry.Item2)
        using (entry.Item1)
        using (var reader = new StreamReader(entry.Item1))
        {
            var text = reader.ReadToEnd();
            StringAssert.StartsWith("using System", text);
        }

        entry = Archive.GetEntryStream(Path.Combine(TestVars.ResourcesDir, "Test7zip.7z"), "Archive.cs");
        Assert.IsNotNull(entry);
        using (entry.Item2)
        using (entry.Item1)
        {
            Assert.AreEqual("37E74AE8", FileSystem.GetCRC32(entry.Item1));
        }
    }
}
