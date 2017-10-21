using NUnit.Framework;
using Playnite.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
                InstallDirectory = dir,
                IsoPath = Path.Combine(dir, "test.iso")
            };

            Assert.AreEqual(string.Empty, game.ResolveVariables(string.Empty));
            Assert.AreEqual("teststring", game.ResolveVariables("teststring"));
            Assert.AreEqual(dir + "teststring", game.ResolveVariables("{InstallDir}teststring"));
            Assert.AreEqual(game.InstallDirectory, game.ResolveVariables("{InstallDir}"));
            Assert.AreEqual(game.IsoPath, game.ResolveVariables("{ImagePath}"));
            Assert.AreEqual("test", game.ResolveVariables("{ImageNameNoExt}"));
            Assert.AreEqual("test.iso", game.ResolveVariables("{ImageName}"));
        }
    }
}
