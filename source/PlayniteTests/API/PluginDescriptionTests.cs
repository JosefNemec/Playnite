using NUnit.Framework;
using Playnite.API;
using Playnite.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.API
{
    [TestFixture]
    public class PluginDescriptionTests
    {
        [Test]
        public void FromFileTest()
        {            
            var path =  Path.Combine(PlayniteTests.ResourcesPath, ExtensionFactory.ExtensionManifestFileName);
            var description = ExtensionDescription.FromFile(path);
            Assert.IsNotEmpty(description.Module);
            Assert.IsNotEmpty(description.Author);
            Assert.IsNotEmpty(description.Name);
            Assert.IsNotEmpty(description.DescriptionPath);
            Assert.IsNotNull(description.Version);
            Assert.AreEqual(ExtensionType.GameLibrary, description.Type);
        }
    }
}
