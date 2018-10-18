using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NUnit.Framework;
using Playnite.Common.System;
using Playnite.Settings;
using Playnite;

namespace PlayniteTests
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
    }
}
