using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.Tests.Extensions
{
    [TestFixture]
    public class BitmapExtensionsTests
    {
        [Test]
        public void BitmapLoadPropertiesEquabilityTest()
        {
            var np1 = new BitmapLoadProperties(1, 1, new DpiScale(1, 1));
            var np2 = new BitmapLoadProperties(1, 1, new DpiScale(1, 1));
            var np3 = new BitmapLoadProperties(2, 2, new DpiScale(2, 2));

            Assert.AreEqual(np1, np2);
            Assert.IsTrue(np1 == np2);

            Assert.AreNotEqual(np1, np3);
            Assert.IsTrue(np1 != np3);
        }
    }
}
