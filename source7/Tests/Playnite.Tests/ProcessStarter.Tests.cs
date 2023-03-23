namespace Playnite.Tests;

[TestFixture]
public class ProcessStarterTests
{
    [Test]
    public void ShellExecuteTest()
    {
        Assert.That(ProcessStarter.ShellExecute("ipconfig"), Is.GreaterThan(0));
        Assert.Fail("Test process args are passed");
    }
}
