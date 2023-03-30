using System.IO;

namespace Playnite.Tests;

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
        var xml1 = File.ReadAllText(Path.Combine(TestVars.ResourcesDir, "XmlTest", "Xml1.xml"));
        var xml2 = File.ReadAllText(Path.Combine(TestVars.ResourcesDir, "XmlTest", "Xml2.xml"));
        var xml3 = File.ReadAllText(Path.Combine(TestVars.ResourcesDir, "XmlTest", "Xml3.xml"));
        var xml4 = File.ReadAllText(Path.Combine(TestVars.ResourcesDir, "XmlTest", "Xml4.xml"));

        Assert.IsTrue(Xml.AreEqual(xml1, xml2));
        Assert.IsFalse(Xml.AreEqual(xml1, xml3));
        Assert.IsFalse(Xml.AreEqual(xml1, xml4));
    }
}
