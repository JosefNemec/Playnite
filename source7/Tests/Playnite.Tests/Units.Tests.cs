namespace Playnite.Tests;

[TestFixture]
public class TimerTests
{
    [Test]
    public void TimeConvertersTests()
    {
        Assert.AreEqual(1000, Units.SecondsToMilliseconds(1));
        Assert.AreEqual(60000, Units.MinutesToMilliseconds(1));
        Assert.AreEqual(3600000, Units.HoursToMilliseconds(1));
    }
}
