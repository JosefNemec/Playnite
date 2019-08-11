using NUnit.Framework;
using Playnite;
using Playnite.Common;
using Playnite.Database;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Playnite.Tests
{
    [TestFixture]
    public class CustomImageStringToImageConverterTests
    {
        [Test]
        public void LocalFileTest()
        {
            var image = Path.Combine(PlaynitePaths.ProgramPath, "Resources", "Images", "applogo.png");
            var result = ImageSourceManager.GetImage(image, false);
            Assert.AreEqual(typeof(BitmapImage), result.GetType());
        }

        [Test]
        public void ResourceTest()
        {
            var image = ImageSourceManager.GetImage("resources:/Resources/Images/icon_dark.png", false);
            Assert.IsNotNull(image);
            Assert.AreNotEqual(DependencyProperty.UnsetValue, image);
        }

        [Test]
        public void WebTest()
        {
            FileSystem.DeleteDirectory(PlaynitePaths.ImagesCachePath);
            var image = @"http://playnite.link/applogo.png";
            var result = ImageSourceManager.GetImage(image, false);
            Assert.AreEqual(typeof(BitmapImage), result.GetType());
        }

        [Test]
        public void DatabaseTest()
        {
            var file = Path.Combine(PlaynitePaths.ProgramPath, "Resources", "Images", "applogo.png");
            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                var image = db.AddFile("image.png", File.ReadAllBytes(file), Guid.NewGuid());
                ImageSourceManager.SetDatabase(db);
                var result = ImageSourceManager.GetImage(image, false);
                Assert.AreEqual(typeof(BitmapImage), result.GetType());
            }
        }
    }
}
