using NUnit.Framework;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests.Models
{
    [TestFixture]
    public class MetadataPropertyTests
    {
        [Test]
        public void EqualityTest()
        {
            Assert.IsTrue(new MetadataNameProperty("test").Equals(new MetadataNameProperty("test")));
            Assert.IsFalse(new MetadataNameProperty("test").Equals(new MetadataNameProperty("test2")));

            Assert.IsTrue(new MetadataSpecProperty("test").Equals(new MetadataSpecProperty("test")));
            Assert.IsFalse(new MetadataSpecProperty("test").Equals(new MetadataSpecProperty("test2")));

            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            Assert.IsTrue(new MetadataIdProperty(guid1).Equals(new MetadataIdProperty(guid1)));
            Assert.IsFalse(new MetadataIdProperty(guid1).Equals(new MetadataIdProperty(guid2)));
        }

        [Test]
        public void CloneTest()
        {
            var orig = new MetadataNameProperty("test");
            var clone = orig.GetClone();
            Assert.IsTrue(orig.Equals(clone));
        }
    }
}
