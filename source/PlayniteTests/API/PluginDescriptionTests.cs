using NUnit.Framework;
using Playnite.API;
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
            var path =  Path.Combine(PlayniteTests.ResourcesPath, "plugin.info");
            var description = PluginDescription.FromFile(path);
            Assert.IsNotEmpty(description.Assembly);
            Assert.IsNotEmpty(description.Author);
            Assert.IsNotEmpty(description.Name);
            Assert.IsNotEmpty(description.Path);
            Assert.IsNotNull(description.Version);
            Assert.AreEqual(PluginType.GameLibrary, description.Type);
        }
    }
}
