using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Playnite.Common.System;

namespace PlayniteTests
{
    [TestFixture]
    public class ProgramsTests
    {
        [Test]
        public async Task GetInstalledProgramsTest()
        {
            var apps = await Programs.GetInstalledPrograms();
            Assert.AreNotEqual(apps.Count, 0);

            var firstApp = apps.First();
            Assert.IsFalse(string.IsNullOrEmpty(firstApp.Icon));
            Assert.IsFalse(string.IsNullOrEmpty(firstApp.Name));
            Assert.IsFalse(string.IsNullOrEmpty(firstApp.Path));
        }

        [Test]
        public async Task GetExecutablesFromFolderTest()
        {
            var apps = await Programs.GetExecutablesFromFolder(@"c:\Program Files\", System.IO.SearchOption.AllDirectories);
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
    }
}
