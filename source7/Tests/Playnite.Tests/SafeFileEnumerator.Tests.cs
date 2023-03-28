using System.IO;

namespace Playnite.Tests;

[TestFixture]
public class SafeFileEnumeratorTests
{
    [Test]
    public void OverUnsafeWorksTest()
    {
        var path = @"c:\Windows\appcompat\";

        var dirInfo = new DirectoryInfo(path);
        Assert.Throws<UnauthorizedAccessException>(() => dirInfo.GetFiles("*.*", SearchOption.AllDirectories));

        var enumerator = new SafeFileEnumerator(path, "*.*", SearchOption.AllDirectories);
        Assert.DoesNotThrow(() => enumerator.ToList());
    }

    [Test]
    public void StandardEnumTest()
    {
        var path = @"c:\Windows\appcompat\";
        var enumerator = new SafeFileEnumerator(path, "*.*", SearchOption.AllDirectories);
        var files = enumerator.ToList();
        CollectionAssert.IsNotEmpty(files);
    }
}
