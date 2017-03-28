using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Playnite.Providers.Origin.Tests
{
    [TestClass()]
    public class OriginTests
    {
        [TestMethod()]
        public void GetInstalledGamesTest()
        {
            var games = Origin.GetInstalledGames(false);
            Assert.AreNotEqual(0, games.Count);

            var game = games[0];
            Assert.AreEqual(Models.Provider.Origin, game.Provider);
            Assert.IsTrue(!string.IsNullOrEmpty(game.Name));
            Assert.IsTrue(!string.IsNullOrEmpty(game.ProviderId));
            Assert.IsTrue(Directory.Exists(game.InstallDirectory));
            Assert.IsNotNull(game.PlayTask);

            foreach (var g in games)
            {
                if (g.PlayTask.Type == Models.GameTaskType.File)
                {
                    Assert.IsTrue(File.Exists(g.PlayTask.Path));
                }
            }
        }

        [TestMethod()]
        public void GetInstalledGamesCacheTest()
        {
            OriginPaths.CachePath = Path.Combine(Playnite.PlayniteTests.TempPath, "origincache");
            FileSystem.CreateFolder(OriginPaths.CachePath, true);

            var games = Origin.GetInstalledGames(true);
            var cacheFiles = Directory.GetFiles(OriginPaths.CachePath, "*.json");
            Assert.IsTrue(cacheFiles.Count() > 0);
        }

        [TestMethod()]
        public void DownloadGameMetadataTest()
        {
            var existingStore = Origin.DownloadGameMetadata("OFB-EAST:60108");
            Assert.IsNotNull(existingStore.StoreDetails);
            Assert.IsNotNull(existingStore.Image.Data);
        }

        [TestMethod()]
        public void GetPathFromPlatformPathTest()
        {
            var path = Origin.GetPathFromPlatformPath(@"[HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\PowerShell\\3\\PowerShellEngine\\ApplicationBase]\\powershell.exe");
            Assert.IsTrue(File.Exists(path));

            path = Origin.GetPathFromPlatformPath(@"[HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\PowerShell\\3\\PowerShellEngine\\ApplicatioBase]\\powershell.exe");
            Assert.IsTrue(string.IsNullOrEmpty(path));
        }
    }
}
