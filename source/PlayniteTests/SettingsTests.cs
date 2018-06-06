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
        [SetUp]
        public void TestInit()
        {
            FileSystem.DeleteFile(Paths.UninstallerInnoPath);
            FileSystem.DeleteFile(Paths.UninstallerNsisPath);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            FileSystem.DeleteFile(Paths.UninstallerInnoPath);
            FileSystem.DeleteFile(Paths.UninstallerNsisPath);
        }

        [Test]
        public void PortableInnoPathsTest()
        {
            Assert.IsTrue(Settings.IsPortable);
            Assert.AreNotEqual(Paths.UserProgramDataPath, Paths.ConfigRootPath);

            File.WriteAllText(Paths.UninstallerInnoPath, "");
            Assert.IsFalse(Settings.IsPortable);
            Assert.AreEqual(Paths.UserProgramDataPath, Paths.ConfigRootPath);
        }

        [Test]
        public void PortableNsisPathsTest()
        {
            Assert.IsTrue(Settings.IsPortable);
            Assert.AreNotEqual(Paths.UserProgramDataPath, Paths.ConfigRootPath);

            File.WriteAllText(Paths.UninstallerNsisPath, "");
            Assert.IsFalse(Settings.IsPortable);
            Assert.AreEqual(Paths.UserProgramDataPath, Paths.ConfigRootPath);
        }

        [Test]
        public void PortableBothPathsTest()
        {
            Assert.IsTrue(Settings.IsPortable);
            Assert.AreNotEqual(Paths.UserProgramDataPath, Paths.ConfigRootPath);

            File.WriteAllText(Paths.UninstallerNsisPath, "");
            File.WriteAllText(Paths.UninstallerInnoPath, "");
            Assert.IsFalse(Settings.IsPortable);
            Assert.AreEqual(Paths.UserProgramDataPath, Paths.ConfigRootPath);
        }
    }
}
