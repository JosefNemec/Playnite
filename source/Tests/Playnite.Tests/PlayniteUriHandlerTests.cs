using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    [TestFixture]
    public class PlayniteUriHandlerTests
    {
        [Test]
        public void BasicProcessingTest()
        {
            var uri = @"playnite://linkutilities/AddLink/https%3A%2F%2Fxyz.de%2Fabc/Test 33";
            var (source, arguments) = PlayniteUriHandler.ParseUri(uri);
            Assert.AreEqual("linkutilities", source);
            Assert.AreEqual(3, arguments.Length);
            Assert.AreEqual("AddLink", arguments[0]);
            Assert.AreEqual("https://xyz.de/abc", arguments[1]);
            Assert.AreEqual("Test 33", arguments[2]);

            uri = @"playnite://test/arg1/arg2/arg3";
            (source, arguments) = PlayniteUriHandler.ParseUri(uri);
            Assert.AreEqual("test", source);
            Assert.AreEqual(3, arguments.Length);
            Assert.AreEqual("arg1", arguments[0]);
            Assert.AreEqual("arg2", arguments[1]);
            Assert.AreEqual("arg3", arguments[2]);
        }
    }
}
