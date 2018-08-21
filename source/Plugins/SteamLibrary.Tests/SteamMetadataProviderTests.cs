using NUnit.Framework;
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
            //var provider = new SteamMetadataProvider(
            //var data = provider.GetMetadata("578080");
            //Assert.IsNotNull(data.GameData);
            //Assert.IsNotNull(data.Icon);
            //Assert.IsNotNull(data.Image);
            //Assert.IsNotNull(data.GameData.ReleaseDate);
            //Assert.IsFalse(string.IsNullOrEmpty(data.BackgroundImage));
            //Assert.IsFalse(string.IsNullOrEmpty(data.GameData.Description));
            //CollectionAssert.IsNotEmpty(data.GameData.Publishers);
            //CollectionAssert.IsNotEmpty(data.GameData.Developers);
            //CollectionAssert.IsNotEmpty(data.GameData.Tags);
            //CollectionAssert.IsNotEmpty(data.GameData.Genres);
            //CollectionAssert.IsNotEmpty(data.GameData.Links);
            //CollectionAssert.IsNotEmpty(data.GameData.Publishers);
        }



        //[Test]
        //public void UpdateGameWithMetadataMissingMetadataTest()
        //{
        //    var steamLib = CreateLibrary();
        //    var game = new Game()
        //    {
        //        Provider = Provider.Steam,
        //        GameId = "704580"
        //    };

        //    Assert.DoesNotThrow(() => steamLib.UpdateGameWithMetadata(game, new SteamSettings()));

        //    game = new Game()
        //    {
        //        Provider = Provider.Steam,
        //        GameId = "347350"
        //    };

        //    Assert.DoesNotThrow(() => steamLib.UpdateGameWithMetadata(game, new SteamSettings()));
        //}



        //[Test]
        //public void DownloadGameMetadataTest()
        //{
        //    var steamLib = CreateLibrary();

        //    // Existing store
        //    var existing = steamLib.DownloadGameMetadata(107410, false);
        //    Assert.IsNotNull(existing.ProductDetails);
        //    Assert.IsNotNull(existing.StoreDetails);
        //    Assert.IsNotNull(existing.Icon.Content);
        //    Assert.IsNotNull(existing.Image.Content);
        //    Assert.IsNotNull(existing.BackgroundImage);

        //    // NonExisting store
        //    var nonExisting = steamLib.DownloadGameMetadata(201280, true);
        //    Assert.IsNotNull(nonExisting.ProductDetails);
        //    Assert.IsNull(nonExisting.StoreDetails);
        //    Assert.IsNotNull(nonExisting.Icon.Content);
        //    Assert.IsNotNull(nonExisting.Image.Content);
        //    Assert.IsNull(nonExisting.BackgroundImage);
        //}
    }
}
