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
            Assert.IsTrue(Programs.IsFileUninstaller("unins000.exe"));
            Assert.IsTrue(Programs.IsFileUninstaller("setup1.exe"));
            Assert.IsTrue(Programs.IsFileUninstaller("setup.exe"));
            Assert.IsFalse(Programs.IsFileUninstaller("test.exe"));
        }

        [Test]
        public void IsPathConfigOrRedistributableTest()
        {
            Assert.IsTrue(Programs.IsFileConfigOrRedistributable("config.exe"));
            Assert.IsTrue(Programs.IsFileConfigOrRedistributable("aConfigFile.exe"));
            Assert.IsTrue(Programs.IsFileConfigOrRedistributable("DXSETUP.exe"));
            Assert.IsTrue(Programs.IsFileConfigOrRedistributable("vc_redist.x64.exe"));
            Assert.IsTrue(Programs.IsFileConfigOrRedistributable("vc_redist.x86.exe"));
            Assert.IsFalse(Programs.IsFileConfigOrRedistributable("test.exe"));
        }

        [Test]
        public void IsPathEngineExecutableTest()
        {
            Assert.IsTrue(Programs.IsFileEngineExecutable("UnityCrashHandler32.exe"));
            Assert.IsTrue(Programs.IsFileEngineExecutable("UnityCrashHandler64.exe"));
            Assert.IsTrue(Programs.IsFileEngineExecutable("notification_helper.exe"));
            Assert.IsTrue(Programs.IsFileEngineExecutable("python.exe"));
            Assert.IsTrue(Programs.IsFileEngineExecutable("pythonw.exe"));
            Assert.IsTrue(Programs.IsFileEngineExecutable("zsync.exe"));
            Assert.IsTrue(Programs.IsFileEngineExecutable("zsyncmake.exe"));
            Assert.IsFalse(Programs.IsFileEngineExecutable("otherPythonFile.exe"));
            Assert.IsFalse(Programs.IsFileEngineExecutable("test.exe"));
        }
    }
}
