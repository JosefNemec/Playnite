using NUnit.Framework;
using Playnite;
using Playnite.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    [TestFixture]
    public class SafeFileEnumeratorTests
    {
        [Test]
        public void OverUnsafeWorksTest()
        {
            var path = @"c:\Windows\appcompat\";

            var dirInfo = new System.IO.DirectoryInfo(path);
            Assert.Throws<UnauthorizedAccessException>(() => dirInfo.GetFiles("*.*", SearchOption.AllDirectories));

            var enumerator = new SafeFileEnumerator(path, "*.*", SearchOption.AllDirectories);
            Assert.DoesNotThrow(() => enumerator.ToList());
        }

        [Test]
        public void StandardEnumTest()
        {
            var path = @"c:\Windows\appcompat\";
            var enumerator = new SafeFileEnumerator(path, "*.*", SearchOption.AllDirectories);
            var files = enumerator.ToList();
            CollectionAssert.IsNotEmpty(files);
        }
    }
}
