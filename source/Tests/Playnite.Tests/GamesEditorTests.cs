using NUnit.Framework;
using Playnite.Common;
using Playnite.Controllers;
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
            ExecuteScriptActionTest(ScriptLanguage.PowerShell, $"'PowerShell' | Out-File {executeScriptActionTestFileName}");
        }

        [Test]
        public void ExecuteScriptActionIronPythonTest()
        {
            ExecuteScriptActionTest(ScriptLanguage.IronPython, string.Format(@"f = open('{0}', 'w')
f.write('IronPython')
f.close()", executeScriptActionTestFileName));
        }

        [Test]
        public void ExecuteScriptActionBatchTest()
        {
            ExecuteScriptActionTest(ScriptLanguage.Batch, $"echo Batch> {executeScriptActionTestFileName}");
        }

        public void ExecuteScriptActionTest(ScriptLanguage language, string script)
        {
            using (var tempDir = TempDirectory.Create())
            {
                var game = new Game()
                {
                    InstallDirectory = tempDir.TempPath
                };

                var editor = new GamesEditor(null, new GameControllerFactory(null), null, null, null, null);
                editor.ExecuteScriptAction(language, script, game);
                var testPath = Path.Combine(tempDir.TempPath, executeScriptActionTestFileName);
                var content = File.ReadAllText(testPath);
                Assert.AreEqual(language.ToString(), content.Trim());    
            }
        }
    }
}
