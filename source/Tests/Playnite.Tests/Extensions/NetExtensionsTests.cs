using NUnit.Framework;
using Playnite;
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests.Extensions
{
    [TestFixture]
    public class NetExtensionsTests
    {
        [Test]
        public void IsSuccessTest()
        {
            Assert.IsTrue(HttpStatusCode.OK.IsSuccess());
            Assert.IsTrue(HttpStatusCode.NoContent.IsSuccess());
            Assert.IsFalse(HttpStatusCode.Continue.IsSuccess());
            Assert.IsFalse(HttpStatusCode.Redirect.IsSuccess());
        }
    }
}
