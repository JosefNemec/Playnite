using NUnit.Framework;
using Playnite.Providers.GOG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Playnite.Models;

namespace Playnite.Providers.GOG.Tests
{
    [TestFixture]
    public class GogLibraryTests
    {
        [Test]
        public void GetInstalledGamesRegistry()
        {
            var gogLib = new GogLibrary();
            var games = gogLib.GetInstalledGames(InstalledGamesSource.Registry);
            Assert.AreNotEqual(0, games.Count);
            CollectionAssert.AllItemsAreUnique(games);

            foreach (Game game in games)
            {
                Assert.IsFalse(string.IsNullOrEmpty(game.Name));
                Assert.IsFalse(string.IsNullOrEmpty(game.ProviderId));
                Assert.IsFalse(string.IsNullOrEmpty(game.InstallDirectory));
                Assert.IsTrue(Directory.Exists(game.InstallDirectory));
                Assert.IsNotNull(game.PlayTask);
                Assert.IsTrue(File.Exists(game.ResolveVariables(game.PlayTask.Path)));
            }
        }

        [Test]
        public void GetInstalledGamesGalaxy()
        {
            var gogLib = new GogLibrary();
            var games = gogLib.GetInstalledGames(InstalledGamesSource.Galaxy);
            Assert.AreNotEqual(0, games.Count);
            CollectionAssert.AllItemsAreUnique(games);

            foreach (Game game in games)
            {
                Assert.IsFalse(string.IsNullOrEmpty(game.Name));
                Assert.IsFalse(string.IsNullOrEmpty(game.ProviderId));
                Assert.IsFalse(string.IsNullOrEmpty(game.InstallDirectory));
                Assert.IsTrue(Directory.Exists(game.InstallDirectory));
                Assert.IsNotNull(game.PlayTask);
                Assert.IsTrue(File.Exists(game.ResolveVariables(game.PlayTask.Path)));
            }
        }

        [Test]
        public void GetInstalledGamesSourceParity()
        {
            var gogLib = new GogLibrary();
            var gamesGalaxy = gogLib.GetInstalledGames(InstalledGamesSource.Galaxy);
            var gamesRegistry = gogLib.GetInstalledGames(InstalledGamesSource.Registry);
            Assert.AreEqual(gamesGalaxy.Count, gamesRegistry.Count);
        }

        [Test]
        public void DownloadGameMetadataTest()
        {
            var gogLib = new GogLibrary();

            // Existing store page - contains all data
            var existingStore = gogLib.DownloadGameMetadata("1207658645");
            Assert.IsNotNull(existingStore.GameDetails);
            Assert.IsNotNull(existingStore.StoreDetails);
            Assert.IsNotNull(existingStore.Icon.Data);
            Assert.IsNotNull(existingStore.Image.Data);
            Assert.IsNotNull(existingStore.BackgroundImage);

            // Game with missing store link in api data
            var customStore = gogLib.DownloadGameMetadata("1207662223", "https://www.gog.com/game/commandos_2_3");
            Assert.IsNotNull(customStore.GameDetails);
            Assert.IsNotNull(customStore.StoreDetails);
            Assert.IsNotNull(customStore.Icon.Data);
            Assert.IsNotNull(customStore.Image.Data);
            Assert.IsNotNull(customStore.BackgroundImage);

            // Existing game not present on store
            var nonStore = gogLib.DownloadGameMetadata("2");
            Assert.IsNotNull(nonStore.GameDetails);
            Assert.IsNull(nonStore.StoreDetails);
            Assert.IsNotNull(nonStore.Icon.Data);
            Assert.IsNotNull(nonStore.Image.Data);
            Assert.IsNotNull(nonStore.BackgroundImage);
        }

    }
}