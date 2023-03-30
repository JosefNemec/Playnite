namespace Playnite.Tests;

public enum TestEnum
{
    [System.ComponentModel.Description("desc1")]
    Test1,
    Test2
}

[TestFixture]
public class EnumsTests
{
    [Test]
    public void Test()
    {
        Assert.AreEqual("desc1", TestEnum.Test1.GetDescription());
        Assert.AreEqual("Test2", TestEnum.Test2.GetDescription());
    }
}
