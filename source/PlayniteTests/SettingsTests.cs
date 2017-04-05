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
    public class SettingsTests
    {
        [OneTimeSetUp]
        public void Init()
        {
            FileSystem.DeleteFile(Paths.UninstallerPath);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            FileSystem.DeleteFile(Paths.UninstallerPath);
        }

        [Test]
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
