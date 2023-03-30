using System.Net;

namespace Playnite.Tests;

[TestFixture]
public class NetExtensionsTests
{
    [Test]
    public void IsSuccessTest()
    {
        Assert.IsTrue(HttpStatusCode.OK.IsSuccess());
        Assert.IsTrue(HttpStatusCode.NoContent.IsSuccess());
        Assert.IsFalse(HttpStatusCode.Continue.IsSuccess());
        Assert.IsFalse(HttpStatusCode.Redirect.IsSuccess());
    }
}
