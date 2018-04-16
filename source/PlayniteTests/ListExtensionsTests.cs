using NUnit.Framework;
using Playnite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests
{
    [TestFixture]
    public class ListExtensionsTests
    {
        [Test]
        public void IsNullOrEmptyTest()
        {
            Assert.IsTrue(ListExtensions.IsNullOrEmpty(null));
            Assert.IsTrue(ListExtensions.IsNullOrEmpty(new List<string>()));
            Assert.IsTrue(ListExtensions.IsNullOrEmpty(new List<string>() { string.Empty }));
            Assert.IsTrue(ListExtensions.IsNullOrEmpty(new List<string>() { "" }));
            Assert.IsTrue(ListExtensions.IsNullOrEmpty(new List<string>() { "", "" }));
            Assert.IsFalse(ListExtensions.IsNullOrEmpty(new List<string>() { "test" }));
            Assert.IsFalse(ListExtensions.IsNullOrEmpty(new List<string>() { "", "test" }));
        }

        [Test]
        public void IntersectsPartiallyWithTest()
        {
            Assert.IsTrue((new List<string> { "test" }).IntersectsPartiallyWith(new List<string> { "test" }));
            Assert.IsTrue((new List<string> { "Test" }).IntersectsPartiallyWith(new List<string> { "test" }));
            Assert.IsTrue((new List<string> { "Test2" }).IntersectsPartiallyWith(new List<string> { "test", "test2" }));
            Assert.IsTrue((new List<string> { "test", "test2" }).IntersectsPartiallyWith(new List<string> { "Test2" }));
            Assert.IsFalse((new List<string> { "test", "test2" }).IntersectsPartiallyWith(new List<string> { "Test3" }));
            Assert.IsFalse((new List<string> { "test", "test2" }).IntersectsPartiallyWith(new List<string> { string.Empty }));
        }
    }
}
