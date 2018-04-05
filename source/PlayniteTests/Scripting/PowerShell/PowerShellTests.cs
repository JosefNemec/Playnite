using NUnit.Framework;
using Playnite.Scripting.PowerShell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.Scripting.PowerShell
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
                Assert.Throws<RuntimeException>(() => ps.Execute("throw \"Testing Exception\""));
                Assert.Throws<RuntimeException>(() => ps.Execute("1 / 0"));
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
    }
}
