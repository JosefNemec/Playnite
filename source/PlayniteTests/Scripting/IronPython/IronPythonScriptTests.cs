using NUnit.Framework;
using Playnite.Scripting.IronPython;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.Scripting.IronPython
{
    [TestFixture]
    public class IronPythonScriptTests
    {
        [Test]
        public void ScriptParserTest()
        {
            using (var py = new IronPythonScript(@"e:\devel\repos\Playnite\source\PlayniteUI\Scripts\IronPython\ExportLibrary.py"))
            {
                Assert.IsNotNull(py.Attributes);
                Assert.IsNotNull(py.FunctionExports);
            }
        }
    }
}
