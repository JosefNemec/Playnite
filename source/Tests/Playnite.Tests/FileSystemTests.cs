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
        public void FixPathLengthTests()
        {
            Assert.AreEqual(
                @"\\?\d:\Users\dasdasdasd_opgjx5r\AppData\Roaming\Playnite\ExtensionsData\00000002-dbd1-46c6-b5d0-b1ba559d10e4\catalogcache\0ded1556336546bf849c9d28878ec86c_e24cb0ee3d1c4279b60100178b73db42_20210929-143303-REL_UPDATE_2_1010972_Shipping_1010972-deploy-20210930-092520.json",
                FileSystem.FixPathLength(
                    @"d:\Users\dasdasdasd_opgjx5r\AppData\Roaming\Playnite\ExtensionsData\00000002-dbd1-46c6-b5d0-b1ba559d10e4\catalogcache\0ded1556336546bf849c9d28878ec86c_e24cb0ee3d1c4279b60100178b73db42_20210929-143303-REL_UPDATE_2_1010972_Shipping_1010972-deploy-20210930-092520.json"));
            Assert.AreEqual(
                @"\\?\UNC\server\share\Users\dasdasdasd_opgjx5r\AppData\Roaming\Playnite\ExtensionsData\00000002-dbd1-46c6-b5d0-b1ba559d10e4\catalogcache\0ded1556336546bf849c9d28878ec86c_e24cb0ee3d1c4279b60100178b73db42_20210929-143303-REL_UPDATE_2_1010972_Shipping_1010972-deploy-20210930-092520.json",
                FileSystem.FixPathLength(
                    @"\\server\share\Users\dasdasdasd_opgjx5r\AppData\Roaming\Playnite\ExtensionsData\00000002-dbd1-46c6-b5d0-b1ba559d10e4\catalogcache\0ded1556336546bf849c9d28878ec86c_e24cb0ee3d1c4279b60100178b73db42_20210929-143303-REL_UPDATE_2_1010972_Shipping_1010972-deploy-20210930-092520.json"));
        }
    }
}
