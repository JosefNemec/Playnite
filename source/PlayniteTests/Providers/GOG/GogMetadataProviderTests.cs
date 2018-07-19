//using NUnit.Framework;
//using Playnite.Providers.GOG;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace PlayniteTests.MetaProviders
//{
//    [TestFixture]
//    public class GogMetadataProviderTests
//    {
//        [Test]
//        public void StandardDownloadTest()
//        {
//            var provider = new GogMetadataProvider();
//            var data = provider.GetMetadata("1207659012");
//            Assert.IsNotNull(data.GameData);
//            Assert.IsNotNull(data.Icon);
//            Assert.IsNotNull(data.Image);
//            Assert.IsNotNull(data.GameData.ReleaseDate);
//            Assert.IsFalse(string.IsNullOrEmpty(data.BackgroundImage));
//            Assert.IsFalse(string.IsNullOrEmpty(data.GameData.Description));
//            CollectionAssert.IsNotEmpty(data.GameData.Publishers);
//            CollectionAssert.IsNotEmpty(data.GameData.Developers);
//            CollectionAssert.IsNotEmpty(data.GameData.Tags);
//            CollectionAssert.IsNotEmpty(data.GameData.Genres);
//            CollectionAssert.IsNotEmpty(data.GameData.Links);
//            CollectionAssert.IsNotEmpty(data.GameData.Publishers);
//        }
//    }
//}
