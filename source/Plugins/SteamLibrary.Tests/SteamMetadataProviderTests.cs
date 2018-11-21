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
            Assert.IsNotNull(data.GameInfo);
            Assert.IsNotNull(data.Icon);
            Assert.IsNotNull(data.CoverImage);
            Assert.IsNotNull(data.GameInfo.ReleaseDate);
            Assert.IsNotNull(data.BackgroundImage);
            Assert.IsFalse(string.IsNullOrEmpty(data.GameInfo.Description));
            CollectionAssert.IsNotEmpty(data.GameInfo.Publishers);
            CollectionAssert.IsNotEmpty(data.GameInfo.Developers);
            CollectionAssert.IsNotEmpty(data.GameInfo.Tags);
            CollectionAssert.IsNotEmpty(data.GameInfo.Genres);
            CollectionAssert.IsNotEmpty(data.GameInfo.Links);
            CollectionAssert.IsNotEmpty(data.GameInfo.Publishers);
        }
    }
}
