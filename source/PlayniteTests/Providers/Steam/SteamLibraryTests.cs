using NUnit.Framework;
using Playnite.Providers.GOG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Playnite.Providers.Steam;
using Playnite.Models;

namespace PlayniteTests.Providers.Steam
{
    [TestFixture]
    public class SteamLibraryTests
    {
        [OneTimeSetUp]
        public void Init()
        {
            // Some test are reading resources, which cannot be access until pack:// namespace is initialized
            // http://stackoverflow.com/questions/6005398/uriformatexception-invalid-uri-invalid-port-specified
            string s = System.IO.Packaging.PackUriHelper.UriSchemePack;
        }

        [Test]
        [Description("Basic verification testing that installed games can be fetched from local client.")]
        public void GetInstalledGamesTest()
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

        [Test]
        public void GetCategorizedGamesTest()
        {
            var steamLib = new SteamLibrary();
            var user = steamLib.GetSteamUsers().First(a => a.Recent);
            var cats = steamLib.GetCategorizedGames(user.Id);
            var game = cats.First();
            CollectionAssert.IsNotEmpty(cats);
            CollectionAssert.IsNotEmpty(game.Categories);
            Assert.IsFalse(string.IsNullOrEmpty(game.ProviderId));
            Assert.AreEqual(game.Provider, Playnite.Models.Provider.Steam);
        }

        [Test]
        public void GetSteamUsersTest()
        {
            var steamLib = new SteamLibrary();
            var users = steamLib.GetSteamUsers();
            CollectionAssert.IsNotEmpty(users);
            var user = users.First();
            Assert.IsFalse(string.IsNullOrEmpty(user.AccountName));
            Assert.IsFalse(string.IsNullOrEmpty(user.PersonaName));
        }

        [Test]
        public void UpdateGameWithMetadataMissingMetadataTest()
        {
            var steamLib = new SteamLibrary();
            var game = new Game()
            {
                Provider = Provider.Steam,
                ProviderId = "704580"
            };

            Assert.DoesNotThrow(() => steamLib.UpdateGameWithMetadata(game));

            game = new Game()
            {
                Provider = Provider.Steam,
                ProviderId = "347350"
            };

            Assert.DoesNotThrow(() => steamLib.UpdateGameWithMetadata(game));            
        }


        [Test]
        public void GetAppStateTest()
        {
            var steamLib = new SteamLibrary();
            var state = steamLib.GetAppState(12140);
        }
    }
}