using System.IO;

namespace Playnite.Tests;

[TestFixture]
public class ImagesTests
{
    [Test]
    public void GetImagePropertiesTest()
    {
        var path = Path.Combine(TestVars.ResourcesDir, "Images", "applogo.png");
        var properties = Images.GetImageProperties(path);
        Assert.AreEqual(256, properties.Height);
        Assert.AreEqual(261, properties.Width);

        var gw2icon = Images.GetImageProperties(Path.Combine(TestVars.ResourcesDir, "Images", "gw2_icon.ico"));
        Assert.AreEqual(256, gw2icon.Height);
        Assert.AreEqual(256, gw2icon.Width);

        var mecicon = Images.GetImageProperties(Path.Combine(TestVars.ResourcesDir, "Images", "mec_icon.ico"));
        Assert.AreEqual(256, mecicon.Height);
        Assert.AreEqual(256, mecicon.Width);
    }
}
