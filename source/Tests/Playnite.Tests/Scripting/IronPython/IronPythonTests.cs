using NUnit.Framework;
using Playnite.Scripting.IronPython;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using IronPython.Runtime.Exceptions;
using Playnite.Common;
using System.IO;
using Playnite.SDK.Exceptions;

namespace Playnite.Tests.Scripting.IronPython
{
    [TestFixture]
    public class IronPythonTests
    {
        [Test]
        public void ExecuteTest()
        {
            using (var py = new IronPythonRuntime())
            {
                var res = py.Execute("2 + 2");
                Assert.AreEqual(4, res);
            }
        }

        [Test]
        public void FunctionExecuteTest()
        {
            using (var py = new IronPythonRuntime())
            {
                py.Execute(@"
def test_func():
    return 4 + 4
");
                var res = py.Execute("test_func()");
                Assert.AreEqual(8, res);
            }
        }

        [Test]
        public void ExecuteFunctionArgumentsTest()
        {
            using (var py = new IronPythonRuntime())
            {
                py.Execute(@"
def test_func(param1, param2):
    return param1 + param2
");

                var res = py.Execute("test_func(pr1, pr2)",
                    new Dictionary<string, object>()
                    {
                        { "pr1", 1 },
                        { "pr2", 2 }
                    });

                Assert.AreEqual(3, res);
            }
        }

        [Test]
        public void GetFunctionExitsTest()
        {
            using (var py = new IronPythonRuntime())
            {
                Assert.IsFalse(py.GetFunctionExits("test_func"));
                py.Execute(@"
def test_func():
    return 4 + 4
");
                Assert.IsTrue(py.GetFunctionExits("test_func"));
            }
        }

        [Test]
        public void ErrorHandlingTest()
        {
            using (var py = new IronPythonRuntime())
            {
                Assert.Throws<ScriptRuntimeException>(() => py.Execute("1 / 0"));
            }
        }

        [Test]
        public void ExecuteWorkDirTest()
        {
            using (var tempDir = TempDirectory.Create())
            using (var runtime = new IronPythonRuntime())
            {
                var outPath = "workDirTest.txt";
                FileSystem.DeleteFile(outPath);
                FileAssert.DoesNotExist(outPath);
                runtime.Execute(@"f = open('workDirTest.txt', 'w')
f.write('test')
f.close()");
                FileAssert.Exists(outPath);

                outPath = Path.Combine(tempDir.TempPath, outPath);
                FileSystem.DeleteFile(outPath);
                FileAssert.DoesNotExist(outPath);
                runtime.Execute(@"f = open('workDirTest.txt', 'w')
f.write('test')
f.close()", tempDir.TempPath);
                FileAssert.Exists(outPath);
            }
        }
    }
}
