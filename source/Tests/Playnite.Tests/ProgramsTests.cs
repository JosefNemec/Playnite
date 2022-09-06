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
        public void IsPathScanExcludedTest()
        {
            // Uninstallers
            Assert.IsTrue(Programs.IsFileScanExcluded("unins000.exe"));
            Assert.IsTrue(Programs.IsFileScanExcluded("setup1.exe"));
            Assert.IsTrue(Programs.IsFileScanExcluded("setup.exe"));
            Assert.IsFalse(Programs.IsFileScanExcluded("test.exe"));

            // Config executables and Redistributables
            Assert.IsTrue(Programs.IsFileScanExcluded("config.exe"));
            Assert.IsTrue(Programs.IsFileScanExcluded("aConfigFile.exe"));
            Assert.IsTrue(Programs.IsFileScanExcluded("DXSETUP.exe"));
            Assert.IsTrue(Programs.IsFileScanExcluded("vc_redist.x64.exe"));
            Assert.IsTrue(Programs.IsFileScanExcluded("vc_redist.x86.exe"));

            // Game engines executables
            Assert.IsTrue(Programs.IsFileScanExcluded("UnityCrashHandler32.exe"));
            Assert.IsTrue(Programs.IsFileScanExcluded("UnityCrashHandler64.exe"));
            Assert.IsTrue(Programs.IsFileScanExcluded("notification_helper.exe"));
            Assert.IsTrue(Programs.IsFileScanExcluded("python.exe"));
            Assert.IsTrue(Programs.IsFileScanExcluded("pythonw.exe"));
            Assert.IsTrue(Programs.IsFileScanExcluded("zsync.exe"));
            Assert.IsTrue(Programs.IsFileScanExcluded("zsyncmake.exe"));
            Assert.IsFalse(Programs.IsFileScanExcluded("otherPythonFile.exe"));
        }
    }
}
