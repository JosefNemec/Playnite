using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NUnit.Framework;
using Playnite;
using Playnite.Settings;
using System.Net;
using Playnite.Common.Web;

namespace Playnite.Tests.Web
{
    [TestFixture]
    public class HttpDownloaderTests
    {
        [Test]
        public void GetResponseCodeTest()
        {
            var resp = HttpDownloader.GetResponseCode(@"https://playnite.link/favicon.ico", out var headers);
            Assert.AreEqual(HttpStatusCode.OK, resp);
            Assert.AreEqual("15086", headers["Content-Length"]);

            resp = HttpDownloader.GetResponseCode(@"https://playnite.link/test.tst", out headers);
            Assert.AreEqual(HttpStatusCode.NotFound, resp);
        }
    }
}
