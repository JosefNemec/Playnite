using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Playnite.Providers.Steam;

namespace PlayniteTests.Providers.Steam
{
    [TestClass()]
    public class WebApiClientTests
    {
        [TestMethod]
        public void GetStoreAppDetailTest()
        {
            var data = WebApiClient.GetStoreAppDetail(214490);
            Assert.IsTrue(!string.IsNullOrEmpty(data.name));
        }
    }
}
