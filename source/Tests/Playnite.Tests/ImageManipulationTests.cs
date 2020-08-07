using System.Drawing;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Playnite.Common;
using Playnite.SDK;

namespace Playnite.Tests
{
    [TestFixture]
    public class ImageManipulationTests
    {
        [Test]
        public void ResizeImageTest()
        {
            var baseDir = Assembly.GetExecutingAssembly().Location;
            Assert.NotNull(baseDir);
            baseDir = Path.GetDirectoryName(baseDir);
            var path = Path.Combine(baseDir, "Resources", "Images", "DesignCover.jpg");
            Assert.True(File.Exists(path));

            var output = Path.Combine(baseDir, "image-manipulation-resize-output.png");

            using (var image = Image.FromFile(path))
            {
                Assert.AreEqual(262, image.Width);
                Assert.AreEqual(374, image.Height);

                byte[] result = ImageManipulation.ResizeImage(image, 200, 285);
                File.WriteAllBytes(output, result);
            }

            var outputProperties = Images.GetImageProperties(output);
            Assert.AreEqual(200, outputProperties.Width);
            Assert.AreEqual(285, outputProperties.Height);
        }

        [Test]
        public void DownscaleTest()
        {
            var baseDir = Assembly.GetExecutingAssembly().Location;
            Assert.NotNull(baseDir);
            baseDir = Path.GetDirectoryName(baseDir);
            var path = Path.Combine(baseDir, "Resources", "Images", "DesignCover.jpg");
            Assert.True(File.Exists(path));

            var output = Path.Combine(baseDir, "image-manipulation-downscale-output.png");

            using (var image = Image.FromFile(path))
            {
                Assert.AreEqual(262, image.Width);
                Assert.AreEqual(374, image.Height);

                byte[] result = ImageManipulation.DownscaleImage(image, 0.25);
                File.WriteAllBytes(output, result);
            }

            var outputProperties = Images.GetImageProperties(output);
            Assert.AreEqual(65, outputProperties.Width);
            Assert.AreEqual(93, outputProperties.Height);
        }
    }
}
