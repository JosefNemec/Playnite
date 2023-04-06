namespace Playnite.Tests;

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

    [Test]
    public void ConvertToSortableNameTest()
    {
        var c = new SortableNameConverter(PlayniteSettings.DefaultSortingNameRemovedArticles);
        var testList = new Dictionary<string, string>
        {
            ["Final Fantasy XIII-2"] = "Final Fantasy 13-02",
            ["Final Fantasy Ⅻ"] = "Final Fantasy 12", //Ⅻ is a single unicode character here
            ["FINAL FANTASY X/X-2 HD Remaster"] = "FINAL FANTASY 10/10-02 HD Remaster",
            ["Warhammer ↂↇ"] = "Warhammer 40000",
            ["Carmageddon 2: Carpocalypse Now"] = "Carmageddon 02: Carpocalypse Now",
            ["SOULCALIBUR IV"] = "SOULCALIBUR 04",
            ["Quake III: Team Arena"] = "Quake 03: Team Arena",
            ["THE KING OF FIGHTERS XIV STEAM EDITION"] = "KING OF FIGHTERS 14 STEAM EDITION",
            ["A Hat in Time"] = "Hat in Time",
            ["Battlefield V"] = "Battlefield 05",
            ["Tales of Monkey Island: Chapter 1 - Launch of the Screaming Narwhal"] = "Tales of Monkey Island: Chapter 01 - Launch of the Screaming Narwhal",
            ["Tales of Monkey Island: Chapter I - Launch of the Screaming Narwhal"] = "Tales of Monkey Island: Chapter 01 - Launch of the Screaming Narwhal",
            ["KOBOLD: Chapter I"] = "KOBOLD: Chapter 01",
            ["Crazy Machines 1.5 New from the Lab"] = "Crazy Machines 01.5 New from the Lab",
            ["Half-Life 2: Episode One"] = "Half-Life 02: Episode 01",
            ["Unravel Two"] = "Unravel 02",
            ["The Elder Scrolls II: Daggerfall Unity - GOG Cut"] = "Elder Scrolls 02: Daggerfall Unity - GOG Cut",
            ["Metal Slug XX"] = "Metal Slug 20",
            ["The Uncanny X-Men"] = "Uncanny X-Men",
            ["Test X-"] = "Test 10-",
            ["The Witcher 3"] = "Witcher 03",
            ["the Witcher 3"] = "Witcher 03",
            ["A Game"] = "Game",
            ["An Usual Game"] = "Usual Game",
        };

        testList.Keys.ForEach(a => Assert.AreEqual(testList[a], c.Convert(a)));
    }

    [Test]
    public void SortableNameIsUnchanged()
    {
        var c = new SortableNameConverter(PlayniteSettings.DefaultSortingNameRemovedArticles);
        var testList = new List<string?>
        {
            "SHENZHEN I/O",
            "XIII",
            "X: Beyond the Frontier",
            "X3: Terran Conflict",
            "X-COM",
            "Gobliiins",
            "Before I Forget",
            "A.I.M. Racing",
            "S.T.A.L.K.E.R.: Shadow of Chernobyl",
            "Battlefield 1942",
            "Metal Wolf Chaos XD",
            "Prince of Persia: The Two Thrones",
            "Daemon X Machina",
            "Bit Blaster XL",
            "STAR WARS X-Wing vs TIE Fighter: Balance of Power Campaigns",
            "Star Wars: X-Wing Alliance",
            "Acceleration of Suguri X-Edition",
            "Guilty Gear X2 #Reload",
            "Mega Man X Legacy Collection", //Mega Man 10 is a different game
            "LEGO DC Super-Villains",
            "Constant C",
            "Metroid: Other M",
            "Zero Escape: Zero Time Dilemma", //zero isn't currently parsed but if it ever is, this title should remain unchanged
            "Worms W M D",
            "Sonic Adventure DX",
            "Zone of The Enders: The 2nd Runner M∀RS",
            "AnUsual Game",
            null,
            "",
            "  "
        };

        testList.ForEach(a => Assert.AreEqual(a, c.Convert(a!)));
    }

    [Test]
    public void SortableNameNoArticlesRemovedTest()
    {
        var c = new SortableNameConverter(new string[0]);
        Assert.AreEqual("The Witcher 03", c.Convert("The Witcher 3"));
        Assert.AreEqual("A Hat in Time", c.Convert("A Hat in Time"));
    }

    [Test]
    public void ConvertRomanNumeralsToIntTest()
    {
        var testList = new Dictionary<string, int>
        {
            ["I"] = 1,
            ["II"] = 2,
            ["IV"] = 4,
            ["VIII"] = 8,
            ["IX"] = 9,
            ["XIII"] = 13,
            ["XIX"] = 19,
            ["CCLXXXI"] = 281,
            ["MCMLVIII"] = 1958,
            ["LMMXXIV"] = 1974,
            ["MLMXXIV"] = 1974,
            ["MCMXCVIII"] = 1998,
            ["MCMXCIX"] = 1999,
        };

        testList.Keys.ForEach(a => Assert.AreEqual(testList[a], SortableNameConverter.ConvertRomanNumeralToInt(a)));
    }

    [Test]
    public void ConvertRomanNumeralsToIntRejectsNonsense()
    {
        var testList = new List<string>
        {
            "IVX",
            "VIX",
            "IIII",
            "XXL",
            "IIX",
            "asdf"
        };

        testList.ForEach(a => Assert.IsNull(SortableNameConverter.ConvertRomanNumeralToInt(a)));
    }
}
