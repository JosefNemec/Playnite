using NUnit.Framework;
using Playnite.Common;
using Playnite.Controllers;
using Playnite.Scripting.PowerShell;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    [TestFixture]
    public class GamesEditorTests
    {
        private const string executeScriptActionTestFileName = "test.txt";

        [Test]
        public void ExecuteScriptActionPowerShellTest()
        {
            using (var runtime = new PowerShellRuntime("test"))
            ExecuteScriptActionTest(runtime, $"'PowerShell' | Out-File {executeScriptActionTestFileName}");
        }

        public void ExecuteScriptActionTest(PowerShellRuntime runtime, string script)
        {
            using (var tempDir = TempDirectory.Create())
            {
                var game = new Game()
                {
                    InstallDirectory = tempDir.TempPath
                };

                var editor = new GamesEditor(null, new GameControllerFactory(null), new PlayniteSettings(), null, null, new TestPlayniteApplication(), null);
                editor.ExecuteScriptAction(runtime, script, game, true, false, GameScriptType.None);
                var testPath = Path.Combine(tempDir.TempPath, executeScriptActionTestFileName);
                var content = File.ReadAllText(testPath);
                Assert.AreEqual("PowerShell", content.Trim());
            }
        }
    }
}
