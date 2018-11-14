using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OriginLibrary.Services;

namespace OriginLibrary.Tests.Services
{
    [TestFixture]
    public class OriginApiClientTests
    {
        [Test]
        public void GetGameLocalDataTest()
        {
            var ids = new string[] { "DR:104624900", "OFB-EAST:52018" };

            foreach (var id in ids)
            {
                var data = OriginApiClient.GetGameLocalData(id);
                Assert.IsNotNull(data.localizableAttributes);
                Assert.IsNotNull(data.publishing);
                Assert.IsFalse(string.IsNullOrEmpty(data.offerId));
                Assert.IsFalse(string.IsNullOrEmpty(data.offerType));
            }
        }

        [Test]
        public void GetGameStoreDataTest()
        {
            var ids = new string[] { "DR:104624900", "OFB-EAST:52018" };

            foreach (var id in ids)
            {
                var data = OriginApiClient.GetGameStoreData(id);
                Assert.IsFalse(string.IsNullOrEmpty(data.offerId));
                Assert.IsFalse(string.IsNullOrEmpty(data.offerType));
                Assert.IsNotNull(data.platforms);
                Assert.IsNotNull(data.i18n);
                Assert.IsFalse(string.IsNullOrEmpty(data.imageServer));
            }
        }
    }
}
