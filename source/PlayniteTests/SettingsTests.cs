using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NUnit.Framework;
using Playnite;
using Playnite.Settings;

namespace PlayniteTests
{
    [TestFixture]
    public class SettingsTests
    {
        [SetUp]
        public void TestInit()
        {
            FileSystem.DeleteFile(PlaynitePaths.UninstallerInnoPath);
            FileSystem.DeleteFile(PlaynitePaths.UninstallerNsisPath);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            FileSystem.DeleteFile(PlaynitePaths.UninstallerInnoPath);
            FileSystem.DeleteFile(PlaynitePaths.UninstallerNsisPath);
        }

        [Test]
        public void PortableInnoPathsTest()
        {
            Assert.IsTrue(PlayniteSettings.IsPortable);
            Assert.AreNotEqual(PlaynitePaths.UserProgramDataPath, PlaynitePaths.ConfigRootPath);

            File.WriteAllText(PlaynitePaths.UninstallerInnoPath, "");
            Assert.IsFalse(PlayniteSettings.IsPortable);
            Assert.AreEqual(PlaynitePaths.UserProgramDataPath, PlaynitePaths.ConfigRootPath);
        }

        [Test]
        public void PortableNsisPathsTest()
        {
            Assert.IsTrue(PlayniteSettings.IsPortable);
            Assert.AreNotEqual(PlaynitePaths.UserProgramDataPath, PlaynitePaths.ConfigRootPath);

            File.WriteAllText(PlaynitePaths.UninstallerNsisPath, "");
            Assert.IsFalse(PlayniteSettings.IsPortable);
            Assert.AreEqual(PlaynitePaths.UserProgramDataPath, PlaynitePaths.ConfigRootPath);
        }

        [Test]
        public void PortableBothPathsTest()
        {
            Assert.IsTrue(PlayniteSettings.IsPortable);
            Assert.AreNotEqual(PlaynitePaths.UserProgramDataPath, PlaynitePaths.ConfigRootPath);

            File.WriteAllText(PlaynitePaths.UninstallerNsisPath, "");
            File.WriteAllText(PlaynitePaths.UninstallerInnoPath, "");
            Assert.IsFalse(PlayniteSettings.IsPortable);
            Assert.AreEqual(PlaynitePaths.UserProgramDataPath, PlaynitePaths.ConfigRootPath);
        }
    }
}
