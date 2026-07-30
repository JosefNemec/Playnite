using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Playnite;
using Playnite.Controls;

namespace Playnite.Tests
{
    [TestFixture]
    public class LocalizationTests
    {
        [Test]
        public void AvailableLangsTest()
        {
            CollectionAssert.IsNotEmpty(Localization.AvailableLanguages);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void WindowLanguageMatchesApplicationLanguageTest()
        {
            var originalLanguage = Localization.CurrentLanguage;
            try
            {
                Localization.SetLanguage("zh_CN");
                var window = new WindowBase();
                Assert.AreEqual(
                    Localization.ApplicationLanguageCultureInfo.Name,
                    window.Language.GetEquivalentCulture().Name);
            }
            finally
            {
                Localization.SetLanguage(originalLanguage);
            }
        }
    }
}
