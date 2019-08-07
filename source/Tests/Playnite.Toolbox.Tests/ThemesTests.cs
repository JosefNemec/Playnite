using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Playnite.Toolbox.Tests
{
    [TestFixture]
    public class ThemesTests
    {
        [Test]
        public void GetThemeChangelogTest()
        {
            var changelogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "ThemeChangeLog.txt");
            var changelog = Themes.GetThemeChangelog(changelogPath);

            Assert.AreEqual(3, changelog.Count);
            foreach (var ver in changelog.Keys)
            {
                foreach (var file in changelog[ver])
                {
                    StringAssert.DoesNotStartWith("source", file);
                }
            }
        }
    }
}
