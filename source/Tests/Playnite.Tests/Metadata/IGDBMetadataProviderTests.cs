using NUnit.Framework;
using Playnite.SDK.Models;
using Playnite.Metadata.Providers;
using Playnite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Metadata;

namespace Playnite.Tests.Metadata
{
    [TestFixture]
    public class IGDBMetadataProviderTests
    {
        private IGDBMetadataProvider provider = new IGDBMetadataProvider(new ServicesClient("http://localhost:5000/"));

        [Test]
        public void StandardDownloadTest()
        {
            var search = provider.SearchMetadata(new Game("Quake 3"));
            CollectionAssert.IsNotEmpty(search);
            var metadata = search.First();
            var data = provider.GetMetadata(metadata.Id);
            Assert.IsNotNull(data.GameInfo);
            Assert.IsNull(data.Icon);
            Assert.IsNotNull(data.CoverImage);
            Assert.IsNotNull(data.GameInfo.ReleaseDate);
            Assert.IsFalse(string.IsNullOrEmpty(data.GameInfo.Description));
            CollectionAssert.IsNotEmpty(data.GameInfo.Publishers);
            CollectionAssert.IsNotEmpty(data.GameInfo.Developers);
            CollectionAssert.IsNotEmpty(data.GameInfo.Tags);
            CollectionAssert.IsNotEmpty(data.GameInfo.Genres);
            CollectionAssert.IsNotEmpty(data.GameInfo.Links);
        }

        [Test]
        public void SteamIdUseTest()
        {
            var steamGame = new Game("")
            {
                PluginId = Guid.Parse("CB91DFC9-B977-43BF-8E70-55F46E410FAB"),
                GameId = "7200"
            };

            var result = provider.GetMetadata(steamGame);
            Assert.IsFalse(result.IsEmpty);
            Assert.AreEqual("TrackMania United", result.GameInfo.Name);

            steamGame.GameId = "999999";
            result = provider.GetMetadata(steamGame);
            Assert.IsTrue(result.IsEmpty);
        }

        [Test]
        public void ReleaseDateUseTest()
        {
            var game = new Game("Tomb Raider")
            {
                ReleaseDate = new DateTime(1996, 1, 1)
            };

            var metadata = provider.GetMetadata(game);
            Assert.IsFalse(metadata.IsEmpty);
            Assert.AreEqual(1996, metadata.GameInfo.ReleaseDate?.Year);
            Assert.AreEqual("Core Design", metadata.GameInfo.Developers[0]);

            game.ReleaseDate = new DateTime(2013, 1, 1);
            metadata = provider.GetMetadata(game);
            Assert.IsFalse(metadata.IsEmpty);
            Assert.AreEqual(2013, metadata.GameInfo.ReleaseDate?.Year);
            Assert.AreEqual("Crystal Dynamics", metadata.GameInfo.Developers[0]);
        }

        [Test]
        public void AlternateNameUseTest()
        {
            var metadata = provider.GetMetadata(new Game("pubg"));
            Assert.IsFalse(metadata.IsEmpty);
            Assert.AreEqual("PLAYERUNKNOWN'S BATTLEGROUNDS", metadata.GameInfo.Name);

            metadata = provider.GetMetadata(new Game("unreal 2"));
            Assert.IsFalse(metadata.IsEmpty);
            Assert.AreEqual("Unreal II: The Awakening", metadata.GameInfo.Name);
        }

        [Test]
        public void NameMatchingTest()
        {
            // & / and test
            var result = provider.GetMetadata(new Game("Command and Conquer"));
            Assert.IsNotNull(result.GameInfo);
            Assert.IsNotNull(result.CoverImage);
            Assert.AreEqual("Command & Conquer", result.GameInfo.Name);
            Assert.AreEqual(1995, result.GameInfo.ReleaseDate?.Year);

            // Matches exactly
            result = provider.GetMetadata(new Game("Grand Theft Auto IV"));
            Assert.IsNotNull(result.GameInfo);
            Assert.IsNotNull(result.CoverImage);

            // Roman numerals test
            result = provider.GetMetadata(new Game("Quake 3 Arena"));
            Assert.IsNotNull(result.GameInfo);
            Assert.IsNotNull(result.CoverImage);
            Assert.AreEqual("Quake III Arena", result.GameInfo.Name);

            // THE test
            result = provider.GetMetadata(new Game("Witcher 3: Wild Hunt"));
            Assert.IsNotNull(result.GameInfo);
            Assert.IsNotNull(result.CoverImage);
            Assert.AreEqual("The Witcher 3: Wild Hunt", result.GameInfo.Name);

            // No subtitle test
            result = provider.GetMetadata(new Game("The Witcher 3"));
            Assert.IsNotNull(result.GameInfo);
            Assert.IsNotNull(result.CoverImage);
            Assert.AreEqual("The Witcher 3: Wild Hunt", result.GameInfo.Name);

            // Apostrophe test
            result = provider.GetMetadata(new Game("Dragons Lair"));
            Assert.IsNotNull(result.GameInfo);
            Assert.IsNotNull(result.CoverImage);
            Assert.AreEqual("Dragon's Lair", result.GameInfo.Name);
        }
    }
}
