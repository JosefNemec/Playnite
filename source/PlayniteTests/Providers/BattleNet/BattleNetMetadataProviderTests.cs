using NUnit.Framework;
using Playnite.Providers.BattleNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.MetaProviders
{
    [TestFixture]
    public class BattleNetMetadataProviderTests
    {
        [Test]
        public void StandardDownloadTest()
        {
            var provider = new BattleNetMetadataProvider();
            Assert.IsTrue(provider.GetSupportsIdSearch());
            var data = provider.GetGameData("D3");
            Assert.IsNotNull(data.GameData);
            Assert.IsNotNull(data.Icon);
            Assert.IsNotNull(data.Image);
            Assert.IsFalse(string.IsNullOrEmpty(data.BackgroundImage));
            CollectionAssert.IsNotEmpty(data.GameData.Links);
        }
    }
}
