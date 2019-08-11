using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Playnite;

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
    }
}
