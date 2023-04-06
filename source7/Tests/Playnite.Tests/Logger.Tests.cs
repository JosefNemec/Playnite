using System.IO;

namespace Playnite.Tests;

[TestFixture]
public class LoggerTests
{
    [Test]
    public void GetFinalPathNameTest()
    {
        var testLogFile = Path.Combine(PlaynitePaths.ProgramDir, "playnite_test.log");
        FileSystem.DeleteFile(testLogFile);

        var testNum = GlobalRandom.Next();
        var logger = LogManager.GetLogger("testLogger");
        logger.Debug($"test message {testNum}");
        StringAssert.Contains(
            $"DEBUG|testLogger:test message {testNum}",
            FileSystem.ReadStringFromFile(PlaynitePaths.LogFile));

        testNum = GlobalRandom.Next();
        var logger2 = LogManager.GetLogger();
        logger2.Warn($"test message {testNum}");
        StringAssert.Contains(
            $"WARN |LoggerTests:test message {testNum}",
            FileSystem.ReadStringFromFile(PlaynitePaths.LogFile));

        testNum = GlobalRandom.Next();
        var logger3 = LogManager.GetLogger("test", testLogFile);
        logger3.Warn($"different file test {testNum}");
        StringAssert.Contains(
            $"WARN |test:different file test {testNum}",
            FileSystem.ReadStringFromFile(testLogFile));
    }
}
