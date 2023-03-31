using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NUnit.Framework;
using Playnite.Common;
using Playnite.Settings;
using Playnite;

namespace Playnite.Tests
{
    [TestFixture]
    public class FileSystemTests
    {
        [Test]
        public void CanWriteToFolderTest()
        {
            Assert.IsTrue(FileSystem.CanWriteToFolder(PlaynitePaths.ProgramPath));
            Assert.IsFalse(FileSystem.CanWriteToFolder(@"c:\Windows\"));
        }

        [Test]
        public void GetFreeSpaceTest()
        {
            Assert.AreNotEqual(0, FileSystem.GetFreeSpace(@"c:\"));
            Assert.AreNotEqual(0, FileSystem.GetFreeSpace(@"c:\test\"));
            Assert.AreNotEqual(0, FileSystem.GetFreeSpace(@"c:\test\file.txt"));
            Assert.AreEqual(0, FileSystem.GetFreeSpace("c"));
            Assert.AreEqual(0, FileSystem.GetFreeSpace("file.txt"));
        }

        [Test]
        public async Task ReadFileAsStringSafeTest()
        {
            var testPath = Path.Combine(PlayniteTests.TempPath, "ReadFileAsStringSafeTest.txt");
            FileSystem.DeleteFile(testPath);
            var fs = new FileStream(testPath, FileMode.Create);
            fs.Write(new byte[] { 1 }, 0, 1);
            Assert.Throws<IOException>(() => FileSystem.ReadFileAsStringSafe(testPath));
            string result = null;
            var task = Task.Run(() => result = FileSystem.ReadFileAsStringSafe(testPath));
            await Task.Delay(1000);
            fs.Dispose();
            await task;
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task WriteStringToFileSafeTest()
        {
            var testPath = Path.Combine(PlayniteTests.TempPath, "WriteStringToFileSafeTest.txt");
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
            var testFile = Path.Combine(PlayniteTests.ResourcesPath, "TestIni.ini");
            StringAssert.AreEqualIgnoringCase("46fcb37aa8e69b4ead0d702fd459299d", FileSystem.GetMD5(testFile));
            StringAssert.AreEqualIgnoringCase("D8B22F5D", FileSystem.GetCRC32(testFile));
        }

        [Test]
        public void DirectorySizeScanTest()
        {
            using (var tempPath = TempDirectory.Create())
            {
                // Subdirectory is used to verify that called methods work correctly in long paths
                var subDirName = new string('a', 255);
                var subDirName2 = new string('b', 30);
                var subdirPath = Path.Combine(tempPath.TempPath, subDirName, subDirName2);
                FileSystem.CreateDirectory(subdirPath);

                var filePath = Path.Combine(subdirPath, "DummyFile");
                var dummyFileLenght = 1024;
                using (var fileStream = new FileStream(Paths.FixPathLength(filePath), FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    fileStream.SetLength(dummyFileLenght);
                }

                // We can't check the exact size because it will vary depending on drive
                // cluster size so we only check if value is higher than zero
                var dirSizeOnDisk = FileSystem.GetDirectorySize(tempPath.TempPath, true);
                Assert.Greater(dirSizeOnDisk, 0);

                var dirSize = FileSystem.GetDirectorySize(tempPath.TempPath, false);
                Assert.AreEqual(dummyFileLenght, dirSize);
            }
        }
    }
}
