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
            string[] invokedArgs = null;
            var handler = new PlayniteUriHandler();
            handler.RegisterSource("test", (args) =>
            {
                invokedArgs = args.Arguments;
            });

            handler.ProcessUri("playnite://test/arg1/arg2/arg3");
            Assert.AreEqual("arg1", invokedArgs[0]);
            Assert.AreEqual("arg2", invokedArgs[1]);
            Assert.AreEqual("arg3", invokedArgs[2]);
        }
    }
}
