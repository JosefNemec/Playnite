using System.Net;

namespace Playnite.Tests;

[TestFixture]
public class HttpDownloaderTests
{
    [Test]
    public async Task GetResponseCodeTest()
    {
        Assert.AreEqual(HttpStatusCode.OK, await Downloader.GetResponseCode(@"https://playnite.link/favicon.ico"));
        Assert.AreEqual(HttpStatusCode.NotFound, await Downloader.GetResponseCode(@"https://playnite.link/test.tst"));
    }

    [Test]
    public async Task DownloadBytesTest()
    {
        Assert.That(await Downloader.DownloadBytes(@"https://playnite.link/favicon.ico"), Is.Not.Empty);
    }

    [Test]
    public async Task DownloadStringTest()
    {
        StringAssert.Contains("<head>", await Downloader.DownloadString(@"https://playnite.link/index.html"));
    }
}