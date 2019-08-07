using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NUnit.Framework;
using Playnite.Common;

namespace Playnite.Toolbox.Tests
{
    [TestFixture]
    public class PathsTests
    {
        [Test]
        public void GetNextBackupFolderTest()
        {
            using (var temp = TempDirectory.Create())
            {
                var backupPath = Paths.GetNextBackupFolder(temp.TempPath);
                Assert.AreEqual("backup_0", Path.GetFileName(backupPath));
                Directory.CreateDirectory(backupPath);
                backupPath = Paths.GetNextBackupFolder(temp.TempPath);
                Assert.AreEqual("backup_1", Path.GetFileName(backupPath));
                Directory.CreateDirectory(Path.Combine(temp.TempPath, "backup_20"));
                backupPath = Paths.GetNextBackupFolder(temp.TempPath);
                Assert.AreEqual("backup_21", Path.GetFileName(backupPath));
            }
        }
    }
}
