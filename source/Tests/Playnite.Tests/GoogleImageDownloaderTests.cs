using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    [TestFixture]
    public class GoogleImageDownloaderTests
    {
        [Test]
        public async Task GenericTest()
        {
            // TODO doesn't work in unit test since we started using webview for this.
            Assert.Fail();
            //var downloader = new GoogleImageDownloader();
            //var images = await downloader.GetImages("quake background");
            //CollectionAssert.IsNotEmpty(images);
        }
    }
}
