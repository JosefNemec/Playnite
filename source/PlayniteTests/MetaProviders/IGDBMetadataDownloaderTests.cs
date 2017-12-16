using NUnit.Framework;
using Playnite.MetaProviders;
using Playnite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.MetaProviders
{
    [TestFixture]
    public class IGDBMetadataDownloaderTests
    {
        [Test]
        public void StandardDownloadTest()
        {
            // TODO: make to a proper unittest
            var client = new ServicesClient("http://localhost:8080/");
            var provider = new IGDBMetadataProvider(client);
            Assert.IsFalse(provider.GetSupportsIdSearch());
            var search = provider.SearchGames("Quake 3");
            CollectionAssert.IsNotEmpty(search);
            var game = search[0];
            var data = provider.GetGameData(game.Id);
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
    }
}
