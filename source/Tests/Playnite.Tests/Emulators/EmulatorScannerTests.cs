using NUnit.Framework;
using Playnite.Common;
using Playnite.Emulators;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.Tests.Emulators
{
    [TestFixture]
    public class EmulatorScannerTests
    {
        [Test]
        public void RelativePathTest()
        {
            var programDir = PlaynitePaths.ProgramPath;
            var emuDef = new EmulatorDefinition
            {
                Id = "testApp",
                Name = "Test App",
                Profiles = new List<EmulatorDefinitionProfile>
            {
                new EmulatorDefinitionProfile
                {
                    ImageExtensions = new List<string> { "iso", "mp3" },
                    Name = "default",
                    StartupExecutable = @"^TestApp\.exe$",
                    StartupArguments = "some args"
                }
            }
            };

            var emus = EmulatorScanner.SearchForEmulators(programDir, new List<EmulatorDefinition> { emuDef }, CancellationToken.None);
            Assert.AreEqual(1, emus.Count);
            Assert.AreEqual(@"{PlayniteDir}\TestApp", emus[0].InstallDir);

            using (var temp = TempDirectory.Create())
            {
                FileSystem.CopyDirectory(TestPaths.TestAppDir, Path.Combine(temp.TempPath, "TestApp"));
                emus = EmulatorScanner.SearchForEmulators(temp.TempPath, new List<EmulatorDefinition> { emuDef }, CancellationToken.None);
                Assert.AreEqual(1, emus.Count);
                Assert.AreEqual(Path.Combine(temp.TempPath, "TestApp"), emus[0].InstallDir);
            }
        }
    }
}
