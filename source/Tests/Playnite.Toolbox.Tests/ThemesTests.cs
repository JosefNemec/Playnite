using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Playnite.SDK;

namespace Playnite.Toolbox.Tests
{
    [TestFixture]
    public class ThemesTests
    {
        [Test]
        public void GetThemeChangelogTest()
        {
            var changelogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Changelog");
            var changelog = Themes.GetThemeChangelog(new Version("1.1.0"), ApplicationMode.Desktop, changelogPath);
            Assert.AreEqual(7, changelog.Count);
            StringAssert.EndsWith("2.xaml", changelog[0].Path);
            StringAssert.EndsWith("3.xaml", changelog[1].Path);
            StringAssert.EndsWith("4.xaml", changelog[2].Path);
            StringAssert.EndsWith("5.xaml", changelog[3].Path);
            StringAssert.EndsWith("1.xaml", changelog[4].Path);
            StringAssert.EndsWith("6.xaml", changelog[5].Path);
            StringAssert.EndsWith("7.xaml", changelog[6].Path);

            changelog = Themes.GetThemeChangelog(new Version("1.2.0"), ApplicationMode.Desktop, changelogPath);
            Assert.AreEqual(5, changelog.Count);

            changelog = Themes.GetThemeChangelog(new Version("1.3.0"), ApplicationMode.Desktop, changelogPath);
            Assert.AreEqual(3, changelog.Count);

            changelog = Themes.GetThemeChangelog(new Version("1.2.0"), ApplicationMode.Fullscreen, changelogPath);
            Assert.AreEqual(4, changelog.Count);
        }
    }
}
