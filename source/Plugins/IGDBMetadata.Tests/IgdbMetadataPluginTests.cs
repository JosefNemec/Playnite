using IGDBMetadata.Models;
using IGDBMetadata.Services;
using NUnit.Framework;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.Tests;
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
        private IgdbMetadataPlugin GetTestPlugin()
        {
            return new IgdbMetadataPlugin(
                PlayniteTests.GetTestingApi().Object,
                new IgdbServiceClient(Version.Parse("1.0.0.0"), "http://localhost:5000/"));
        }

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

        [Test]
        public void StandardDownloadTest()
        {
            var plugin = GetTestPlugin();
            using (var provider = plugin.GetMetadataProvider(new MetadataRequestOptions(new Game("Subnautica"), true)))
            {
                var fields = provider.AvailableFields;
                CollectionAssert.Contains(fields, MetadataField.Name);
                CollectionAssert.Contains(fields, MetadataField.Genres);
                CollectionAssert.Contains(fields, MetadataField.ReleaseDate);
                CollectionAssert.Contains(fields, MetadataField.Developers);
                CollectionAssert.Contains(fields, MetadataField.Publishers);
                CollectionAssert.Contains(fields, MetadataField.Features);
                CollectionAssert.Contains(fields, MetadataField.Description);
                CollectionAssert.Contains(fields, MetadataField.Links);
                CollectionAssert.Contains(fields, MetadataField.CriticScore);
                CollectionAssert.Contains(fields, MetadataField.CommunityScore);
                CollectionAssert.Contains(fields, MetadataField.CoverImage);
                CollectionAssert.Contains(fields, MetadataField.BackgroundImage);

                Assert.IsFalse(provider.GetName().IsNullOrEmpty());
                CollectionAssert.IsNotEmpty(provider.GetGenres());
                Assert.IsNotNull(provider.GetReleaseDate());
                CollectionAssert.IsNotEmpty(provider.GetDevelopers());
                CollectionAssert.IsNotEmpty(provider.GetPublishers());
                CollectionAssert.IsNotEmpty(provider.GetFeatures());
                CollectionAssert.IsNotEmpty(provider.GetLinks());
                Assert.IsFalse(provider.GetDescription().IsNullOrEmpty());
                Assert.IsNotNull(provider.GetCriticScore());
                Assert.IsNotNull(provider.GetCommunityScore());
                Assert.IsNotNull(provider.GetCoverImage());
                Assert.IsNotNull(provider.GetBackgroundImage());
            }
        }
    }
}
