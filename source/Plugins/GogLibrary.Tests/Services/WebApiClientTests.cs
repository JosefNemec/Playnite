using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GogLibrary.Services;
using NUnit.Framework;

namespace GogLibrary.Tests.Services
{
    [TestFixture]
    public class WebApiClientTests
    {
        [Test]
        public void GetGameDetailsTest()
        {
            var client = new GogApiClient();
            var existingDetails = client.GetGameDetails("2");
            Assert.IsNotNull(existingDetails);

            var nonexistingDetails = client.GetGameDetails("99999999");
            Assert.IsNull(nonexistingDetails);
        }

        [Test]
        public void GetGameStoreDataTest()
        {
            var client = new GogApiClient();
            var existingStore = client.GetGameStoreData(@"https://www.gog.com/game/vampire_the_masquerade_bloodlines");
            Assert.IsNotNull(existingStore);

            var noneexistingStore = client.GetGameDetails("https://www.gog.com/game/vampire");
            Assert.IsNull(noneexistingStore);
        }
    }
}
