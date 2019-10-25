using IGDBMetadata.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IGDBMetadata.Tests
{
    [TestFixture]
    public class IgdbMetadataPluginTests
    {
        [Test]
        public void GetImageUrlTest()
        {
            var image = new PlayniteServices.Models.IGDB.GameImage
            {
                url = @"//images.igdb.com/igdb/image/upload/t_thumb/fh5txlnfczqruy55bo6i.jpg"
            };

            Assert.AreEqual(
                "https://images.igdb.com/igdb/image/upload/t_720p/fh5txlnfczqruy55bo6i.jpg",
                IgdbMetadataPlugin.GetImageUrl(image, ImageSizes.p720));
        }

        [Test]
        public void GetGameInfoFromUrlTest()
        {
            Assert.AreEqual("333",
                IgdbMetadataPlugin.GetGameInfoFromUrl(@"https://www.igdb.com/games/quake"));
            Assert.AreEqual("793",
                IgdbMetadataPlugin.GetGameInfoFromUrl(@"https://www.igdb.com/games/mobil-1-rally-championship"));
        }
    }
}
