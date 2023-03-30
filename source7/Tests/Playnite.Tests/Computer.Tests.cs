namespace Playnite.Tests;

[TestFixture]
public class ComputerTests
{
    [Test]
    public void DeviceFriendlyNameTest()
    {
        var screen = Screen.PrimaryScreen;
        Assert.IsNotNull(screen);
        var name = Computer.DeviceFriendlyName(screen);

        Assert.IsFalse(name.IsNullOrEmpty());
        Assert.AreNotEqual(screen.DeviceName, name);
    }

    [Test]
    public void GetMonitorsTest()
    {
        var screens = Computer.GetScreens();
        CollectionAssert.IsNotEmpty(screens);
        Assert.IsTrue(screens.Any(a => a.Primary));
    }

    [Test]
    public void GetSystemInfoTest()
    {
        var info = Computer.GetSystemInfo();
        CollectionAssert.IsNotEmpty(info.Gpus);
        Assert.IsFalse(string.IsNullOrEmpty(info.Cpu));
        Assert.IsFalse(string.IsNullOrEmpty(info.WindowsVersion));
        Assert.AreNotEqual(0, info.Ram);
    }
}
