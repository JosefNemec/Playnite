using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Playnite;

namespace PlayniteTests
{
    [TestClass()]
    public class SettingsTests
    {
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            FileSystem.DeleteFile(Paths.UninstallerPath);
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            FileSystem.DeleteFile(Paths.UninstallerPath);
        }

        [TestMethod()]
        public void PortablePathsTest()
        {
            Assert.IsTrue(Settings.IsPortable);
            Assert.AreNotEqual(Paths.UserProgramDataPath, Paths.ConfigRootPath);
            File.WriteAllText(Paths.UninstallerPath, "");

            Assert.IsFalse(Settings.IsPortable);
            Assert.AreEqual(Paths.UserProgramDataPath, Paths.ConfigRootPath);
        }
    }
}
