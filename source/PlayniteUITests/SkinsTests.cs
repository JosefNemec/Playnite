using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlayniteUI;
using Playnite;

namespace PlayniteUITests
{
    [TestFixture]
    public class SkinsTests
    {
        [Test]
        public void AvailableSkinsTest()
        {
            CollectionAssert.IsNotEmpty(Themes.AvailableThemes);
        }
    }
}
