using Moq;
using NUnit.Framework;
using Playnite.SDK;
using Playnite.SDK.Models;
using SteamLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamLibrary.Tests
{
    [TestFixture]
    public class SteamMetadataProviderTests
    {
        [Test]
        public void StandardDownloadTest()
        {
            var api = new Mock<IPlayniteAPI>();
            var provider = new SteamMetadataProvider(null, new SteamLibrary(api.Object, null), new SteamApiClient());
            var data = provider.GetMetadata(new Game() { GameId = "578080" });
            Assert.IsNotNull(data.GameData);
            Assert.IsNotNull(data.Icon);
            Assert.IsNotNull(data.Image);
            Assert.IsNotNull(data.GameData.ReleaseDate);
            Assert.IsFalse(string.IsNullOrEmpty(data.BackgroundImage));
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
