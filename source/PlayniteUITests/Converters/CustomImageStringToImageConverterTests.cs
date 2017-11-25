using NUnit.Framework;
using Playnite;
using Playnite.Database;
using PlayniteUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var image = Path.Combine(Paths.ProgramFolder, "Resources", "Images", "applogo.png");
            var result = converter.Convert(image, null, null, CultureInfo.CurrentCulture);
            Assert.AreEqual(typeof(BitmapImage), result.GetType());
        }

        [Test]
        public void ResourceTest()
        {
            var converter = new CustomImageStringToImageConverter();
            var image = "resources:test/test.img";
            Assert.AreEqual("test/test.img", converter.Convert(image, null, null, CultureInfo.CurrentCulture));
        }

        [Test]
        public void WebTest()
        {
            var converter = new CustomImageStringToImageConverter();
            FileSystem.DeleteFolder(Paths.ImagesCachePath);
            var image = @"http://playnite.link/applogo.png";
            var result = converter.Convert(image, null, null, CultureInfo.CurrentCulture);
            Assert.AreEqual(typeof(BitmapImage), result.GetType());
        }

        [Test]
        public void DatabaseTest()
        {
            var converter = new CustomImageStringToImageConverter();
            var file = Path.Combine(Paths.ProgramFolder, "Resources", "Images", "applogo.png");
            var path = Path.Combine(PlayniteUITests.TempPath, "imageconverttest.db");
            FileSystem.DeleteFile(path);
            var db = new GameDatabase(null);
            using (db.OpenDatabase(path))
            {
                var image = "image.png";
                db.AddFile(image, image, File.ReadAllBytes(file));
                CustomImageStringToImageConverter.Database = db;
                var result = converter.Convert(image, null, null, CultureInfo.CurrentCulture);
                Assert.AreEqual(typeof(BitmapImage), result.GetType());
            }
        }
    }
}
