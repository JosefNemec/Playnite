using System.IO;

namespace Playnite.Tests;

[TestFixture]
public class IniParserTests
{
    [Test]
    public void StandardTest()
    {
        var iniStr = File.ReadAllLines(Path.Combine(TestVars.ResourcesDir, "TestIni.ini"));
        var ini = IniFile.Parse(iniStr);
        Assert.AreEqual("true", ini["RegionDoesRequirePermission"]!["GameDev"]);
        Assert.IsNull(ini["test"]?["test2"]);
    }
}
