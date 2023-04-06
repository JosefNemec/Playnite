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
        Assert.IsTrue(FileSystem.CanWriteToFolder(PlaynitePaths.ProgramDir));
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
        var testPath = Path.Combine(TestVars.TempDir, "ReadFileAsStringSafeTest.txt");
        FileSystem.DeleteFile(testPath);
        var fs = new FileStream(testPath, FileMode.Create);
        fs.Write(new byte[] { 1 }, 0, 1);
        Assert.Throws<IOException>(() => FileSystem.ReadFileAsStringSafe(testPath));
        string? result = null;
        var task = Task.Run(() => result = FileSystem.ReadFileAsStringSafe(testPath));
        await Task.Delay(1000);
        fs.Dispose();
        await task;
        Assert.IsNotNull(result);
    }

    [Test]
    public async Task WriteStringToFileSafeTest()
    {
        var testPath = Path.Combine(TestVars.TempDir, "WriteStringToFileSafeTest.txt");
        FileSystem.DeleteFile(testPath);
        var fs = new FileStream(testPath, FileMode.Create);
        Assert.Throws<IOException>(() => FileSystem.WriteStringToFileSafe(testPath, "test"));
        var task = Task.Run(() => FileSystem.WriteStringToFileSafe(testPath, "test"));
        await Task.Delay(1000);
        fs.Dispose();
        await task;
        Assert.AreEqual("test", File.ReadAllText(testPath));
    }

    [Test]
    public void CheckSumTests()
    {
        var testFile = Path.Combine(TestVars.ResourcesDir, "Test7zip.7z");
        StringAssert.AreEqualIgnoringCase("6d420aaa961c1b2f5d56d2015a28dde6", FileSystem.GetMD5(testFile));
        StringAssert.AreEqualIgnoringCase("A9B7F8B5", FileSystem.GetCRC32(testFile));
    }

    [Test]
    public void DirectorySizeScanTest()
    {
        using var tempPath = TempDirectory.Create();
        // Subdirectory is used to verify that called methods work correctly in long paths
        var subdirPath = Path.Combine(tempPath.TempDir, GlobalRandom.GetRandomString(255), GlobalRandom.GetRandomString(50));
        FileSystem.CreateDirectory(subdirPath);

        var filePath = Path.Combine(subdirPath, "DummyFile.file");
        var dummyFileLenght = 1024;
        var dummyContent = new byte[dummyFileLenght];
        GlobalRandom.NextBytes(dummyContent);
        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            fileStream.Write(dummyContent, 0, dummyFileLenght);
        }

        // We can't check the exact size because it will vary depending on drive
        // cluster size so we only check if value is higher than zero
        var dirSizeOnDisk = FileSystem.GetDirectorySize(tempPath.TempDir, true);
        Assert.That(dirSizeOnDisk, Is.GreaterThan(0));

        var dirSize = FileSystem.GetDirectorySize(tempPath.TempDir, false);
        Assert.AreEqual(dummyFileLenght, dirSize);
    }

    [Test]
    public void LongPathTest()
    {
        using var tempPath = TempDirectory.Create();
        var longDir = Path.Combine(tempPath.TempDir, GlobalRandom.GetRandomString(255), GlobalRandom.GetRandomString(50));
        var longFile = Path.Combine(longDir, "file.test");

        FileSystem.CreateDirectory(longDir);
        DirectoryAssert.Exists(longDir);

        FileSystem.CreateFile(longFile);
        FileAssert.Exists(longFile);
    }
}
