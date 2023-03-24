using System.IO;
using System.Reflection;
using Playnite.Common;

namespace Playnite.Tests;

[TestFixture]
public class FileSystemTests
{
    [Test]
    public void GetFileSizeOnDiskTest()
    {
        Assert.That(FileSystem.GetFileSizeOnDisk(Assembly.GetExecutingAssembly().Location), Is.GreaterThan(0));
    }

    [Test]
    public void GetFileSizeTest()
    {
        Assert.That(FileSystem.GetFileSize(Assembly.GetExecutingAssembly().Location), Is.GreaterThan(0));
    }

    [Test]
    public void CanWriteToFolderTest()
    {
        Assert.IsTrue(FileSystem.CanWriteToFolder(PlaynitePaths.ProgramPath));
        Assert.IsFalse(FileSystem.CanWriteToFolder(@"c:\Windows\"));
    }

    [Test]
    public void GetFreeSpaceTest()
    {
        Assert.That(FileSystem.GetFreeSpace(@"c:\"), Is.GreaterThan(0));
        Assert.That(FileSystem.GetFreeSpace(@"c:\test\"), Is.GreaterThan(0));
        Assert.That(FileSystem.GetFreeSpace(@"c:\test\file.txt"), Is.GreaterThan(0));
        Assert.That(FileSystem.GetFreeSpace("c"), Is.EqualTo(0));
        Assert.That(FileSystem.GetFreeSpace("file.txt"), Is.EqualTo(0));
    }

    [Test]
    public async Task ReadFileAsStringSafeTest()
    {
        Assert.Fail();
        //var testPath = Path.Combine(PlayniteTests.TempPath, "ReadFileAsStringSafeTest.txt");
        //FileSystem.DeleteFile(testPath);
        //var fs = new FileStream(testPath, FileMode.Create);
        //fs.Write(new byte[] { 1 }, 0, 1);
        //Assert.Throws<IOException>(() => FileSystem.ReadFileAsStringSafe(testPath));
        //string result = null;
        //var task = Task.Run(() => result = FileSystem.ReadFileAsStringSafe(testPath));
        //await Task.Delay(1000);
        //fs.Dispose();
        //await task;
        //Assert.IsNotNull(result);
    }

    [Test]
    public async Task WriteStringToFileSafeTest()
    {
        Assert.Fail();
        //var testPath = Path.Combine(PlayniteTests.TempPath, "WriteStringToFileSafeTest.txt");
        //FileSystem.DeleteFile(testPath);
        //var fs = new FileStream(testPath, FileMode.Create);
        //Assert.Throws<IOException>(() => FileSystem.WriteStringToFileSafe(testPath, "test"));
        //var task = Task.Run(() => FileSystem.WriteStringToFileSafe(testPath, "test"));
        //await Task.Delay(1000);
        //fs.Dispose();
        //await task;
        //Assert.AreEqual("test", File.ReadAllText(testPath));
    }

    [Test]
    public void CheckSumTests()
    {
        var testFile = Path.Combine(PlayniteTests.ResourcesPath, "TestIni.ini");
        StringAssert.AreEqualIgnoringCase("46fcb37aa8e69b4ead0d702fd459299d", FileSystem.GetMD5(testFile));
        StringAssert.AreEqualIgnoringCase("D8B22F5D", FileSystem.GetCRC32(testFile));
    }

    [Test]
    public void DirectorySizeScanTest()
    {
        Assert.Fail();
        //using (var tempPath = TempDirectory.Create())
        //{
        //    // Subdirectory is used to verify that called methods work correctly in long paths
        //    var subDirName = new string('a', 255);
        //    var subDirName2 = new string('b', 30);
        //    var subdirPath = Path.Combine(tempPath.TempPath, subDirName, subDirName2);
        //    FileSystem.CreateDirectory(subdirPath);

        //    var filePath = Path.Combine(subdirPath, "DummyFile");
        //    var dummyFileLenght = 1024;
        //    using (var fileStream = new FileStream(Paths.FixPathLength(filePath), FileMode.Create, FileAccess.Write, FileShare.None))
        //    {
        //        fileStream.SetLength(dummyFileLenght);
        //    }

        //    // We can't check the exact size because it will vary depending on drive
        //    // cluster size so we only check if value is higher than zero
        //    var dirSizeOnDisk = FileSystem.GetDirectorySize(tempPath.TempPath, true);
        //    Assert.Greater(dirSizeOnDisk, 0);

        //    var dirSize = FileSystem.GetDirectorySize(tempPath.TempPath, false);
        //    Assert.AreEqual(dummyFileLenght, dirSize);
        //}
    }
}
