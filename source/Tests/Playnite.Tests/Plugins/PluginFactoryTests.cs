using Moq;
using NUnit.Framework;
using Playnite.API;
using Playnite.Controllers;
using Playnite.Database;
using Playnite.Plugins;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests.Plugins
{
    [TestFixture]
    public class PluginFactoryTests
    {
        [Test]
        public void LoadGameLibraryPluginTest()
        {
            var api = new Mock<IPlayniteAPI>();
            var factory = new ExtensionFactory(new GameDatabase(), new GameControllerFactory());

            var descriptors = factory.GetExtensionDescriptors();
            Assert.AreEqual(2, descriptors.Count);

            var descriptor = descriptors[0];
            factory.LoadLibraryPlugins(api.Object, null);
            Assert.AreEqual(1, factory.LibraryPlugins.Count);
            Assert.AreEqual(0, factory.GenericPlugins.Count);

            factory.LoadLibraryPlugins(api.Object, new List<string>() { "TestGameLibrary" });
            Assert.AreEqual(0, factory.LibraryPlugins.Count);
        }

        [Test]
        public void LoadGenericPluginTest()
        {
            var api = new Mock<IPlayniteAPI>();
            var factory = new ExtensionFactory(new GameDatabase(), new GameControllerFactory());

            var descriptors = factory.GetExtensionDescriptors();
            Assert.AreEqual(2, descriptors.Count);

            var descriptor = descriptors[0];
            factory.LoadGenericPlugins(api.Object, null);
            Assert.AreEqual(1, factory.GenericPlugins.Count);
            Assert.AreEqual(0, factory.LibraryPlugins.Count);

            factory.LoadGenericPlugins(api.Object, new List<string>() { "TestPlugin" });
            Assert.AreEqual(0, factory.GenericPlugins.Count);
        }
    }
}
