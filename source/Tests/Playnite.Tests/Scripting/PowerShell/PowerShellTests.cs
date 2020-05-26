using NUnit.Framework;
using Playnite.Common;
using Playnite.Scripting.PowerShell;
using Playnite.SDK.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests.Scripting.PowerShell
{
    [TestFixture]
    public class PowerShellTests
    {
        [Test]
        public void ExecuteTest()
        {
            using (var ps = new PowerShellRuntime())
            {
                var res = ps.Execute("return 2 + 2");
                Assert.AreEqual(4, res);
            }
        }

        [Test]
        public void ExecuteArgumentsTest()
        {
            using (var ps = new PowerShellRuntime())
            {
                var res = ps.Execute("return $param1 + $param2",
                    new Dictionary<string, object>()
                    {
                        { "param1", 1 },
                        { "param2", 2 }
                    });

                Assert.AreEqual(3, res);
            }
        }

        [Test]
        public void FunctionExecuteTest()
        {
            using (var ps = new PowerShellRuntime())
            {
                ps.Execute(@"
function TestFunc()
{
    return 4 + 4
}
");
                var res = ps.Execute("TestFunc");
                Assert.AreEqual(8, res);
            }
        }

        [Test]
        public void ErrorHandlingTest()
        {
            using (var ps = new PowerShellRuntime())
            {
                Assert.Throws<ScriptRuntimeException>(() => ps.Execute("throw \"Testing Exception\""));
                Assert.Throws<ScriptRuntimeException>(() => ps.Execute("1 / 0"));
            }
        }

        [Test]
        public void GetFunctionExitsTest()
        {
            using (var ps = new PowerShellRuntime())
            {
                Assert.IsFalse(ps.GetFunctionExits("TestFunc"));
                ps.Execute(@"
function TestFunc()
{
    return 4 + 4
}
");
                Assert.IsTrue(ps.GetFunctionExits("TestFunc"));
            }
        }

        [Test]
        public void ExecuteWorkDirTest()
        {
            using (var tempDir = TempDirectory.Create())
            using (var runtime = new PowerShellRuntime())
            {
                var outPath = "workDirTest.txt";
                FileSystem.DeleteFile(outPath);
                FileAssert.DoesNotExist(outPath);
                runtime.Execute($"'test' | Out-File workDirTest.txt");
                FileAssert.Exists(outPath);

                outPath = Path.Combine(tempDir.TempPath, outPath);
                FileSystem.DeleteFile(outPath);
                FileAssert.DoesNotExist(outPath);
                runtime.Execute($"'test' | Out-File workDirTest.txt", tempDir.TempPath);
                FileAssert.Exists(outPath);
            }
        }
    }
}
