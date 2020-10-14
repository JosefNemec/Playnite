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
    }
}
