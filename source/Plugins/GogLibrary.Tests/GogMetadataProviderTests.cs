using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.MetaProviders
{
    [TestFixture]
    public class GogMetadataProviderTests
    {
        //[Test]
        //public void StandardDownloadTest()
        //{
        //    var provider = new GogMetadataProvider();
        //    var data = provider.GetMetadata("1207659012");
        //    Assert.IsNotNull(data.GameData);
        //    Assert.IsNotNull(data.Icon);
        //    Assert.IsNotNull(data.Image);
        //    Assert.IsNotNull(data.GameData.ReleaseDate);
        //    Assert.IsFalse(string.IsNullOrEmpty(data.BackgroundImage));
        //    Assert.IsFalse(string.IsNullOrEmpty(data.GameData.Description));
        //    CollectionAssert.IsNotEmpty(data.GameData.Publishers);
        //    CollectionAssert.IsNotEmpty(data.GameData.Developers);
        //    CollectionAssert.IsNotEmpty(data.GameData.Tags);
        //    CollectionAssert.IsNotEmpty(data.GameData.Genres);
        //    CollectionAssert.IsNotEmpty(data.GameData.Links);
        //    CollectionAssert.IsNotEmpty(data.GameData.Publishers);
        //}

        //[Test]
        //public void DownloadGameMetadataTest()
        //{
        //    var gogLib = new GogLibrary();

        //    // Existing store page - contains all data
        //    var existingStore = gogLib.DownloadGameMetadata("1207658645");
        //    Assert.IsNotNull(existingStore.GameDetails);
        //    Assert.IsNotNull(existingStore.StoreDetails);
        //    Assert.IsNotNull(existingStore.Icon.Content);
        //    Assert.IsNotNull(existingStore.Image.Content);
        //    Assert.IsNotNull(existingStore.BackgroundImage);

        //    // Game with missing store link in api data
        //    var customStore = gogLib.DownloadGameMetadata("1207662223", "https://www.gog.com/game/commandos_2_3");
        //    Assert.IsNotNull(customStore.GameDetails);
        //    Assert.IsNotNull(customStore.StoreDetails);
        //    Assert.IsNotNull(customStore.Icon.Content);
        //    Assert.IsNotNull(customStore.Image.Content);
        //    Assert.IsNotNull(customStore.BackgroundImage);

        //    // Existing game not present on store
        //    var nonStore = gogLib.DownloadGameMetadata("2");
        //    Assert.IsNotNull(nonStore.GameDetails);
        //    Assert.IsNull(nonStore.StoreDetails);
        //    Assert.IsNotNull(nonStore.Icon.Content);
        //    Assert.IsNotNull(nonStore.Image.Content);
        //    Assert.IsNotNull(nonStore.BackgroundImage);
        //}
    }
}
