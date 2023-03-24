using System.Reflection;

namespace Playnite.Tests;

[TestFixture]
public class PathsTests
{
    [Test]
    public void GetFinalPathNameTest()
    {
        Assert.False(string.IsNullOrWhiteSpace(Paths.GetFinalPathName(Assembly.GetExecutingAssembly().Location)));
        Assert.AreEqual(@"C:\Users", Paths.GetFinalPathName(@"c:\Documents and Settings"));
    }

    [Test]
    public void MathcesFilePattern()
    {
        Assert.IsTrue(Paths.MathcesFilePattern(@"c:\test\aaa.exe", "*.exe"));
        Assert.IsTrue(Paths.MathcesFilePattern(@"c:\test\aaa.exe", "*.*"));
        Assert.IsFalse(Paths.MathcesFilePattern(@"c:\test\aaa.exe", "*.doc"));
    }
}
