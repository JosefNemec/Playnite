using NUnit.Framework;
using Playnite.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    [TestFixture]
    public class IniParserTests
    {
        [Test]
        public void StandardTest()
        {
            var iniStr = File.ReadAllLines(Path.Combine(PlayniteTests.ResourcesPath, "TestIni.ini"));
            var ini = IniParser.Parse(iniStr);
            Assert.AreEqual("true", ini["RegionDoesRequirePermission"]["GameDev"]);
            Assert.IsNull(ini["test"]?["test2"]);
        }
    }
}
