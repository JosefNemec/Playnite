using EpicLibrary.Services;
using Moq;
using NUnit.Framework;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicLibrary.Tests
{
    [TestFixture]
    public class WebStoreClientTests
    {
        [Test]
        public async Task QuerySearchTest()
        {
            using (var client = new WebStoreClient())
            {
                var catalogs = await client.QuerySearch("thimb");
                CollectionAssert.IsNotEmpty(catalogs);
                var catalog = catalogs[0];
                Assert.IsNotEmpty(catalog.productSlug);
                Assert.IsNotEmpty(catalog.title);
            }
        }

        [Test]
        public async Task GetProductInfoTest()
        {
            using (var client = new WebStoreClient())
            {
                var catalogs = await client.QuerySearch("thimb");
                var product = await client.GetProductInfo(catalogs[0].productSlug);
                Assert.IsNotEmpty(product.@namespace);
                Assert.IsNotEmpty(product.pages[0].data.hero.backgroundImageUrl);
                Assert.IsNotEmpty(product.pages[0].data.about.description);
                Assert.IsNotEmpty(product.pages[0].data.about.developerAttribution);
            }
        }
    }
}
