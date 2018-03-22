using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;
using Playnite.Providers.Origin;
using Playnite.Models;
using Playnite;

namespace PlayniteTests.Providers.Origin
{
    [TestFixture]
    public class OriginLibraryTests
    {
        [Test]
        public void GetInstalledGamesTest()
        {
            var originLib = new OriginLibrary();
            var games = originLib.GetInstalledGames(false);
            Assert.AreNotEqual(0, games.Count);

            var game = games[0];
            Assert.AreEqual(Provider.Origin, game.Provider);
            Assert.IsTrue(!string.IsNullOrEmpty(game.Name));
            Assert.IsTrue(!string.IsNullOrEmpty(game.ProviderId));
            Assert.IsTrue(Directory.Exists(game.InstallDirectory));
            Assert.IsNotNull(game.PlayTask);

            foreach (Game g in games)
            {
                if (g.PlayTask.Type == GameTaskType.File)
                {                     
                    Assert.IsTrue(File.Exists(g.ResolveVariables(g.PlayTask.Path)));
                }
            }
        }

        [Test]
        public void GetInstalledGamesCacheTest()
        {
            var originLib = new OriginLibrary();
            OriginPaths.CachePath = Path.Combine(Playnite.PlayniteTests.TempPath, "origincache");
            FileSystem.CreateDirectory(OriginPaths.CachePath, true);

            var games = originLib.GetInstalledGames(true);
            var cacheFiles = Directory.GetFiles(OriginPaths.CachePath, "*.json");
            Assert.IsTrue(cacheFiles.Count() > 0);
        }

        [Test]
        public void DownloadGameMetadataTest()
        {
            var originLib = new OriginLibrary();
            var existingStore = originLib.DownloadGameMetadata("OFB-EAST:60108");
            Assert.IsNotNull(existingStore.StoreDetails);
            Assert.IsNotNull(existingStore.Image.Data);
        }

        [Test]
        public void GetPathFromPlatformPathTest()
        {
            var originLib = new OriginLibrary();
            var path = originLib.GetPathFromPlatformPath(@"[HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\PowerShell\\3\\PowerShellEngine\\ApplicationBase]\\powershell.exe");
            Assert.IsTrue(File.Exists(path));

            path = originLib.GetPathFromPlatformPath(@"[HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\PowerShell\\3\\PowerShellEngine\\ApplicatioBase]\\powershell.exe");
            Assert.IsTrue(string.IsNullOrEmpty(path));
        }
    }
}
