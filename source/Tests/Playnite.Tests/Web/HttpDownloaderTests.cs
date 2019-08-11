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
            Assert.AreEqual(HttpStatusCode.OK, HttpDownloader.GetResponseCode(@"https://playnite.link/favicon.ico"));
            Assert.AreEqual(HttpStatusCode.NotFound, HttpDownloader.GetResponseCode(@"https://playnite.link/test.tst"));
        }
    }
}
