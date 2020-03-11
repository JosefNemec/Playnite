using NUnit.Framework;
using Playnite.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    [TestFixture]
    public class SigningToolsTests
    {
        [Test]
        public void IsTrustedTest()
        {
            var selfFilePath = Assembly.GetExecutingAssembly().Location;
            Assert.IsFalse(SigningTools.IsTrusted(selfFilePath));
            Assert.IsTrue(SigningTools.IsTrusted(@"c:\Windows\explorer.exe"));
        }
    }
}
