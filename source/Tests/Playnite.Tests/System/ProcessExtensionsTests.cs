using NUnit.Framework;
using Playnite.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
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
