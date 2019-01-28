using NUnit.Framework;
using Playnite.Common.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests
{
    [TestFixture]
    public class ProcessExtensionsTests
    {
        [Test]
        public void IsRunningTest()
        {
            Assert.IsTrue(ProcessExtensions.IsRunning("svchost"));
            Assert.IsFalse(ProcessExtensions.IsRunning("perfwatson$"));
        }
    }
}
