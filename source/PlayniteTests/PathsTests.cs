using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NUnit.Framework;
using Playnite;

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
    }
}
