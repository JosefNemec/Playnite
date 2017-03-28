using Microsoft.VisualStudio.TestTools.UnitTesting;
using Playnite.Providers.GOG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Playnite.Providers.Steam.Tests
{
    [TestClass()]
    public class SteamTests
    {
        [TestMethod()]
        [Description("Basic verification testing that installed games can be fetched from local client.")]
        public void GetInstalledGames_Basic()
        {
            var games = Steam.GetInstalledGames();
            Assert.AreNotEqual(0, games.Count);
            CollectionAssert.AllItemsAreUnique(games);

            foreach (var game in games)
            {
                Assert.IsFalse(string.IsNullOrEmpty(game.Name));
                Assert.IsFalse(string.IsNullOrEmpty(game.ProviderId));
                Assert.IsFalse(string.IsNullOrEmpty(game.InstallDirectory));
                Assert.IsTrue(Directory.Exists(game.InstallDirectory));
                Assert.IsNotNull(game.PlayTask);
                Assert.IsTrue(game.PlayTask.Type == Models.GameTaskType.URL);
            }
        }

        [TestMethod()]
        public void DownloadGameMetadataTest()
        {
            // Existing store
            var existing = Steam.DownloadGameMetadata(107410);
            Assert.IsNotNull(existing.ProductDetails);
            Assert.IsNotNull(existing.StoreDetails);
            Assert.IsNotNull(existing.Icon.Data);
            Assert.IsNotNull(existing.Image.Data);
            Assert.IsNotNull(existing.BackgroundImage);

            // NonExisting store
            var nonExisting = Steam.DownloadGameMetadata(201280);
            Assert.IsNotNull(nonExisting.ProductDetails);
            Assert.IsNull(nonExisting.StoreDetails);
            Assert.IsNotNull(nonExisting.Icon.Data);
            Assert.IsNotNull(nonExisting.Image.Data);
            Assert.IsNotNull(nonExisting.BackgroundImage);
        }
    }
}