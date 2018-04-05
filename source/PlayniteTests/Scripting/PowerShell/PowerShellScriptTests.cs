using NUnit.Framework;
using Playnite.Scripting.PowerShell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.Scripting.PowerShell
{
    [TestFixture]
    public class PowerShellScriptTests
    {
        [Test]
        public void ScriptParserTest()
        {
            using (var ps = new PowerShellScript(@"e:\devel\repos\Playnite\source\PlayniteUI\Scripts\PowerShell\ExportLibrary.ps1"))
            {
                Assert.IsNotNull(ps.Attributes);
                Assert.IsNotNull(ps.FunctionExports);
            }
        }
    }
}
