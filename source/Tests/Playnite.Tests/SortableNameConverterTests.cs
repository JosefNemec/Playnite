using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    [TestFixture]
    public class SortableNameConverterTests
    {
        [TestCase("Final Fantasy XIII-2", "Final Fantasy 13-02")]
        [TestCase("Final Fantasy Ⅻ", "Final Fantasy 12")] //Ⅻ is a single unicode character here
        [TestCase("FINAL FANTASY X/X-2 HD Remaster", "FINAL FANTASY 10/10-02 HD Remaster")]
        [TestCase("Warhammer ↂↇ", "Warhammer 40000")]
        [TestCase("Carmageddon 2: Carpocalypse Now", "Carmageddon 02: Carpocalypse Now")]
        [TestCase("SOULCALIBUR IV", "SOULCALIBUR 04")]
        [TestCase("Quake III: Team Arena", "Quake 03: Team Arena")]
        [TestCase("THE KING OF FIGHTERS XIV STEAM EDITION", "KING OF FIGHTERS 14 STEAM EDITION")]
        [TestCase("A Hat in Time", "Hat in Time")]
        [TestCase("Battlefield V", "Battlefield 05")]
        [TestCase("Tales of Monkey Island: Chapter 1 - Launch of the Screaming Narwhal", "Tales of Monkey Island: Chapter 01 - Launch of the Screaming Narwhal")]
        [TestCase("Tales of Monkey Island: Chapter I - Launch of the Screaming Narwhal", "Tales of Monkey Island: Chapter 01 - Launch of the Screaming Narwhal")]
        [TestCase("KOBOLD: Chapter I", "KOBOLD: Chapter 01")]
        [TestCase("Crazy Machines 1.5 New from the Lab", "Crazy Machines 01.5 New from the Lab")]
        [TestCase("Half-Life 2: Episode One", "Half-Life 02: Episode 01")]
        [TestCase("Unravel Two", "Unravel 02")]
        [TestCase("The Witcher 3", "Witcher 03")]
        [TestCase("the Witcher 3", "Witcher 03")]
        [TestCase("A Game", "Game")]
        [TestCase("An Usual Game", "Usual Game")]
        public void ConvertToSortableNameTest(string input, string expected)
        {
            var c = new SortableNameConverter(new PlayniteSettings().GameSortingNameRemovedArticles);
            var output = c.Convert(input);
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
        [TestCase("Battlefield 1942")]
        [TestCase("Metal Wolf Chaos XD")]
        [TestCase("Prince of Persia: The Two Thrones")]
        [TestCase("AnUsual Game")]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void SortableNameIsUnchanged(string input)
        {
            ConvertToSortableNameTest(input, input);
        }

        [TestCase("The Witcher 3", "The Witcher 03")]
        [TestCase("A Hat in Time", "A Hat in Time")]
        public void SortableNameNoArticlesRemovedTest(string input, string expected)
        {
            var c = new SortableNameConverter(new string[0]);
            var output = c.Convert(input);
            Assert.AreEqual(expected, output);
        }

        [TestCase("I", 1)]
        [TestCase("II", 2)]
        [TestCase("IV", 4)]
        [TestCase("VIII", 8)]
        [TestCase("IX", 9)]
        [TestCase("XIII", 13)]
        [TestCase("XIX", 19)]
        [TestCase("CCLXXXI", 281)]
        [TestCase("MCMLVIII", 1958)]
        [TestCase("LMMXXIV", 1974)]
        [TestCase("MLMXXIV", 1974)]
        [TestCase("MCMXCVIII", 1998)]
        [TestCase("MCMXCIX", 1999)]
        public void ConvertRomanNumeralsToIntTest(string input, int expected)
        {
            int? output = SortableNameConverter.ConvertRomanNumeralToInt(input);
            Assert.AreEqual(expected, output);
        }

        [TestCase("IVX")]
        [TestCase("VIX")]
        [TestCase("IIII")]
        [TestCase("XXL")]
        [TestCase("IIX")]
        public void ConvertRomanNumeralsToIntRejectsNonsense(string input)
        {
            int? output = SortableNameConverter.ConvertRomanNumeralToInt(input);
            Assert.AreEqual(null, output);
        }

    }
}
