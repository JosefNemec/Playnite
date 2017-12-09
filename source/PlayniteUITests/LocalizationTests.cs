using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Playnite.Models;
using PlayniteUI;
using Playnite;

namespace PlayniteUITests
{
    [TestFixture]
    public class LocalizationTests
    {
        [Test]
        public void AvailableLangsTest()
        {
            CollectionAssert.IsNotEmpty(Localization.AvailableLanguages);
        }
    }
}
