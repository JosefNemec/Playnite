using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NUnit.Framework;
using Playnite.Common;

namespace Playnite.Tests
{
    [TestFixture]
    public class PathsTests
    {
        [Test]
        public void GetFinalPathNameTest()
        {
            Assert.AreEqual(@"C:\Users", Paths.GetFinalPathName(@"c:\Documents and Settings"));
            Assert.AreEqual(@"C:\Users", Paths.GetFinalPathName(@"C:\Users"));
            Assert.AreEqual(@"\\server\someunc\testpath", Paths.GetFinalPathName(@"\\server\someunc\testpath"));
        }

        [Test]
        public void GetValidFilePathTest()
        {
            Assert.IsTrue(Paths.IsValidFilePath(@"test.db"));
            Assert.IsTrue(Paths.IsValidFilePath(@"c:\test.db"));
            Assert.IsTrue(Paths.IsValidFilePath(@"..\test.db"));

            Assert.IsFalse(Paths.IsValidFilePath(@"c:\test"));
            Assert.IsFalse(Paths.IsValidFilePath(@"q:\test.db"));
            Assert.IsFalse(Paths.IsValidFilePath(string.Empty));
            Assert.IsFalse(Paths.IsValidFilePath(@"test"));
            Assert.IsFalse(Paths.IsValidFilePath(@"..\test"));
        }

        [Test]
        public void FixSeparatorsTest()
        {
            Assert.AreEqual(@"D:\GOG Games\AAAlbion\DOSBOX\dosbox.exe", Paths.FixSeparators(@"D:/GOG Games//AAAlbion\\\DOSBOX/\dosbox.exe"));
            Assert.AreEqual(@"D:\GOG Games\Albion\DOSBOX\dosbox.exe", Paths.FixSeparators(@"D:\GOG Games\Albion\DOSBOX\dosbox.exe"));
            Assert.AreEqual(@"\\unc\test\path\dosbox.exe", Paths.FixSeparators(@"\\unc\test/path\\dosbox.exe"));
            Assert.AreEqual(@"\\unc\test\path\dosbox.exe", Paths.FixSeparators(@"\\unc\test\path\dosbox.exe"));
            Assert.AreEqual(@"D:\これはユニコードパステストです\dosbox.exe", Paths.FixSeparators(@"D:/これはユニコードパステストです\/\dosbox.exe"));
        }

