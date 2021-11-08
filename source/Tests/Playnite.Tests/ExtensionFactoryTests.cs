using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Playnite.Common;
using Playnite.Plugins;

namespace Playnite.Tests
{
    [TestFixture]
    public class ExtensionFactoryTests
    {
        [Test]
        public void DeduplicateExtListTest()
        {
            var manifests = new List<BaseExtensionManifest>
            {
                new ExtensionManifest { Id = "test1", Version = "1.0" },
                new ExtensionManifest { Id = "test1", Version = "3.0" },
                new ExtensionManifest { Id = "test1", Version = "2.0.2" },
                new ExtensionManifest { Id = "test2", Version = "5.0.1.2" },
                new ExtensionManifest { Id = "test3", Version = "1.0" },
                new ExtensionManifest { Id = "test3", Version = "1.0.1" },
            };

            var list = ExtensionFactory.DeduplicateExtList(manifests).ToList();
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual("3.0", list[0].Version);
            Assert.AreEqual("5.0.1.2", list[1].Version);
            Assert.AreEqual("1.0.1", list[2].Version);
        }
    }
}
