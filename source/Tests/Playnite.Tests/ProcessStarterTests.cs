using NUnit.Framework;
using Playnite.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    [TestFixture]
    public class ProcessStarterTests
    {
        [Test]
        public void StartProcessWaitTest()
        {
            var notepad = ProcessStarter.StartProcess("notepad");
            Assert.AreEqual(1, Process.GetProcessesByName("notepad").Count());

            var ivalidRes = ProcessStarter.StartProcessWait(CmdLineTools.TaskKill, "/pid 999999", null, true);
            Assert.AreEqual(128, ivalidRes);

            var validRes = ProcessStarter.StartProcessWait(CmdLineTools.TaskKill, $"/pid {notepad.Id}", null, true);
            Assert.AreEqual(0, validRes);
            Thread.Sleep(200);
            Assert.AreEqual(0, Process.GetProcessesByName("notepad").Count());
        }

        [Test]
        public void StartProcessWaitStdTest()
        {
            ProcessStarter.StartProcessWait(CmdLineTools.IPConfig, null, null, out var stdOut, out var stdErr);
            StringAssert.Contains("Windows IP Configuration", stdOut);
            Assert.IsTrue(stdErr.IsNullOrEmpty());

            ProcessStarter.StartProcessWait(CmdLineTools.TaskKill, "/pid 999999", null, out var stdOut2, out var stdErr2);
            StringAssert.Contains("ERROR: The process", stdErr2);
            Assert.IsTrue(stdOut2.IsNullOrEmpty());
        }
    }
}
