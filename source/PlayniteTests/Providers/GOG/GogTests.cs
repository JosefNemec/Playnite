using Microsoft.VisualStudio.TestTools.UnitTesting;
using Playnite.Providers.GOG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Playnite.Providers.GOG.Tests
{
    [TestClass()]
    public class GogTests
    {
        [TestMethod()]
        [Description("Basic verification testing that installed games can be fetched from local client.")]
        public void GetInstalledGames_Basic()
        {
            var games = Gog.GetInstalledGames();
            Assert.AreNotEqual(0, games.Count);
            CollectionAssert.AllItemsAreUnique(games);

            foreach (var game in games)
            {
                Assert.IsFalse(string.IsNullOrEmpty(game.Name));
                Assert.IsFalse(string.IsNullOrEmpty(game.ProviderId));
                Assert.IsFalse(string.IsNullOrEmpty(game.InstallDirectory));
                Assert.IsTrue(Directory.Exists(game.InstallDirectory));
                Assert.IsNotNull(game.PlayTask);
                Assert.IsTrue(File.Exists(game.PlayTask.Path));
            }
        }

        [TestMethod()]
        public void DownloadGameMetadataTest()
        {
            // Existing store page - contains all data
            var existingStore = Gog.DownloadGameMetadata("1207658645");
            Assert.IsNotNull(existingStore.GameDetails);
            Assert.IsNotNull(existingStore.StoreDetails);
            Assert.IsNotNull(existingStore.Icon.Data);
            Assert.IsNotNull(existingStore.Image.Data);
            Assert.IsNotNull(existingStore.BackgroundImage);

            // Game with missing store link in api data
            var customStore = Gog.DownloadGameMetadata("1207662223", "https://www.gog.com/game/commandos_2_3");
            Assert.IsNotNull(customStore.GameDetails);
            Assert.IsNotNull(customStore.StoreDetails);
            Assert.IsNotNull(customStore.Icon.Data);
            Assert.IsNotNull(customStore.Image.Data);
            Assert.IsNotNull(customStore.BackgroundImage);

            // Existing game not present on store
            var nonStore = Gog.DownloadGameMetadata("2");
            Assert.IsNotNull(nonStore.GameDetails);
            Assert.IsNull(nonStore.StoreDetails);
            Assert.IsNotNull(nonStore.Icon.Data);
            Assert.IsNotNull(nonStore.Image.Data);
            Assert.IsNotNull(nonStore.BackgroundImage);
        }

    }
}