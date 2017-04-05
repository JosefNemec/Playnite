using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Playnite.Providers.Steam;

namespace PlayniteTests.Providers.Steam
{
    [TestFixture]
    public class WebApiClientTests
    {
        [Test]
        public void GetStoreAppDetailTest()
        {
            var data = WebApiClient.GetStoreAppDetail(214490);
            Assert.IsTrue(!string.IsNullOrEmpty(data.name));
        }
    }
}
