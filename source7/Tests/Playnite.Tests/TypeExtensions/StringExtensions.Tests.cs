namespace Playnite.Tests;

[TestFixture]
public class StringExtensionsTests
{
    [Test]
    public void NormalizeGameNameTest()
    {
        Assert.AreEqual("Middle-earth: Shadow of War", StringExtensions.NormalizeGameName("Middle-earth™: Shadow of War™"));
        Assert.AreEqual("Command & Conquer Red Alert 3: Uprising: Best Game", StringExtensions.NormalizeGameName("Command®   & Conquer™ Red_Alert 3™ : Uprising©:_Best Game"));
        Assert.AreEqual("The Witcher 3", StringExtensions.NormalizeGameName("Witcher 3, The"));
        Assert.AreEqual("Pokemon Red Test", StringExtensions.NormalizeGameName("Pokemon.Red.[US].[l33th4xor].Test.[22]"));
        Assert.AreEqual("Pokemon Red Test", StringExtensions.NormalizeGameName("Pokemon.Red.[US].(l33th 4xor).Test.(22)"));
        Assert.AreEqual("[PROTOTYPE]", StringExtensions.NormalizeGameName("[PROTOTYPE]™"));
        Assert.AreEqual("(PROTOTYPE2)", StringExtensions.NormalizeGameName("(PROTOTYPE2)™"));
        Assert.AreEqual(string.Empty, StringExtensions.NormalizeGameName(string.Empty));
    }

    [Test]
    public void GetPathWithoutAllExtensionsTest()
    {
        Assert.AreEqual(@"c:\test\SomeFile", StringExtensions.GetPathWithoutAllExtensions(@"c:\test\SomeFile.zip"));
        Assert.AreEqual(@"SomeFile", StringExtensions.GetPathWithoutAllExtensions(@"SomeFile.r888s.42rar.zip1"));
        Assert.AreEqual(@"SomeFile", StringExtensions.GetPathWithoutAllExtensions(@"SomeFile"));
        Assert.AreEqual(@"SomeFile", StringExtensions.GetPathWithoutAllExtensions(@"SomeFile.Test.42rar.zip1"));
        Assert.AreEqual(@"SomeFile.zip1test.aa_aa", StringExtensions.GetPathWithoutAllExtensions(@"SomeFile.zip1test.aa_aa.zip"));
        Assert.AreEqual(@"SomeFile.zip1_test", StringExtensions.GetPathWithoutAllExtensions(@"SomeFile.zip1_test"));
    }

    [Test]
    public void IsHttpUrlTest()
    {
        Assert.IsTrue(@"http://www.playnite.link".IsHttpUrl());
        Assert.IsTrue(@"https://playnite.link".IsHttpUrl());
        Assert.IsTrue(@"HTTP://WWW.PLAYNITE.LINK".IsHttpUrl());
        Assert.IsTrue(@"HTTPS://PLAYNITE.LINK".IsHttpUrl());
        Assert.IsFalse(@"ftp://www.playnite.link".IsHttpUrl());
        Assert.IsFalse(@"www.playnite.link".IsHttpUrl());
    }

    [Test]
    public void IsUriTest()
    {
        Assert.IsTrue(@"http://www.playnite.link".IsUri());
        Assert.IsFalse(@"www.playnite.link".IsUri());
        Assert.IsTrue(@"mailto:someadress@test.cz".IsUri());
        Assert.IsTrue(@"playnite://test/test".IsUri());
        Assert.IsFalse(@"testsstring".IsUri());
        Assert.IsFalse(@"c:\test\aa.txt".IsUri());
    }

    [Test]
    public void IsNullOrWhiteSpaceTest()
    {
        var multiLine = @"

";
        Assert.IsTrue(multiLine.IsNullOrWhiteSpace());
    }

    [Test]
    public void TrimEndStringTest()
    {
        Assert.AreEqual("Test ", "Test totrim".TrimEndString("totrim"));
        Assert.AreEqual("Test totrim", "Test totrim".TrimEndString("aaa"));
    }

    [Test]
    public void ToTileCaseTest()
    {
        Assert.AreEqual("Test Is Good", "tEst is gOOD".ToTileCase());
    }

    [Test]
    public void ContainsAnyTest()
    {
        Assert.IsTrue("test[dasd".ContainsAny(new char[] { ']', '[' }));
        Assert.IsFalse("test dasd".ContainsAny(new char[] { ']', '[' }));
    }

    [Test]
    public void GetLineCountTest()
    {
        Assert.AreEqual(0, ((string)null!).GetLineCount());
        Assert.AreEqual(1, ("").GetLineCount());
        Assert.AreEqual(2, ("\n").GetLineCount());
        Assert.AreEqual(3, ("line1\nline2\nline3").GetLineCount());
    }

    [Test]
    public void EndWithDirSeparatorTest()
    {
        Assert.AreEqual(@"", "".EndWithDirSeparator());
        Assert.IsNull(((string)null!).EndWithDirSeparator());
        Assert.AreEqual(@"test\", "test".EndWithDirSeparator());
        Assert.AreEqual(@"test\", @"test\".EndWithDirSeparator());
        Assert.AreEqual(@"test\", @"test\\\".EndWithDirSeparator());
    }

    [Test]
    public void UriCombineTest()
    {
        Assert.AreEqual("https://test.domain.com/seg1/seg2", "https://test.domain.com/".UriCombine("/seg1", "/seg2"));
        Assert.AreEqual("https://test.domain.com/seg1/seg2/", "https://test.domain.com".UriCombine("seg1", "/seg2/"));
    }
}
