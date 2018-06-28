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

namespace PlayniteTests.Metadata
{
    [TestFixture]
    public class IGDBMetadataProviderTests
    {        
        private IGDBMetadataProvider provider = new IGDBMetadataProvider(new ServicesClient("http://localhost:8080/"));

        [Test]
        public void StandardDownloadTest()
        {         
            var search = provider.SearchMetadata(new Game("Quake 3"));
            CollectionAssert.IsNotEmpty(search);
            var metadata = search.First();
            var data = provider.GetMetadata(metadata.Id);
            Assert.IsNotNull(data.GameData);
            Assert.IsNull(data.Icon);
            Assert.IsNotNull(data.Image);
            Assert.IsNotNull(data.GameData.ReleaseDate);
            Assert.IsTrue(string.IsNullOrEmpty(data.BackgroundImage));
            Assert.IsFalse(string.IsNullOrEmpty(data.GameData.Description));
            CollectionAssert.IsNotEmpty(data.GameData.Publishers);
            CollectionAssert.IsNotEmpty(data.GameData.Developers);
            CollectionAssert.IsNotEmpty(data.GameData.Tags);
            CollectionAssert.IsNotEmpty(data.GameData.Genres);
            CollectionAssert.IsNotEmpty(data.GameData.Links);
            CollectionAssert.IsNotEmpty(data.GameData.Publishers);
        }

        [Test]
        public void SteamIdUseTest()
        {
            var steamGame = new Game("")
            {
                Provider = Provider.Steam,
                ProviderId = "7200"
            };

            var result = provider.GetMetadata(steamGame);
            Assert.IsFalse(result.IsEmpty);
            Assert.AreEqual("TrackMania United", result.GameData.Name);

            steamGame.ProviderId = "999999";
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
            Assert.AreEqual(1996, metadata.GameData.ReleaseDate?.Year);
            Assert.AreEqual("Core Design", metadata.GameData.Developers[0]);

            game.ReleaseDate = new DateTime(2013, 1, 1);
            metadata = provider.GetMetadata(game);
            Assert.IsFalse(metadata.IsEmpty);
            Assert.AreEqual(2013, metadata.GameData.ReleaseDate?.Year);
            Assert.AreEqual("Crystal Dynamics", metadata.GameData.Developers[0]);
        }

        [Test]
        public void AlternateNameUseTest()
        {            
            var metadata = provider.GetMetadata(new Game("pubg"));
            Assert.IsFalse(metadata.IsEmpty);
            Assert.AreEqual("PLAYERUNKNOWN'S BATTLEGROUNDS", metadata.GameData.Name);

            metadata = provider.GetMetadata(new Game("unreal 2"));
            Assert.IsFalse(metadata.IsEmpty);
            Assert.AreEqual("Unreal II: The Awakening", metadata.GameData.Name);
        }

        [Test]
        public void NameMatchingTest()
        {
            // & / and test
            var result = provider.GetMetadata(new Game("Command and Conquer"));
            Assert.IsNotNull(result.GameData);
            Assert.IsNotNull(result.Image);
            Assert.AreEqual("Command & Conquer", result.GameData.Name);
            Assert.AreEqual(1995, result.GameData.ReleaseDate?.Year);

            // Matches exactly
            result = provider.GetMetadata(new Game("Grand Theft Auto IV"));
            Assert.IsNotNull(result.GameData);
            Assert.IsNotNull(result.Image);

            // Roman numerals test
            result = provider.GetMetadata(new Game("Quake 3 Arena"));
            Assert.IsNotNull(result.GameData);
            Assert.IsNotNull(result.Image);
            Assert.AreEqual("Quake III Arena", result.GameData.Name);

            // THE test
            result = provider.GetMetadata(new Game("Witcher 3: Wild Hunt"));
            Assert.IsNotNull(result.GameData);
            Assert.IsNotNull(result.Image);
            Assert.AreEqual("The Witcher 3: Wild Hunt", result.GameData.Name);

            // No subtitle test
            result = provider.GetMetadata(new Game("The Witcher 3"));
            Assert.IsNotNull(result.GameData);
            Assert.IsNotNull(result.Image);
            Assert.AreEqual("The Witcher 3: Wild Hunt", result.GameData.Name);
        }
    }
}
