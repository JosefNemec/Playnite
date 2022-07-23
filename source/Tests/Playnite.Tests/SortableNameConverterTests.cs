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
        //Insignificant side-effects (harder to fix without making exceptions, and not damaging for sorting with other games in their franchise):

        //Back 4 Blood                                      ->      Back 04 Blood
        //Left 4 Dead                                       ->      Left 04 Dead
        //Spirit Swap: Lofi Beats to Match-3 To             ->      Spirit Swap: Lofi Beats to Match-03 To
        //Kingdom Hearts 358/2 Days                         ->      Kingdom Hearts 358/02 Days
        //Ether One                                         ->      Ether 01
        //It Takes Two                                      ->      It Takes 02
        //Army of Two	                                    ->      Army of 02
        //                                                          Army of Two: The Devil's Cartel won't be changed, but this will still preserve release order sorting
        //Hyperdimension Neptunia Re;Birth3 V Generation	->      Hyperdimension Neptunia Re;Birth3 05 Generation
        //STAR WARS: Rebel Assault I + II                   ->      STAR WARS: Rebel Assault I + 02
        //Emily is Away <3	                                ->      Emily is Away <03

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
        [TestCase("The Elder Scrolls II: Daggerfall Unity - GOG Cut", "Elder Scrolls 02: Daggerfall Unity - GOG Cut")]
        [TestCase("Metal Slug XX", "Metal Slug 20")]
        [TestCase("The Uncanny X-Men", "Uncanny X-Men")]
        [TestCase("Test X-", "Test 10-")]
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
        [TestCase("Daemon X Machina")]
        [TestCase("Bit Blaster XL")]
        [TestCase("STAR WARS X-Wing vs TIE Fighter: Balance of Power Campaigns")]
        [TestCase("Star Wars: X-Wing Alliance")]
        [TestCase("Acceleration of Suguri X-Edition")]
        [TestCase("Guilty Gear X2 #Reload")]
        [TestCase("Mega Man X Legacy Collection")] //Mega Man 10 is a different game
        [TestCase("LEGO DC Super-Villains")]
        [TestCase("Constant C")]
        [TestCase("Metroid: Other M")]
        [TestCase("Zero Escape: Zero Time Dilemma")] //zero isn't currently parsed but if it ever is, this title should remain unchanged
        [TestCase("Worms W M D")]
        [TestCase("Sonic Adventure DX")]
        [TestCase("Zone of The Enders: The 2nd Runner M∀RS")]
        [TestCase("AnUsual Game")]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void SortableNameIsUnchanged(string input)
        {
            var c = new SortableNameConverter(new PlayniteSettings().GameSortingNameRemovedArticles);
            var output = c.Convert(input);
            Assert.AreEqual(input, output);
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
        [TestCase("asdf")]
        public void ConvertRomanNumeralsToIntRejectsNonsense(string input)
        {
            int? output = SortableNameConverter.ConvertRomanNumeralToInt(input);
            Assert.AreEqual(null, output);
        }

    }
}
