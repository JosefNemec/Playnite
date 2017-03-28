using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Playnite.Providers.GOG;

namespace PlayniteTests.Providers.GOG
{
    [TestClass()]
    public class WebApiClientTests
    {
        [TestMethod()]
        public void GetGameDetailsTest()
        {
            var existingDetails = WebApiClient.GetGameDetails("2");
            Assert.IsNotNull(existingDetails);

            var nonexistingDetails = WebApiClient.GetGameDetails("99999999");
            Assert.IsNull(nonexistingDetails);
        }

        [TestMethod()]
        public void GetGameStoreDataTest()
        {
            var existingStore = WebApiClient.GetGameStoreData(@"https://www.gog.com/game/vampire_the_masquerade_bloodlines");
            Assert.IsNotNull(existingStore);

            var noneexistingStore = WebApiClient.GetGameDetails("https://www.gog.com/game/vampire");
            Assert.IsNull(noneexistingStore);
        }
    }
}
