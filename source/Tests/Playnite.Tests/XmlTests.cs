using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using NUnit.Framework;
using Playnite;
using Playnite.Common;

namespace Playnite.Tests
{
    [TestFixture]
    public class XmlTets
    {
        [Test]
        public  static void AreEqualStringTest()
        {
            Assert.IsTrue(Xml.AreEqual(
                @"<t><a att='aa' ett='bb'>11</a></t>",
                @"<t><a att='aa' ett='bb'>11</a></t>"));
            Assert.IsTrue(Xml.AreEqual(
                @"<t><a att='aa' ett='bb'>11</a></t>",
                @"<t><a ett='bb' att='aa'>11</a></t>"));
            Assert.IsTrue(Xml.AreEqual(
                @"<t><a att='aa'    ett='bb'>11</a>  </t>",
                @"<t>  <a ett='bb' att='aa'>11</a></t>"));
        }
    }
}
