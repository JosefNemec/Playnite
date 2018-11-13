using NUnit.Framework;
using Playnite;
using Playnite.Common.System;
using Playnite.Database;
using Playnite.Settings;
using PlayniteUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PlayniteUITests.Converters
{
    [TestFixture]
    public class CustomImageStringToImageConverterTests
    {
        [Test]
        public void LocalFileTest()
        {
            var converter = new CustomImageStringToImageConverter();
            var image = Path.Combine(PlaynitePaths.ProgramPath, "Resources", "Images", "applogo.png");
            var result = converter.Convert(image, null, null, CultureInfo.CurrentCulture);
            Assert.AreEqual(typeof(BitmapImage), result.GetType());
        }

        //[Test]
        //public void ResourceTest()
        //{
        //    var converter = new CustomImageStringToImageConverter();
        //    var image = converter.Convert("resources:/Resources/Images/icon_dark.png", null, null, CultureInfo.CurrentCulture);
        //    Assert.IsNotNull(image);
        //    Assert.AreNotEqual(DependencyProperty.UnsetValue, image);
        //}

        [Test]
        public void WebTest()
        {
            var converter = new CustomImageStringToImageConverter();
            FileSystem.DeleteDirectory(PlaynitePaths.ImagesCachePath);
            var image = @"http://playnite.link/applogo.png";
            var result = converter.Convert(image, null, null, CultureInfo.CurrentCulture);
            Assert.AreEqual(typeof(BitmapImage), result.GetType());
        }

        [Test]
        public void DatabaseTest()
        {
            var converter = new CustomImageStringToImageConverter();
            var file = Path.Combine(PlaynitePaths.ProgramPath, "Resources", "Images", "applogo.png");
            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                var image = db.AddFile("image.png", File.ReadAllBytes(file), Guid.NewGuid());
                CustomImageStringToImageConverter.SetDatabase(db);
                var result = converter.Convert(image, null, null, CultureInfo.CurrentCulture);
                Assert.AreEqual(typeof(BitmapImage), result.GetType());
            }
        }
    }
}
