using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Playnite.Common;

namespace Playnite.Tests
{
    [TestFixture]
    public class ImagesTests
    {
        [Test]
        public void GetImagePropertiesTest()
        {
            var path = Path.Combine(PlaynitePaths.ProgramPath, "Resources", "Images", "applogo.png");
            var properties = Images.GetImageProperties(path);
            Assert.AreEqual(256, properties.Height);
            Assert.AreEqual(261, properties.Width);
        }
    }
}
