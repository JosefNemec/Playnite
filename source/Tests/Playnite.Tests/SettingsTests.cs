using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NUnit.Framework;
using Playnite;
using Playnite.Settings;
using Playnite.Common;

namespace Playnite.Tests
{
    [TestFixture]
    public class SettingsTests
    {
        [SetUp]
        public void TestInit()
        {
            FileSystem.DeleteFile(PlaynitePaths.UninstallerPath);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            FileSystem.DeleteFile(PlaynitePaths.UninstallerPath);
        }

        [Test]
        public void PortableInnoPathsTest()
        {
            Assert.IsTrue(PlayniteSettings.IsPortable);
            Assert.AreNotEqual(PlaynitePaths.UserProgramDataPath, PlaynitePaths.ConfigRootPath);

            File.WriteAllText(PlaynitePaths.UninstallerPath, "");
            Assert.IsFalse(PlayniteSettings.IsPortable);
            Assert.AreEqual(PlaynitePaths.UserProgramDataPath, PlaynitePaths.ConfigRootPath);
        }
    }
}
