using NUnit.Framework;
using Playnite.API;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginLibrary.Tests
{
    [TestFixture]
    public class OriginMetadataProviderTests
    {
        [Test]
        public void StandardDownloadTest()
        {
            var provider = new OriginMetadataProvider(OriginLibraryTests.CreateLibrary());
            var data = provider.GetMetadata(new Game() { GameId = "Origin.OFR.50.0000557" });
            Assert.IsNotNull(data.GameInfo);
            Assert.IsNotNull(data.CoverImage);
            Assert.IsNotNull(data.BackgroundImage);
            Assert.IsNotNull(data.GameInfo.ReleaseDate);
            Assert.IsFalse(string.IsNullOrEmpty(data.GameInfo.Description));
            CollectionAssert.IsNotEmpty(data.GameInfo.Publishers);
            CollectionAssert.IsNotEmpty(data.GameInfo.Developers);
            CollectionAssert.IsNotEmpty(data.GameInfo.Genres);
            CollectionAssert.IsNotEmpty(data.GameInfo.Links);
            CollectionAssert.IsNotEmpty(data.GameInfo.Publishers);
        }
    }
}
