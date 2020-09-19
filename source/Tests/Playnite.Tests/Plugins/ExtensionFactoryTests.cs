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
    public class ExtensionFactoryTests
    {
        [Test]
        public void LoadPluginsTest()
        {
            var api = new Mock<IPlayniteAPI>();
            var factory = new ExtensionFactory(new GameDatabase(), new GameControllerFactory());

            var descriptors = factory.GetExtensionDescriptors();
            Assert.AreEqual(2, descriptors.Count);

            factory.LoadPlugins(api.Object, null, false);
            Assert.AreEqual(2, factory.Plugins.Count);
            Assert.AreEqual(1, factory.ExportedFunctions.Count);
        }
    }
}
