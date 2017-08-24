using NUnit.Framework;
using Playnite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.Models
{
    [TestFixture]
    public class GameTests
    {
        [Test]
        public void ResolveVariablesTest()
        {
            var dir = @"c:\test\test2\";
            var game = new Game()
            {
                InstallDirectory = dir
            };

            Assert.AreEqual(game.ResolveVariables("teststring"), "teststring");
            Assert.AreEqual(game.ResolveVariables("{InstallDir}teststring"), dir + "teststring");
            Assert.AreEqual(game.ResolveVariables(string.Empty), string.Empty);
        }
    }
}
