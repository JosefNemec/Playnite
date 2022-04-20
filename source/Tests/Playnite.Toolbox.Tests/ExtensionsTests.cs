using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Playnite.SDK;

namespace Playnite.Toolbox.Tests
{
    [TestFixture]
    public class ExtensionsTests
    {
        [Test]
        public void ConvertToValidIdentifierNameTest()
        {
            Assert.AreEqual("itchio2", Extensions.ConvertToValidIdentifierName("itch.io@$#% 2"));
        }
    }
}
