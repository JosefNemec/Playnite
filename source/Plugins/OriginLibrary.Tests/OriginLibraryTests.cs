using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;
using Playnite;
using Playnite.SDK.Models;
using Moq;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.API;
using Playnite.Tests;
using Playnite.Common;

namespace OriginLibrary.Tests
{
    [TestFixture]
    public class OriginLibraryTests
    {
        public static OriginLibrary CreateLibrary()
        {
            return new OriginLibrary(PlayniteTests.GetTestingApi().Object);
        }

        [Test]
        public void GetInstalledGamesTest()
        {
            var originLib = CreateLibrary();
            var games = originLib.GetInstalledGames();
            Assert.AreNotEqual(0, games.Count);

            var game = games.Values.First();
            Assert.IsTrue(!string.IsNullOrEmpty(game.Name));
            Assert.IsTrue(!string.IsNullOrEmpty(game.GameId));
            Assert.IsTrue(Directory.Exists(game.InstallDirectory));
            Assert.IsNotNull(game.PlayAction);

            foreach (var g in games.Values)
            {
                if (g.PlayAction.Type == GameActionType.File)
                {
                    var file = Path.Combine(g.InstallDirectory, g.ExpandVariables(g.PlayAction.Path));
                    Assert.IsTrue(File.Exists(file));
                }
            }
        }

        [Test]
        public void GetPathFromPlatformPathTest()
        {
            var originLib = CreateLibrary();
            var path = originLib.GetPathFromPlatformPath(@"[HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\PowerShell\\3\\PowerShellEngine\\ApplicationBase]\\powershell.exe");
            Assert.IsTrue(File.Exists(path.CompletePath));

            path = originLib.GetPathFromPlatformPath(@"[HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\PowerShell\\3\\PowerShellEngine\\NonSense]\\powershell.exe");
            Assert.IsNull(path);
        }
    }
}
