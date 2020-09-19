using NUnit.Framework;
using Playnite.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests.Plugins
{
    [TestFixture]
    public class ExtensionManifestTests
    {
        [Test]
        public void LegacyIdTest()
        {
            var manifest = new BaseExtensionManifest();
            Assert.AreEqual(string.Empty, manifest.LegacyDirId);

            manifest.Author = "test author";
            manifest.Name = "test plugin";
            Assert.AreEqual("testplugin_7054ba945d54e7918bb613e25a631e54", manifest.LegacyDirId);
        }
    }
}