        [Test]
        public void AreEqualTest()
        {
            Assert.IsTrue(Paths.AreEqual(@"c:\test", @"c:\TesT"));
            Assert.IsTrue(Paths.AreEqual("test", "TesT"));
            Assert.IsTrue(Paths.AreEqual(@"c:\test\", @"c:\TesT"));
            Assert.IsTrue(Paths.AreEqual(@"c:/test/", @"c:\TesT"));
            Assert.IsTrue(Paths.AreEqual(@"..\test\", @"..\TesT"));
            Assert.IsTrue(Paths.AreEqual(@".\test\", @"TesT"));
            Assert.IsTrue(Paths.AreEqual(@"\\unc\test\", @"\\UNC\TesT"));
            Assert.IsTrue(Paths.AreEqual(@"file.exe", @".\file.exe"));

            Assert.IsFalse(Paths.AreEqual(@"file2.exe", @".\file.exe"));
            Assert.IsFalse(Paths.AreEqual(@"c:\file.exe", @"d:\file.exe"));
            Assert.IsFalse(Paths.AreEqual(@"c:\file:?.exe", @"d:\file.exe"));
        }

        [Test]
        public void IsFullPathTest()
        {
            Assert.IsTrue(Paths.IsFullPath(@"c:\test"));
            Assert.IsTrue(Paths.IsFullPath(@"c:\test\test.exe"));
            Assert.IsTrue(Paths.IsFullPath(@"c:\test\"));
            Assert.IsTrue(Paths.IsFullPath(@"c:\test\..\test.exe"));
            Assert.IsTrue(Paths.IsFullPath(@"\\server\share\test.exe"));
            Assert.IsFalse(Paths.IsFullPath(@"test"));
            Assert.IsFalse(Paths.IsFullPath(@"\test"));
            Assert.IsFalse(Paths.IsFullPath(@".\test"));
            Assert.IsFalse(Paths.IsFullPath(@"test.exe"));
            Assert.IsFalse(Paths.IsFullPath(@"\test.exe"));
            Assert.IsFalse(Paths.IsFullPath(@".\test.exe"));
        }

        [Test]
        public void GetSafeFilenameTest()
        {
            Assert.AreEqual("test aaa", Paths.GetSafePathName("test >> aaa "));
        }

        [Test]
        public void GetCommonDirectoryTest()
        {
            Assert.AreEqual(@"c:\test\", Paths.GetCommonDirectory(new string[]
            {
                @"c:\test\file.exe",
                @"c:\test\file2.exe",
                @"c:\test\",
            }));

            Assert.AreEqual(@"c:\test\file\", Paths.GetCommonDirectory(new string[]
            {
                @"c:\test\file\aa.exe",
                @"c:\test\file\bb.ee",
                @"c:\test\file\cc.ss",
            }));

            Assert.AreEqual(@"c:\", Paths.GetCommonDirectory(new string[]
            {
                @"c:\file1.aa",
                @"c:\file1.aa",
                @"c:\file1.aa",
            }));

            Assert.AreEqual(string.Empty, Paths.GetCommonDirectory(new string[]
            {
                @"c:\test\file1",
                @"d:\test\file2",
                @"e:\test\file",
            }));

            Assert.AreEqual(string.Empty, Paths.GetCommonDirectory(new string[]
{
                @"c:\test\file1",
                @"d:\test1\aa\file2",
                @"e:\test2\file",
            }));
        }

        [Test]
        public void MathcesFilePattern()
        {
            Assert.IsTrue(Paths.MathcesFilePattern(@"c:\test\aa.txt", "*.txt"));
            Assert.IsTrue(Paths.MathcesFilePattern(@"c:\test\aa.txt", "*.TXT"));
            Assert.IsFalse(Paths.MathcesFilePattern(@"c:\test\aa.txt", "*.doc"));
            Assert.IsTrue(Paths.MathcesFilePattern(@"c:\test\aa.txt", @"c:\**\*.txt"));
            Assert.IsFalse(Paths.MathcesFilePattern(@"c:\test\aa.txt", @"d:\**\*.txt"));
            Assert.IsTrue(Paths.MathcesFilePattern(@"c:\test\aa.txt", ".doc;*.txt"));
        }

        [Test]
        public void FixPathLengthTests()
        {
            Assert.AreEqual(
                @"\\?\d:\Users\dasdasdasd_opgjx5r\AppData\Roaming\Playnite\ExtensionsData\00000002-dbd1-46c6-b5d0-b1ba559d10e4\catalogcache\0ded1556336546bf849c9d28878ec86c_e24cb0ee3d1c4279b60100178b73db42_20210929-143303-REL_UPDATE_2_1010972_Shipping_1010972-deploy-20210930-092520.json",
                Paths.FixPathLength(
                    @"d:\Users\dasdasdasd_opgjx5r\AppData\Roaming\Playnite\ExtensionsData\00000002-dbd1-46c6-b5d0-b1ba559d10e4\catalogcache\0ded1556336546bf849c9d28878ec86c_e24cb0ee3d1c4279b60100178b73db42_20210929-143303-REL_UPDATE_2_1010972_Shipping_1010972-deploy-20210930-092520.json"));
            Assert.AreEqual(
                @"\\?\UNC\server\share\Users\dasdasdasd_opgjx5r\AppData\Roaming\Playnite\ExtensionsData\00000002-dbd1-46c6-b5d0-b1ba559d10e4\catalogcache\0ded1556336546bf849c9d28878ec86c_e24cb0ee3d1c4279b60100178b73db42_20210929-143303-REL_UPDATE_2_1010972_Shipping_1010972-deploy-20210930-092520.json",
                Paths.FixPathLength(
                    @"\\server\share\Users\dasdasdasd_opgjx5r\AppData\Roaming\Playnite\ExtensionsData\00000002-dbd1-46c6-b5d0-b1ba559d10e4\catalogcache\0ded1556336546bf849c9d28878ec86c_e24cb0ee3d1c4279b60100178b73db42_20210929-143303-REL_UPDATE_2_1010972_Shipping_1010972-deploy-20210930-092520.json"));
        }
    }
}
