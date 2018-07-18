using Moq;
using NUnit.Framework;
using Playnite.API;
using Playnite.Plugins;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.Plugins
{
    [TestFixture]
    public class PluginFactoryTests
    {
        [Test]
        public void LoadGameLibraryPluginTest()
        {
            var api = new Mock<IPlayniteAPI>();
            var descriptors = PluginFactory.GetPluginDescriptorFiles();
            Assert.AreEqual(1, descriptors.Count);

            var descriptor = PluginDescription.FromFile(descriptors[0]);
            var libraries = PluginFactory.LoadGameLibraryPlugin(descriptor, api.Object);
            Assert.AreEqual(1, libraries.Count);
            Assert.IsNotNull(libraries[0].Id);
        }
    }
}
