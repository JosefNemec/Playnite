using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using NUnit.Framework;
using Playnite;
using Playnite.Common;
using System.IO;

namespace Playnite.Tests
{
    [TestFixture]
    public class XmlTets
    {
        [Test]
        public static void SimpleAreEqualStringTest()
        {
            Assert.IsTrue(Xml.AreEqual(
                @"<t><a att='aa'  ett='bb'>11</a></t>",
                @"<t><a att='aa' ett='bb'>11</a></t>"));
            Assert.IsTrue(Xml.AreEqual(
                @"<t><a att='aa' ett='bb'>11</a></t>",
                @"<t><a ett='bb' att='aa'>11</a></t>"));
            Assert.IsTrue(Xml.AreEqual(
                @"<t><a att='aa'    ett='bb'>11</a>  </t>",
                @"<t>  <a ett='bb' att='aa'>11</a></t>"));

            Assert.IsFalse(Xml.AreEqual(
                @"<t><a att='aa' ett='bb'>11</a></t>",
                @"<t><a att='aa' ett='bb'>22</a></t>"));
            Assert.IsFalse(Xml.AreEqual(
                @"<t><a att='aa' ett='aa'>11</a></t>",
                @"<t><a ett='bb' att='aa'>11</a></t>"));
            Assert.IsFalse(Xml.AreEqual(
                @"<t><a att='aa'    ett='bb'>11</a>  </t>",
                @"<t>  <b ett='bb' att='aa'>11</b></t>"));            
        }

        [Test]
        public static void XamlsAreEqualStringTest()
        {
            var xml1 = File.ReadAllText(Path.Combine(PlayniteTests.ResourcesPath, "XmlTest", "Xml1.xaml"));
            var xml2 = File.ReadAllText(Path.Combine(PlayniteTests.ResourcesPath, "XmlTest", "Xml2.xaml"));
            var xml3 = File.ReadAllText(Path.Combine(PlayniteTests.ResourcesPath, "XmlTest", "Xml3.xaml"));
            var xml4 = File.ReadAllText(Path.Combine(PlayniteTests.ResourcesPath, "XmlTest", "Xml4.xaml"));

            Assert.IsTrue(Xml.AreEqual(xml1, xml2));
            Assert.IsFalse(Xml.AreEqual(xml1, xml3));
            Assert.IsFalse(Xml.AreEqual(xml1, xml4));
        }
    }
}
