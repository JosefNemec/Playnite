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
            Assert.IsFalse(((IEnumerable<object>)null).HasItems());
            Assert.IsFalse(new List<string>().HasItems());
            Assert.IsFalse(new List<string>() { string.Empty }.HasItems());
            Assert.IsFalse(new List<string>() { "" }.HasItems());
            Assert.IsFalse(new List<string>() { "", "" }.HasItems());
            Assert.IsTrue(new List<string>() { "test" }.HasItems());
            Assert.IsTrue(new List<string>() { "", "test" }.HasItems());
        }

        [Test]
        public void IntersectsPartiallyWithTest()
        {
            Assert.IsTrue((new List<string> { "test" }).IntersectsPartiallyWith(new List<string> { "test" }));
            Assert.IsTrue((new List<string> { "Test" }).IntersectsPartiallyWith(new List<string> { "test" }));
            Assert.IsTrue((new List<string> { "Test" }).IntersectsPartiallyWith(new List<string> { "test2" }));
            Assert.IsTrue((new List<string> { "Test2" }).IntersectsPartiallyWith(new List<string> { "test", "test2" }));
            Assert.IsTrue((new List<string> { "test", "test2" }).IntersectsPartiallyWith(new List<string> { "Test2" }));
            Assert.IsFalse((new List<string> { "test", "test2" }).IntersectsPartiallyWith(new List<string> { "Test3" }));
            Assert.IsFalse((new List<string> { "test", "test2" }).IntersectsPartiallyWith(new List<string> { string.Empty }));
        }

        [Test]
        public void IntersectsExactlyWithTest()
        {
            Assert.IsTrue((new List<string> { "test" }).IntersectsExactlyWith(new List<string> { "test" }));
            Assert.IsTrue((new List<string> { "Test" }).IntersectsExactlyWith(new List<string> { "test" }));
            Assert.IsTrue((new List<string> { "Test2" }).IntersectsExactlyWith(new List<string> { "test", "test2" }));
            Assert.IsTrue((new List<string> { "Test3", "test2" }).IntersectsExactlyWith(new List<string> { "test", "test2" }));
            Assert.IsTrue((new List<string> { "test", "test2" }).IntersectsExactlyWith(new List<string> { "Test2" }));
            Assert.IsFalse((new List<string> { "test", "test2" }).IntersectsExactlyWith(new List<string> { "Test3" }));
            Assert.IsFalse((new List<string> { "test", "test2" }).IntersectsExactlyWith(new List<string> { "Test3", "test5" }));
            Assert.IsFalse((new List<string> { "test", "test2" }).IntersectsExactlyWith(new List<string> { string.Empty }));
        }
    }
}
