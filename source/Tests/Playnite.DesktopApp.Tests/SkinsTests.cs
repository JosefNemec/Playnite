using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Playnite;
using Playnite.SDK;
using PlayniteServices.Models.IGDB;

namespace Playnite.DesktopApp.Tests
{
    [TestFixture]
    public class SkinsTests
    {
        [Test]
        public void AvailableSkinsTest()
        {
            CollectionAssert.IsNotEmpty(ThemeManager.GetAvailableThemes(ApplicationMode.Desktop));
        }
    }
}
