using System.IO;

namespace Playnite.Tests;

[TestFixture]
public class ProgramsTests
{
    [Test]
    public async Task GetInstalledProgramsTest()
    {
        var apps = await Programs.GetInstalledPrograms(CancellationToken.None);
        Assert.That(apps.Count, Is.GreaterThan(0));

        var firstApp = apps.First();
        Assert.IsFalse(string.IsNullOrEmpty(firstApp.Icon));
        Assert.IsFalse(string.IsNullOrEmpty(firstApp.Name));
        Assert.IsFalse(string.IsNullOrEmpty(firstApp.Path));
    }

    [Test]
    public async Task GetExecutablesFromFolderTest()
    {
        var apps = await Programs.GetExecutablesFromFolder(PlaynitePaths.ProgramPath, System.IO.SearchOption.AllDirectories, CancellationToken.None);
        Assert.That(apps.Count, Is.GreaterThan(0));

        var firstApp = apps.First();
        Assert.IsFalse(string.IsNullOrEmpty(firstApp.Icon));
        Assert.IsFalse(string.IsNullOrEmpty(firstApp.Name));
        Assert.IsFalse(string.IsNullOrEmpty(firstApp.Path));
        Assert.IsFalse(string.IsNullOrEmpty(firstApp.WorkDir));
    }

    [Test]
    public void GetUnistallProgramsListTest()
    {
        CollectionAssert.IsNotEmpty(Programs.GetUnistallProgramsList());
    }

    [Test]
    public void CreateShortcutTest()
    {
        using var tempDir = TempDirectory.Create();
        var filePath = TestVars.ProcessRunTesterExe;
        var linkPath = Path.Combine(tempDir, "link.lnk");
        Programs.CreateShortcut(
            linkPath, filePath,
            "test test2",
            Path.GetDirectoryName(filePath),
            @"%SystemRoot%\System32\SHELL32.dll",
            10);

        var lnk = Programs.GetLnkShortcutData(linkPath);
        Assert.AreEqual(filePath, lnk.Path);
        Assert.AreEqual(Path.GetDirectoryName(filePath), lnk.WorkDir);
        Assert.AreEqual("test test2", lnk.Arguments);
        Assert.AreEqual("link", lnk.Name);
        Assert.AreEqual(@"%SystemRoot%\System32\SHELL32.dll", lnk.Icon);
        Assert.AreEqual(10, lnk.IconIndex);
    }

    [Test]
    public void GetLnkShortcutDataTest()
    {
        var lnk = Programs.GetLnkShortcutData(Path.Combine(TestVars.ResourcesDir, "shortcut.lnk"));
        Assert.AreEqual(@"D:\Downloads\PlayniteInstaller.exe", lnk.Path);
        Assert.AreEqual(@"D:\Downloads", lnk.WorkDir);
        Assert.AreEqual("test test2", lnk.Arguments);
        Assert.AreEqual("shortcut", lnk.Name);
        Assert.AreEqual(@"D:\Downloads\PlayniteInstaller.exe", lnk.Icon);
        Assert.AreEqual(0, lnk.IconIndex);

        lnk = Programs.GetLnkShortcutData(Path.Combine(TestVars.ResourcesDir, "shortcut_icon.lnk"));
        Assert.AreEqual(@"%SystemRoot%\System32\SHELL32.dll", lnk.Icon);
        Assert.AreEqual(3, lnk.IconIndex);
    }

    [Test]
    public void GetUWPAppsTest()
    {
        var apps = Programs.GetUWPApps();
        Assert.That(apps.Count, Is.GreaterThan(0));

        var firstApp = apps.First();
        Assert.IsFalse(string.IsNullOrEmpty(firstApp.Icon));
        Assert.IsFalse(string.IsNullOrEmpty(firstApp.Name));
        Assert.IsFalse(string.IsNullOrEmpty(firstApp.Path));
    }

    [Test]
    public void IsPathScanExcludedTest()
    {
        // Uninstallers
        Assert.IsTrue(Programs.IsFileScanExcluded("unins000.exe"));
        Assert.IsTrue(Programs.IsFileScanExcluded("setup1.exe"));
        Assert.IsTrue(Programs.IsFileScanExcluded("setup.exe"));
        Assert.IsFalse(Programs.IsFileScanExcluded("test.exe"));

        // Config executables and Redistributables
        Assert.IsTrue(Programs.IsFileScanExcluded("config.exe"));
        Assert.IsTrue(Programs.IsFileScanExcluded("aConfigFile.exe"));
        Assert.IsTrue(Programs.IsFileScanExcluded("DXSETUP.exe"));
        Assert.IsTrue(Programs.IsFileScanExcluded("vc_redist.x64.exe"));
        Assert.IsTrue(Programs.IsFileScanExcluded("vc_redist.x86.exe"));

        // Game engines executables
        Assert.IsTrue(Programs.IsFileScanExcluded("UnityCrashHandler32.exe"));
        Assert.IsTrue(Programs.IsFileScanExcluded("UnityCrashHandler64.exe"));
        Assert.IsTrue(Programs.IsFileScanExcluded("notification_helper.exe"));
        Assert.IsTrue(Programs.IsFileScanExcluded("python.exe"));
        Assert.IsTrue(Programs.IsFileScanExcluded("pythonw.exe"));
        Assert.IsTrue(Programs.IsFileScanExcluded("zsync.exe"));
        Assert.IsTrue(Programs.IsFileScanExcluded("zsyncmake.exe"));
        Assert.IsFalse(Programs.IsFileScanExcluded("otherPythonFile.exe"));
    }
}
