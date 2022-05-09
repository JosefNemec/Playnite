using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Playnite.Common;

namespace Playnite.Tests
{
    [TestFixture]
    public class ProgramsTests
    {
        [Test]
        public async Task GetInstalledProgramsTest()
        {
            var apps = await Programs.GetInstalledPrograms(CancellationToken.None);
            Assert.AreNotEqual(apps.Count, 0);

            var firstApp = apps.First();
            Assert.IsFalse(string.IsNullOrEmpty(firstApp.Icon));
            Assert.IsFalse(string.IsNullOrEmpty(firstApp.Name));
            Assert.IsFalse(string.IsNullOrEmpty(firstApp.Path));
        }

        [Test]
        public async Task GetExecutablesFromFolderTest()
        {
            var apps = await Programs.GetExecutablesFromFolder(@"c:\Program Files\", System.IO.SearchOption.AllDirectories, CancellationToken.None);
            Assert.AreNotEqual(apps.Count, 0);

            var firstApp = apps.First();
            Assert.IsFalse(string.IsNullOrEmpty(firstApp.Icon));
            Assert.IsFalse(string.IsNullOrEmpty(firstApp.Name));
            Assert.IsFalse(string.IsNullOrEmpty(firstApp.Path));
            Assert.IsFalse(string.IsNullOrEmpty(firstApp.WorkDir));
        }

        [Test]
        public void GetUWPAppsTest()
        {
            var apps = Programs.GetUWPApps();
            Assert.AreNotEqual(apps.Count, 0);

            var firstApp = apps.First();
            Assert.IsFalse(string.IsNullOrEmpty(firstApp.Icon));
            Assert.IsFalse(string.IsNullOrEmpty(firstApp.Name));
            Assert.IsFalse(string.IsNullOrEmpty(firstApp.Path));
        }

        [Test]
        public void IsPathUninstallerTest()
        {
            // Uninstallers
            Assert.IsTrue(Programs.IsFileUninstaller("unins000.exe"));
            Assert.IsTrue(Programs.IsFileUninstaller("setup1.exe"));
            Assert.IsTrue(Programs.IsFileUninstaller("setup.exe"));
            Assert.IsFalse(Programs.IsFileUninstaller("test.exe"));

            // Config executables and Redistributables
            Assert.IsTrue(Programs.IsFileUninstaller("config.exe"));
            Assert.IsTrue(Programs.IsFileUninstaller("aConfigFile.exe"));
            Assert.IsTrue(Programs.IsFileUninstaller("DXSETUP.exe"));
            Assert.IsTrue(Programs.IsFileUninstaller("vc_redist.x64.exe"));
            Assert.IsTrue(Programs.IsFileUninstaller("vc_redist.x86.exe"));

            // Game engines executables
            Assert.IsTrue(Programs.IsFileUninstaller("UnityCrashHandler32.exe"));
            Assert.IsTrue(Programs.IsFileUninstaller("UnityCrashHandler64.exe"));
            Assert.IsTrue(Programs.IsFileUninstaller("notification_helper.exe"));
            Assert.IsTrue(Programs.IsFileUninstaller("python.exe"));
            Assert.IsTrue(Programs.IsFileUninstaller("pythonw.exe"));
            Assert.IsTrue(Programs.IsFileUninstaller("zsync.exe"));
            Assert.IsTrue(Programs.IsFileUninstaller("zsyncmake.exe"));
            Assert.IsFalse(Programs.IsFileUninstaller("otherPythonFile.exe"));
        }
    }
}
