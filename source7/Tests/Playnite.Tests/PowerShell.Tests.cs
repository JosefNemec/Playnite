using System.IO;

namespace Playnite.Tests;

[TestFixture]
public class PowerShellTests
{
    [Test]
    public void ExecuteTest()
    {
        using var ps = new PowerShellRuntime("ExecuteTest");
        var res = ps.Execute("return 2 + 2");
        Assert.AreEqual(4, res);
    }

    [Test]
    public void ExecuteArgumentsTest()
    {
        using var ps = new PowerShellRuntime("ExecuteArgumentsTest");
        var res = ps.Execute("return $param1 + $param2",
            variables: new Dictionary<string, object>()
            {
                { "param1", 1 },
                { "param2", 2 }
            });

        Assert.AreEqual(3, res);
    }

    [Test]
    public void FunctionExecuteTest()
    {
        using var ps = new PowerShellRuntime("FunctionExecuteTest");
        ps.Execute("""
            function TestFunc()
            {
                return 4 + 4
            }
            """);
        var res = ps.Execute("TestFunc");
        Assert.AreEqual(8, res);
        Assert.That(ps.Execute("Get-Process"), Is.Not.Empty);
    }

    [Test]
    public void ErrorHandlingTest()
    {
        using var ps = new PowerShellRuntime("ErrorHandlingTest");
        Assert.That(Assert.Throws<ScriptRuntimeException>(() => ps.Execute("throw \"Testing Exception\"")).ScriptStackTrace, Is.Not.Empty);
        Assert.That(Assert.Throws<ScriptRuntimeException>(() => ps.Execute("1 / 0")).ScriptStackTrace, Is.Not.Empty);
    }

    [Test]
    public void GetModuleFunctionTest()
    {
        using var tempDir = TempDirectory.Create();
        using var ps = new PowerShellRuntime("GetFunctionTest");
        Assert.IsNull(ps.GetModuleFunction("TestFunc"));
        var path = Path.Combine(tempDir, "GetFunctionTest.psm1");
        File.WriteAllText(path, """
            function TestFunc()
            {
                return 4 + 4
            }
            """);
        ps.ImportModule(path);
        Assert.IsNotNull(ps.GetModuleFunction("TestFunc"));
    }

    [Test]
    public void ExecuteWorkDirTest()
    {
        using var tempDir = TempDirectory.Create();
        Directory.SetCurrentDirectory(tempDir);
        using var runtime = new PowerShellRuntime("ExecuteWorkDirTest");

        try
        {
            var outPath = "workDirTest.txt";
            FileSystem.DeleteFile(outPath);
            FileAssert.DoesNotExist(outPath);
            runtime.Execute($"'test' | Out-File workDirTest.txt");
            FileAssert.Exists(outPath);

            FileSystem.CreateDirectory("subdirectory");
            var tempDir2 = Path.Combine(tempDir, "subdirectory");
            outPath = Path.Combine(tempDir2, outPath);
            FileSystem.DeleteFile(outPath);
            FileAssert.DoesNotExist(outPath);
            runtime.Execute($"'test' | Out-File workDirTest.txt", tempDir2);
            FileAssert.Exists(outPath);
        }
        finally
        {
            Directory.SetCurrentDirectory("\\");
        }
    }

    [Test]
    public void ExecuteFileTest()
    {
        using var tempDir = TempDirectory.Create();
        using var runtime = new PowerShellRuntime("ExecuteFileTest");

        var filePath = Path.Combine(tempDir, "ExecuteFileTest.ps1");
        File.WriteAllText(filePath, """
            param($FileArgs)
            return $FileArgs.Arg1 + $FileArgs.Arg2
            """);
        var res = runtime.ExecuteFile(filePath, null, new Dictionary<string, object>
        {
            { "Arg1", 2 },
            { "Arg2", 3 }
        });
        Assert.AreEqual(5, res);
    }
}
