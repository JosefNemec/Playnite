using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PlayniteTests.Providers.Steam
{
    [TestFixture]
    public class SteamApiClientTests
    {
        [Test]
        public void GetProductInfoTest()
        {
            //var data = SteamApiClient.GetProductInfo(214490).GetAwaiter().GetResult();
            //Assert.IsTrue(!string.IsNullOrEmpty(data["common"]["name"].Value));
        }
    }
}
