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

        public enum enum1 : int
        {
            val1 = 0,
            val2 = 1
        }

        public enum enum2 : int
        {
            val1 = 0,
            val2 = 1,
            val3 = 2
        }

        [Test]
        public void enumtest()
        {
            var val1 = enum1.val1;
            var val2 = enum2.val3;

            var test = Enum.IsDefined(val1.GetType(), (int)val2);
        }
    }
}
