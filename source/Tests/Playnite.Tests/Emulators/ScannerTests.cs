using NUnit.Framework;
using Playnite.Common;
using Playnite.Emulators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests.Emulators
{
    [TestFixture]
    public class ScannerTests
    {
        [Test]
        public void DoubleExtensionTest() // #2768 bug
        {
            using (var tempPath = TempDirectory.Create())
            {
                var path1 = Path.Combine(tempPath.TempPath, "game 1.p8");
                var path2 = Path.Combine(tempPath.TempPath, "game 2.p8.png");
                FileSystem.CreateFile(path1);
                FileSystem.CreateFile(path2);
                FileSystem.CreateFile(Path.Combine(tempPath.TempPath, "game 3.png"));

                var scanner = new GameScanner(new SDK.Models.GameScannerConfig(), null, null);
                var scanResults = new Dictionary<string, List<ScannedRom>>();
                scanner.ScanDirectoryBase(
                    tempPath.TempPath,
                    new List<string> { "p8", "p8.png" },
                    null,
                    scanResults,
                    new System.Threading.CancellationTokenSource().Token,
                    null);

                Assert.AreEqual(2, scanResults.Count);
                Assert.AreEqual("game 1", scanResults["game 1"][0].Name.Name);
                Assert.AreEqual(path1, scanResults["game 1"][0].Path);
                Assert.AreEqual("game 2", scanResults["game 2"][0].Name.Name);
                Assert.AreEqual(path2, scanResults["game 2"][0].Path);
            }
        }
    }
}
