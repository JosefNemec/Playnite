using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite;
using Playnite.SDK.Models;
using Newtonsoft.Json;

namespace Playnite.Tests
{
    [TestFixture]
    public class AddonManifestTests
    {
        [Test]
        public void GetLatestCompatiblePackageTest()
        {
            var manifest = new AddonInstallerManifest
            {
                Packages = new List<AddonInstallerPackage>
                {
                    new AddonInstallerPackage
                    { Version = new Version(1, 0), RequiredApiVersion = new Version (1, 0) },
                    new AddonInstallerPackage
                    { Version = new Version(1, 1), RequiredApiVersion = new Version (1, 0) },
                    new AddonInstallerPackage
                    { Version = new Version(1, 2), RequiredApiVersion = new Version (1, 1) },
                    new AddonInstallerPackage
                    { Version = new Version(1, 3), RequiredApiVersion = new Version (2, 0) },
                }
            };

            Assert.AreEqual(null, manifest.GetLatestCompatiblePackage(new Version(3, 0)));
            Assert.AreEqual(new Version(1, 1), manifest.GetLatestCompatiblePackage(new Version(1, 0)).Version);
            Assert.AreEqual(new Version(1, 2), manifest.GetLatestCompatiblePackage(new Version(1, 1)).Version);
            Assert.AreEqual(new Version(1, 3), manifest.GetLatestCompatiblePackage(new Version(2, 0)).Version);
        }
    }
}
