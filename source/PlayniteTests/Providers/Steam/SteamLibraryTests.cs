using NUnit.Framework;
using Playnite.Providers.GOG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Playnite.Providers.Steam;

namespace PlayniteTests.Providers.Steam
{
    [TestFixture]
    public class SteamLibraryTests
    {
        [Test]
        [Description("Basic verification testing that installed games can be fetched from local client.")]
        public void GetInstalledGames_Basic()
        {
            var steamLib = new SteamLibrary();
            var games = steamLib.GetInstalledGames();
            Assert.AreNotEqual(0, games.Count);
            CollectionAssert.AllItemsAreUnique(games);

            foreach (var game in games)
            {
                Assert.IsFalse(string.IsNullOrEmpty(game.Name));
                Assert.IsFalse(string.IsNullOrEmpty(game.ProviderId));
                Assert.IsFalse(string.IsNullOrEmpty(game.InstallDirectory));
                Assert.IsTrue(Directory.Exists(game.InstallDirectory));
                Assert.IsNotNull(game.PlayTask);
                Assert.IsTrue(game.PlayTask.Type == Playnite.Models.GameTaskType.URL);
            }
        }

        [Test]
        public void DownloadGameMetadataTest()
        {
            var steamLib = new SteamLibrary();

            // Existing store
            var existing = steamLib.DownloadGameMetadata(107410);
            Assert.IsNotNull(existing.ProductDetails);
            Assert.IsNotNull(existing.StoreDetails);
            Assert.IsNotNull(existing.Icon.Data);
            Assert.IsNotNull(existing.Image.Data);
            Assert.IsNotNull(existing.BackgroundImage);

            // NonExisting store
            var nonExisting = steamLib.DownloadGameMetadata(201280);
            Assert.IsNotNull(nonExisting.ProductDetails);
            Assert.IsNull(nonExisting.StoreDetails);
            Assert.IsNotNull(nonExisting.Icon.Data);
            Assert.IsNotNull(nonExisting.Image.Data);
            Assert.IsNotNull(nonExisting.BackgroundImage);
        }
    }
}