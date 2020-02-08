using GogLibrary;
using NUnit.Framework;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GogLibrary.Tests
{
    [TestFixture]
    public class GogMetadataProviderTests
    {
        [Test]
        public void StandardDownloadTest()
        {
            var provider = new GogMetadataProvider(GogLibraryTests.CreateLibrary());
            var data = provider.GetMetadata(new Game() { GameId = "1207659012" });
            Assert.IsNotNull(data.GameInfo);
            Assert.IsNotNull(data.Icon);
            Assert.IsNotNull(data.CoverImage);
            Assert.IsNotNull(data.GameInfo.ReleaseDate);
            Assert.IsNotNull(data.BackgroundImage);
            Assert.IsFalse(string.IsNullOrEmpty(data.GameInfo.Description));
            CollectionAssert.IsNotEmpty(data.GameInfo.Publishers);
            CollectionAssert.IsNotEmpty(data.GameInfo.Developers);
            CollectionAssert.IsNotEmpty(data.GameInfo.Features);
            CollectionAssert.IsNotEmpty(data.GameInfo.Genres);
            CollectionAssert.IsNotEmpty(data.GameInfo.Links);
            CollectionAssert.IsNotEmpty(data.GameInfo.Publishers);
        }

        [Test]
        public void DownloadGameMetadataTest()
        {
            var gogLib = new GogMetadataProvider(GogLibraryTests.CreateLibrary());

            // Existing store page - contains all data
            var existingStore = gogLib.DownloadGameMetadata(new Game() { GameId = "1207658645" });
            Assert.IsNotNull(existingStore.GameDetails);
            Assert.IsNotNull(existingStore.StoreDetails);
            Assert.IsNotNull(existingStore.Icon.OriginalUrl);
            Assert.IsNotNull(existingStore.CoverImage.OriginalUrl);
            Assert.IsNotNull(existingStore.BackgroundImage.OriginalUrl);

            // Game with missing store link in api data
            var customStore = gogLib.DownloadGameMetadata(new Game() { GameId = "1207662223" });
            Assert.IsNotNull(customStore.GameDetails);
            Assert.IsNull(customStore.StoreDetails);
            Assert.IsNotNull(existingStore.Icon.OriginalUrl);
            Assert.IsNotNull(existingStore.CoverImage.OriginalUrl);
            Assert.IsNotNull(existingStore.BackgroundImage.OriginalUrl);

            // Existing game not present on store
            var nonStore = gogLib.DownloadGameMetadata(new Game() { GameId = "2" });
            Assert.IsNotNull(nonStore.GameDetails);
            Assert.IsNull(nonStore.StoreDetails);
            Assert.IsNotNull(existingStore.Icon.OriginalUrl);
            Assert.IsNotNull(existingStore.CoverImage.OriginalUrl);
            Assert.IsNotNull(existingStore.BackgroundImage.OriginalUrl);
        }
    }
}
