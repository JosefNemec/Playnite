//using NUnit.Framework;
//using Playnite.Providers.GOG;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.IO;
//using Playnite.SDK.Models;
//using Playnite;

//namespace PlayniteTests.Providers.GOG
//{
//    [TestFixture]
//    public class GogLibraryTests
//    {
//        [Test]
//        public void GetInstalledGamesRegistry()
//        {
//            var gogLib = new GogLibrary();
//            var games = gogLib.GetInstalledGames();
//            Assert.AreNotEqual(0, games.Count);
//            CollectionAssert.AllItemsAreUnique(games);

//            foreach (Game game in games)
//            {
//                Assert.IsFalse(string.IsNullOrEmpty(game.Name));
//                Assert.IsFalse(string.IsNullOrEmpty(game.GameId));
//                Assert.IsFalse(string.IsNullOrEmpty(game.InstallDirectory));
//                Assert.IsTrue(Directory.Exists(game.InstallDirectory));
//                Assert.IsNotNull(game.PlayTask);
//                Assert.IsTrue(File.Exists(game.ResolveVariables(game.PlayTask.Path)));
//            }
//        }

//        [Test]
//        public void DownloadGameMetadataTest()
//        {
//            var gogLib = new GogLibrary();

//            // Existing store page - contains all data
//            var existingStore = gogLib.DownloadGameMetadata("1207658645");
//            Assert.IsNotNull(existingStore.GameDetails);
//            Assert.IsNotNull(existingStore.StoreDetails);
//            Assert.IsNotNull(existingStore.Icon.Content);
//            Assert.IsNotNull(existingStore.Image.Content);
//            Assert.IsNotNull(existingStore.BackgroundImage);

//            // Game with missing store link in api data
//            var customStore = gogLib.DownloadGameMetadata("1207662223", "https://www.gog.com/game/commandos_2_3");
//            Assert.IsNotNull(customStore.GameDetails);
//            Assert.IsNotNull(customStore.StoreDetails);
//            Assert.IsNotNull(customStore.Icon.Content);
//            Assert.IsNotNull(customStore.Image.Content);
//            Assert.IsNotNull(customStore.BackgroundImage);

//            // Existing game not present on store
//            var nonStore = gogLib.DownloadGameMetadata("2");
//            Assert.IsNotNull(nonStore.GameDetails);
//            Assert.IsNull(nonStore.StoreDetails);
//            Assert.IsNotNull(nonStore.Icon.Content);
//            Assert.IsNotNull(nonStore.Image.Content);
//            Assert.IsNotNull(nonStore.BackgroundImage);
//        }

//    }
//}