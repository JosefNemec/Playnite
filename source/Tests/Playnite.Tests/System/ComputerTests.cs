using NUnit.Framework;
using Playnite.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    [TestFixture]
    public class ComputerTests
    {
        [Test]
        public void GetMonitorsTest()
        {
            var screens = Computer.GetScreens();
            CollectionAssert.IsNotEmpty(screens);
            Assert.IsTrue(screens.Any(a => a.Primary));
        }

        [Test]
        public void GetSystemInfoTest()
        {
            var info = Computer.GetSystemInfo();
            CollectionAssert.IsNotEmpty(info.Gpus);
            Assert.IsFalse(string.IsNullOrEmpty(info.Cpu));
            Assert.IsFalse(string.IsNullOrEmpty(info.WindowsVersion));
            Assert.AreNotEqual(0, info.Ram);
        }
    }
}
