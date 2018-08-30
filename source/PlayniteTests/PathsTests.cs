using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NUnit.Framework;
using Playnite.Common.System;

namespace PlayniteTests
{
    [TestFixture]
    public class PathsTests
    {
        [Test]
        public void GetValidFilePathTest()
        {
            Assert.IsTrue(Paths.GetValidFilePath(@"test.db"));
            Assert.IsTrue(Paths.GetValidFilePath(@"c:\test.db"));
            Assert.IsTrue(Paths.GetValidFilePath(@"..\test.db"));

            Assert.IsFalse(Paths.GetValidFilePath(@"c:\test"));
            Assert.IsFalse(Paths.GetValidFilePath(@"q:\test.db"));
            Assert.IsFalse(Paths.GetValidFilePath(string.Empty));
            Assert.IsFalse(Paths.GetValidFilePath(@"test"));
            Assert.IsFalse(Paths.GetValidFilePath(@"..\test"));
        }

        [Test]
        public void FixSeparatorsTest()
        {
            Assert.AreEqual(@"D:\GOG Games\Albion\DOSBOX\dosbox.exe", Paths.FixSeparators(@"D:/GOG Games//Albion\\\DOSBOX\\dosbox.exe"));
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
    }
}
