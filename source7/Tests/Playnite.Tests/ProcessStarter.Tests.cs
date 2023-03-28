using System.Diagnostics;

namespace Playnite.Tests;

[TestFixture]
public class ProcessStarterTests
{
    [Test]
    public async Task StartProcessWaitTest()
    {
        var testProc = ProcessStarter.StartProcess(TestsVars.ProcessRunTesterExe, TestsVars.ProcessRunTesterKeepRunningArg);
        Assert.AreEqual(1, Process.GetProcessesByName(TestsVars.ProcessRunTesterProcessName).Length);

        var ivalidRes = ProcessStarter.StartProcessWait(CmdLineTools.TaskKill, "/f /pid 999999", true);
        Assert.AreEqual(128, ivalidRes);

        var validRes = ProcessStarter.StartProcessWait(CmdLineTools.TaskKill, $"/f /pid {testProc.Id}", true);
        Assert.AreEqual(0, validRes);
        await Task.Delay(200);
        Assert.AreEqual(0, Process.GetProcessesByName(TestsVars.ProcessRunTesterExe).Length);
    }

    [Test]
    public async Task ShellExecuteTest()
    {
        var procid = ProcessStarter.ShellExecute($"\"{TestsVars.ProcessRunTesterExe}\" {TestsVars.ProcessRunTesterKeepRunningArg}");
        Assert.AreNotEqual(0, procid);
        await Task.Delay(200);

        Assert.AreEqual(1, Process.GetProcessesByName(TestsVars.ProcessRunTesterProcessName).Length);
        ProcessStarter.ShellExecute($"{CmdLineTools.TaskKill} /f /pid {procid}");
        await Task.Delay(200);
        Assert.AreEqual(0, Process.GetProcessesByName(TestsVars.ProcessRunTesterProcessName).Length);
    }

    [Test]
    public void StartProcessWaitStdTest()
    {
        ProcessStarter.StartProcessWait(CmdLineTools.IPConfig, string.Empty, string.Empty, out var stdOut, out var stdErr);
        StringAssert.Contains("Windows IP Configuration", stdOut);
        Assert.That(stdErr, Is.Empty);

        ProcessStarter.StartProcessWait(CmdLineTools.TaskKill, "/pid 999999", string.Empty, out var stdOut2, out var stdErr2);
        StringAssert.Contains("ERROR: The process", stdErr2);
        Assert.That(stdOut2, Is.Empty);
    }
}
