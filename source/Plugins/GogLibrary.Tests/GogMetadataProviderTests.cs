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
            var provider = new GogMetadataProvider();
            var data = provider.GetMetadata(new Game() { GameId = "1207659012" });
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

        [Test]
        public void DownloadGameMetadataTest()
        {
            var gogLib = new GogMetadataProvider();

            // Existing store page - contains all data
            var existingStore = gogLib.DownloadGameMetadata("1207658645");
            Assert.IsNotNull(existingStore.GameDetails);
            Assert.IsNotNull(existingStore.StoreDetails);
            Assert.IsNotNull(existingStore.Icon.Content);
            Assert.IsNotNull(existingStore.CoverImage.Content);
            Assert.IsNotNull(existingStore.BackgroundImage);

            // Game with missing store link in api data
            var customStore = gogLib.DownloadGameMetadata("1207662223");
            Assert.IsNotNull(customStore.GameDetails);
            Assert.IsNotNull(customStore.StoreDetails);
            Assert.IsNotNull(customStore.Icon.Content);
            Assert.IsNotNull(customStore.CoverImage.Content);
            Assert.IsNotNull(customStore.BackgroundImage);

            // Existing game not present on store
            var nonStore = gogLib.DownloadGameMetadata("2");
            Assert.IsNotNull(nonStore.GameDetails);
            Assert.IsNull(nonStore.StoreDetails);
            Assert.IsNotNull(nonStore.Icon.Content);
            Assert.IsNotNull(nonStore.CoverImage.Content);
            Assert.IsNotNull(nonStore.BackgroundImage);
        }
    }
}
