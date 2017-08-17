using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Playnite;

namespace PlayniteTests
{
    [TestFixture]
    public class ProgramsTests
    {
        [Test]
        public void GetInstalledProgramsTest()
        {
            var apps = Programs.GetInstalledPrograms();
            Assert.AreNotEqual(apps.Count, 0);

            var firstApp = apps.First();
            Assert.IsFalse(string.IsNullOrEmpty(firstApp.Icon));
            Assert.IsFalse(string.IsNullOrEmpty(firstApp.Name));
            Assert.IsFalse(string.IsNullOrEmpty(firstApp.Path));
            Assert.IsFalse(string.IsNullOrEmpty(firstApp.WorkDir));
        }

        [Test]
        public void GetExecutablesFromFolderTest()
        {
            var apps = Programs.GetExecutablesFromFolder(@"c:\windows\system32\", System.IO.SearchOption.TopDirectoryOnly);
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
