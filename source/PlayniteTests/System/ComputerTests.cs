﻿using NUnit.Framework;
using Playnite.Common.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests
{
    [TestFixture]
    public class ComputerTests
    {
        [Test]
        public void GetMonitorsTest()
        {
            var screens = Computer.GetMonitors();
            CollectionAssert.IsNotEmpty(screens);
            Assert.IsTrue(screens.Any(a => a.IsPrimary));
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
