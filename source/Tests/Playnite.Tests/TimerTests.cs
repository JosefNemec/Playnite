using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NUnit.Framework;
using Playnite;
using Playnite.Common;

namespace Playnite.Tests
{
    [TestFixture]
    public class TimerTests
    {
        [Test]
        public void TimeConvertersTests()
        {
            Assert.AreEqual(1000, Timer.SecondsToMilliseconds(1));
            Assert.AreEqual(60000, Timer.MinutesToMilliseconds(1));
            Assert.AreEqual(3600000, Timer.HoursToMilliseconds(1));
        }
    }
}
