using NUnit.Framework;
using Playnite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [Test]
        public void NormalizeGameNameTest()
        {
            Assert.AreEqual("Middle-earth: Shadow of War",
                StringExtensions.NormalizeGameName("Middle-earth™: Shadow of War™"));

            Assert.AreEqual("Command & Conquer Red Alert 3: Uprising: Best Game",
                StringExtensions.NormalizeGameName("Command®   & Conquer™ Red_Alert 3™ : Uprising©:_Best Game"));

            Assert.AreEqual("The Witcher 3",
                StringExtensions.NormalizeGameName("Witcher 3, The"));

            Assert.AreEqual("Pokemon Red Test",
                StringExtensions.NormalizeGameName("Pokemon.Red.[US].[l33th4xor].Test.[22]"));

            Assert.AreEqual("Pokemon Red Test",
                StringExtensions.NormalizeGameName("Pokemon.Red.[US].(l33th 4xor).Test.(22)"));
        }

        [Test]
        public void ConvertToSortableName()
        {
            Assert.AreEqual("Witcher 3", StringExtensions.ConvertToSortableName("The Witcher 3"));
            Assert.AreEqual("Witcher 3", StringExtensions.ConvertToSortableName("the Witcher 3"));
            Assert.AreEqual("Game", StringExtensions.ConvertToSortableName("A Game"));
            Assert.AreEqual("Usual Game", StringExtensions.ConvertToSortableName("An Usual Game"));
            Assert.AreEqual("AnUsual Game", StringExtensions.ConvertToSortableName("AnUsual Game"));
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
        public void IsNullOrWhiteSpaceTest()
        {
            var multiLine = @"

   

";
            Assert.IsTrue(multiLine.IsNullOrWhiteSpace());
        }

    }
}
