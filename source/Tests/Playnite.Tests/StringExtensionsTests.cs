using NUnit.Framework;
using Playnite;
using Playnite.Common;
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
        [TestCase("Middle-earth™: Shadow of War™", "Middle-earth: Shadow of War")]
        [TestCase("Command®   & Conquer™ Red_Alert 3™ : Uprising©:_Best Game", "Command & Conquer Red Alert 3: Uprising: Best Game")]
        [TestCase("Witcher 3, The", "The Witcher 3")]
        [TestCase("Pokemon.Red.[US].[l33th4xor].Test.[22]", "Pokemon Red Test")]
        [TestCase("Pokemon.Red.[US].(l33th 4xor).Test.(22)", "Pokemon Red Test")]
        [TestCase("[PROTOTYPE]™", "[PROTOTYPE]")]
        [TestCase("(PROTOTYPE2)™", "(PROTOTYPE2)")]
        public void NormalizeGameNameTest(string input, string expectedOutput)
        {
            string output = StringExtensions.NormalizeGameName(input);
            Assert.AreEqual(expectedOutput, output);
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

        [TestCase("Final Fantasy XIII-2", "Final Fantasy 13-02")]
        [TestCase("Final Fantasy Ⅻ", "Final Fantasy 12")] //Ⅻ is a single unicode character here
        [TestCase("FINAL FANTASY X/X-2 HD Remaster", "FINAL FANTASY 10/10-02 HD Remaster")]
        [TestCase("Warhammer ↂↇ", "Warhammer 40000")]
        [TestCase("The Jungle Book", "Jungle Book")]
        [TestCase("Carmageddon 2: Carpocalypse Now", "Carmageddon 02: Carpocalypse Now")]
        [TestCase("SOULCALIBUR IV", "SOULCALIBUR 04")]
        [TestCase("Quake III: Team Arena", "Quake 03: Team Arena")]
        [TestCase("THE KING OF FIGHTERS XIV STEAM EDITION", "KING OF FIGHTERS 14 STEAM EDITION")]
        [TestCase("A Hat in Time", "Hat in Time")]
        [TestCase("Battlefield 1942", "Battlefield 1942")]
        [TestCase("Battlefield V", "Battlefield 05")] //with the Battlefield series' numbering, ordering is going to be messed up no matter what, so this is acceptable
        [TestCase("12 is Better Than 6", "12 is Better Than 06")] //edge case, should be acceptable
        [TestCase("Tales of Monkey Island: Chapter 1 - Launch of the Screaming Narwhal", "Tales of Monkey Island: Chapter 01 - Launch of the Screaming Narwhal")]
        [TestCase("KOBOLD: Chapter I", "KOBOLD: Chapter 01")]
        [TestCase("The Witcher 3", "Witcher 03")]
        [TestCase("the Witcher 3", "Witcher 03")]
        [TestCase("A Game", "Game")]
        [TestCase("An Usual Game", "Usual Game")]
        public void ConvertToSortableNameTest(string input, string expected)
        {
            var output = input.ConvertToSortableName();
            Assert.AreEqual(expected, output);
        }

        [TestCase("SHENZHEN I/O")]
        [TestCase("XIII")]
        [TestCase("X: Beyond the Frontier")]
        [TestCase("X3: Terran Conflict")]
        [TestCase("X-COM")]
        [TestCase("Gobliiins")]
        [TestCase("Before I Forget")]
        [TestCase("A.I.M. Racing")]
        [TestCase("S.T.A.L.K.E.R.: Shadow of Chernobyl")]
        [TestCase("AnUsual Game")]
        public void SortableNameIsUnchanged(string input)
        {
            ConvertToSortableNameTest(input, input);
        }

        [TestCase("I", 1)]
        [TestCase("II", 2)]
        [TestCase("IV", 4)]
        [TestCase("IIII", 4)] //odd notation, but should parse
        [TestCase("VIII", 8)]
        [TestCase("IIX", 8)] //odd notation, but should parse
        [TestCase("XIII", 13)]
        [TestCase("XIX", 19)]
        [TestCase("CCLXXXI", 281)]
        [TestCase("MCMLVIII", 1958)]
        [TestCase("MCMXCVIII", 1998)]
        [TestCase("MCMXCIX", 1999)]
        public void ConvertRomanNumeralsToIntTest(string input, int expected)
        {
            int output = SortableNameConverter.ConvertRomanNumeralToInt(input);
            Assert.AreEqual(expected, output);
        }
    }
}
