using System.Diagnostics;

namespace Playnite.Tests;

[TestFixture]
public class ProcessExtensionsTests
{
    [Test]
    public void TryGetMainModuleFileNameTest()
    {
        Assert.IsTrue(Process.GetCurrentProcess().TryGetMainModuleFilePath(out var fileName));
        Assert.That(fileName, Is.Not.Empty);
    }

    [Test]
    public void TryGetParentIdTest()
    {
        Assert.IsTrue(Process.GetCurrentProcess().TryGetParentId(out var parentId));
        Assert.That(parentId, Is.GreaterThan(0));
    }

    [Test]
    public void IsRunningTest()
    {
        Assert.IsTrue(ProcessExtensions.IsRunning("svchost"));
        Assert.IsTrue(ProcessExtensions.IsRunning("svc\\S+"));
        Assert.IsFalse(ProcessExtensions.IsRunning("random"));
    }

    [Test]
    public void GetCommandLineTest()
    {
        Assert.That(Process.GetCurrentProcess().GetCommandLine(), Is.Not.Empty);
    }
}
