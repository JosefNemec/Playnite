using NUnit.Framework;
using Steam;
using SteamLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamLibrary.Tests.Services
{
    [TestFixture]
    public class SteamApiClientTests
    {
        [Test]
        public void GetProductInfoTest()
        {
            var client = new SteamApiClient();
            var data = client.GetProductInfo(214490).GetAwaiter().GetResult();
            Assert.IsTrue(!string.IsNullOrEmpty(data["common"]["name"].Value));
        }
    }
}
