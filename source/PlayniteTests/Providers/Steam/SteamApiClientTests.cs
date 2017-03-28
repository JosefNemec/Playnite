using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Playnite.Providers.Steam;

namespace PlayniteTests.Providers.Steam
{
    [TestClass]
    public class SteamApiClientTests
    {
        [TestMethod]
        public void GetProductInfoTest()
        {
            var data = SteamApiClient.GetProductInfo(214490).GetAwaiter().GetResult();
            Assert.IsTrue(!string.IsNullOrEmpty(data["common"]["name"].Value));
        }
    }
}
