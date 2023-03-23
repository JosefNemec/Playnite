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
}
