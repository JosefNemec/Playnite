using NUnit.Framework;
using Playnite.Providers.Origin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.MetaProviders
{
    [TestFixture]
    public class OriginMetadataProviderTests
    {
        [Test]
        public void StandardDownloadTest()
        {
            var provider = new OriginMetadataProvider();
            var data = provider.GetMetadata("Origin.OFR.50.0000557");
            Assert.IsNotNull(data.GameData);
            Assert.IsNotNull(data.Image);
            Assert.IsNotNull(data.GameData.ReleaseDate);
            Assert.IsFalse(string.IsNullOrEmpty(data.GameData.Description));
            CollectionAssert.IsNotEmpty(data.GameData.Publishers);
            CollectionAssert.IsNotEmpty(data.GameData.Developers);
            CollectionAssert.IsNotEmpty(data.GameData.Genres);
            CollectionAssert.IsNotEmpty(data.GameData.Links);
            CollectionAssert.IsNotEmpty(data.GameData.Publishers);
        }
    }
}
