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

        [Test]
        public void SteamIdUseTest()
        {
            var steamGame = new Game("")
            {
                PluginId = Guid.Parse("CB91DFC9-B977-43BF-8E70-55F46E410FAB"),
                GameId = "7200"
            };

            var plugin = GetTestPlugin();
            using (var provider = plugin.GetMetadataProvider(new MetadataRequestOptions(steamGame, true)))
            {
                Assert.AreEqual("TrackMania United", provider.GetName());
            }
        }

        [Test]
        public void ReleaseDateUseTest()
        {
            var game = new Game("Tomb Raider")
            {
                ReleaseDate = new DateTime(1996, 1, 1)
            };

            var plugin = GetTestPlugin();
            using (var provider = plugin.GetMetadataProvider(new MetadataRequestOptions(game, true)))
            {
                Assert.AreEqual(1996, provider.GetReleaseDate().Value.Year);
                Assert.AreEqual("Core Design", provider.GetDevelopers()[0]);
            }

            game.ReleaseDate = new DateTime(2013, 1, 1);
            using (var provider = plugin.GetMetadataProvider(new MetadataRequestOptions(game, true)))
            {
                Assert.AreEqual(2013, provider.GetReleaseDate().Value.Year);
                Assert.AreEqual("Crystal Dynamics", provider.GetDevelopers()[0]);
            }
        }

        [Test]
        public void AlternateNameUseTest()
        {
            var plugin = GetTestPlugin();
            using (var provider = plugin.GetMetadataProvider(new MetadataRequestOptions(new Game("pubg"), true)))
            {
                Assert.AreEqual("PLAYERUNKNOWN'S BATTLEGROUNDS", provider.GetName());
            }

            using (var provider = plugin.GetMetadataProvider(new MetadataRequestOptions(new Game("unreal 2"), true)))
            {
                Assert.AreEqual("Unreal II: The Awakening", provider.GetName());
            }
        }

        [Test]
        public void NameMatchingTest()
        {
            var plugin = GetTestPlugin();

            // & / and test
            using (var provider = plugin.GetMetadataProvider(new MetadataRequestOptions(new Game("Command and Conquer"), true)))
            {
                Assert.IsNotNull(provider.GetCoverImage());
                Assert.AreEqual("Command & Conquer", provider.GetName());
                Assert.AreEqual(1995, provider.GetReleaseDate().Value.Year);
            }

            // Matches exactly
            using (var provider = plugin.GetMetadataProvider(new MetadataRequestOptions(new Game("Grand Theft Auto IV"), true)))
            {
                CollectionAssert.IsNotEmpty(provider.AvailableFields);
            }

            // Roman numerals test
            using (var provider = plugin.GetMetadataProvider(new MetadataRequestOptions(new Game("Quake 3 Arena"), true)))
            {
                Assert.IsNotNull(provider.GetCoverImage());
                Assert.AreEqual("Quake III Arena", provider.GetName());
            }

            // THE test
            using (var provider = plugin.GetMetadataProvider(new MetadataRequestOptions(new Game("Witcher 3: Wild Hunt"), true)))
            {
                Assert.AreEqual("The Witcher 3: Wild Hunt", provider.GetName());
            }

            // No subtitle test
            using (var provider = plugin.GetMetadataProvider(new MetadataRequestOptions(new Game("The Witcher 3"), true)))
            {
                Assert.AreEqual("The Witcher 3: Wild Hunt", provider.GetName());
            }

            // Apostrophe test
            using (var provider = plugin.GetMetadataProvider(new MetadataRequestOptions(new Game("Dragons Lair"), true)))
            {
                Assert.AreEqual("Dragon's Lair", provider.GetName());
            }

            // Hyphen vs. colon test
            using (var provider = plugin.GetMetadataProvider(new MetadataRequestOptions(new Game("Legacy of Kain - Soul Reaver 2"), true)))
            {
                Assert.AreEqual("Legacy of Kain: Soul Reaver 2", provider.GetName());
            }

            using (var provider = plugin.GetMetadataProvider(new MetadataRequestOptions(new Game("Legacy of Kain: Soul Reaver 2"), true)))
            {
                Assert.AreEqual("Legacy of Kain: Soul Reaver 2", provider.GetName());
            }

            // Trademarks test
            using (var provider = plugin.GetMetadataProvider(new MetadataRequestOptions(new Game("Dishonored®: Death of the Outsider™"), true)))
            {
                Assert.AreEqual("Dishonored: Death of the Outsider", provider.GetName());
            }
        }
    }
}
