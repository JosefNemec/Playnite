using BattleNetLibrary.Models;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleNetLibrary
{
    public class BattleNetGames
    {
        public static readonly List<BNetApp> Games = new List<BNetApp>()
        {
            new BNetApp()
            {
                ApiId = 5730135,
                ProductId = "WoW",
                InternalId = "wow",
                IconUrl = @"https://blznav.akamaized.net/img/games/logo-wow-3dd2cfe06df74407.png",
                BackgroundUrl = @"https://bnetproduct-a.akamaihd.net//fe4/e09d3a01538f92686e2d7e30dc89ee1e-prod-mobile-bg.jpg",
                CoverUrl = "http://bnetproduct-a.akamaihd.net//fab/a25ed0ddd3225929bc3ad5139ebc7483-prod-card-tall.jpg",
                Name = "World of Warcraft",
                Type = BNetAppType.Default,
                Links = new List<Link>()
                {
                    new Link("Homepage", "https://worldofwarcraft.com/"),
                    new Link("Forums", "https://battle.net/forums/en/wow/")
                }
            },
            new BNetApp()
            {
                ApiId = 17459,
                ProductId = "D3",
                InternalId = "diablo3",
                IconUrl = @"https://blznav.akamaized.net/img/games/logo-d3-ab08e4045fed09ee.png",
                BackgroundUrl = @"https://bnetproduct-a.akamaihd.net//fad/6a06a79f8b1134a80d794dc24c9cd2d1-prod-mobile-bg.jpg",
                CoverUrl = "http://bnetproduct-a.akamaihd.net//fbd/bafaafcfb7c6c620067662a04409ba66-prod-card-tall.jpg",
                Name = "Diablo III",
                Type = BNetAppType.Default,
                Links = new List<Link>()
                {
                    new Link("Homepage", "http://www.diablo3.com"),
                    new Link("Forums", "https://battle.net/forums/en/d3/")
                }
            },
            new BNetApp()
            {
                ApiId = 21298,
                ProductId = "S2",
                InternalId = "s2",
                IconUrl = @"https://blznav.akamaized.net/img/games/logo-sc2-6e33583ba0547b6a.png",
                BackgroundUrl = @"https://bnetproduct-a.akamaihd.net//fcd/ab0419d498190f5f2ccf69414265b70b-prod-mobile-bg.jpg",
                CoverUrl = "http://bnetproduct-a.akamaihd.net//fd8/18fb5862b6d5aea418ad4102ed48aa63-prod-card-tall.jpg",
                Name = "StarCraft II",
                Type = BNetAppType.Default,
                Links = new List<Link>()
                {
                    new Link("Homepage", "https://www.starcraft2.com"),
                    new Link("Forums", "https://battle.net/forums/en/sc2/")
                }
            },
            new BNetApp()
            {
                ApiId = 21297,
                ProductId = "S1",
                InternalId = "s1",
                IconUrl = @"https://blznav.akamaized.net/img/games/logo-scr-fef4f892c20f584c.png",
                BackgroundUrl = @"https://bnetproduct-a.akamaihd.net//fb2/eb1b3feb5cc03da2d05f3e9e88aaec2a-prod-mobile-bg.jpg",
                CoverUrl = "http://bnetproduct-a.akamaihd.net//f95/6d9453be1750dbf035f0ee574cff2c25-prod-card-tall.jpg",
                Name = "StarCraft",
                Type = BNetAppType.Default,
                Links = new List<Link>()
                {
                    new Link("Homepage", "https://starcraft.com"),
                    new Link("Forums", "https://us.battle.net/forums/en/starcraft/")
                }
            },
            new BNetApp()
            {
                ApiId = 1465140039,
                ProductId = "WTCG",
                InternalId = "hs_beta",
                IconUrl = @"https://blznav.akamaized.net/img/games/logo-hs-93512467e87f82c6.png",
                BackgroundUrl = @"https://bnetproduct-a.akamaihd.net//fac/895ca992a21d9c960bd30f9738d7bfb8-prod-mobile-bg.jpg",
                CoverUrl = "http://bnetproduct-a.akamaihd.net//f89/c074270c5024a5bb627d46cddf024dad-prod-card-tall.jpg",
                Name = "Hearthstone",
                Type = BNetAppType.Default,
                Links = new List<Link>()
                {
                    new Link("Homepage", "https://playhearthstone.com"),
                    new Link("Forums", "https://battle.net/forums/en/hearthstone/")
                }
            },
            new BNetApp()
            {
                ApiId = 1214607983,
                ProductId = "Hero",
                InternalId = "heroes",
                IconUrl = @"https://blznav.akamaized.net/img/games/logo-heroes-78cae505b7a524fb.png",
                BackgroundUrl = @"https://bnetproduct-a.akamaihd.net//f88/9eaac80f3496502843198b092eb35b84-prod-mobile-bg.jpg",
                CoverUrl = "http://bnetproduct-a.akamaihd.net//f8c/0f2efeb8d64127edb647a95c236c92ba-prod-card-tall.jpg",
                Name = "Heroes of the Storm",
                Type = BNetAppType.Default,
                Links = new List<Link>()
                {
                    new Link("Homepage", "http://www.heroesofthestorm.com"),
                    new Link("Forums", "https://battle.net/forums/en/heroes/")
                }
            },
            new BNetApp()
            {
                ApiId = 5272175,
                ProductId = "Pro",
                InternalId = "prometheus",
                IconUrl = @"https://blznav.akamaized.net/img/games/logo-ow-4be5755bc0a4cbaf.png",
                BackgroundUrl = @"https://bnetproduct-a.akamaihd.net//fc3/e21df4ac2fd75cd9884a55744a1786c3-prod-mobile-bg.jpg",
                CoverUrl = "http://bnetproduct-a.akamaihd.net//4c/c358d897f1348281ed0b21ea2027059b-prod-card-tall.jpg",
                Name = "Overwatch",
                Type = BNetAppType.Default,
                Links = new List<Link>()
                {
                    new Link("Homepage", "https://playoverwatch.com"),
                    new Link("Forums", "https://battle.net/forums/en/overwatch/")
                }
            },
            new BNetApp()
            {
                ApiId = 1146311730,
                ProductId = "DST2",
                InternalId = "destiny2",
                IconUrl = @"https://blznav.akamaized.net/img/games/logo-dest2-933dcf397eb647e0.png",
                BackgroundUrl = @"https://bnetproduct-a.akamaihd.net//fbd/22512bcb91e4a3b3d9ee208be2ee3beb-prod-mobile-bg.jpg",
                CoverUrl = "http://bnetproduct-a.akamaihd.net//f84/7d453e354c9df8ca335ad45da020704c-prod-card-tall.jpg",
                Name = "Destiny 2",
                Type = BNetAppType.Default,
                Links = new List<Link>()
                {
                    new Link("Homepage", "https://www.destinythegame.com/"),
                    new Link("Forums", "https://www.bungie.net/en/Forums/Topics?pNumber=0&tg=Destiny2")
                }
            },
            new BNetApp()
            {
                ProductId = "D2",
                InternalId = "Diablo II",
                IconUrl = @"https://bneteu-a.akamaihd.net/account/static/local-common/images/game-icons/d2dv-32.4PqK2.png",
                BackgroundUrl = @"https://bnetproduct-a.akamaihd.net//70/23fd57c691805861a899eabaa12f39f5-prod-mobile-bg.jpg",
                CoverUrl = "http://bnetproduct-a.akamaihd.net//7f/a31777e05911989e7839ea02435c9eb5-prod-card-tall.jpg",
                Name = "Diablo II",
                Type = BNetAppType.Classic,
                ClassicExecutable = "Diablo II.exe",
                Links = new List<Link>()
                {
                    new Link("Homepage", "http://blizzard.com/games/d2/"),
                    new Link("Forums", "https://us.battle.net/forums/en/bnet/12790218/")
                }
            },
            new BNetApp()
            {
                ProductId = "D2X",
                InternalId = "Diablo II",
                IconUrl = @"https://bneteu-a.akamaihd.net/account/static/local-common/images/game-icons/d2xp.1gR7W.png",
                BackgroundUrl = @"https://bnetproduct-a.akamaihd.net//f9a/3935e198b09577d63a394ee195ddec2e-prod-mobile-bg.jpg",
                CoverUrl = "http://bnetproduct-a.akamaihd.net//4/cb3a7d551cb3524c5a8c68abacd4fda9-prod-card-tall.jpg",
                Name = "Diablo II: Lord of Destruction",
                Type = BNetAppType.Classic,
                ClassicExecutable = "Diablo II.exe",
                Links = new List<Link>()
                {
                    new Link("Homepage", "http://blizzard.com/games/d2/"),
                    new Link("Forums", "https://us.battle.net/forums/en/bnet/12790218/")
                }
            },
            new BNetApp()
            {
                ProductId = "W3",
                InternalId = "Warcraft III",
                IconUrl = @"https://blznav.akamaized.net/img/games/logo-w3r-c8a76eea272dbd55.png",
                BackgroundUrl = @"https://bnetproduct-a.akamaihd.net//11/b924dd7257d4728f314822837d9a5e68-prod-mobile-bg.jpg",
                CoverUrl = "http://bnetproduct-a.akamaihd.net//42/a4e5b0ccd23d09ad34e7c0a074bb4c11-prod-card-tall.jpg",
                Name = "Warcraft III: Reign of Chaos",
                Type = BNetAppType.Classic,
                ClassicExecutable = "Warcraft III Launcher.exe",
                Links = new List<Link>()
                {
                    new Link("Homepage", "http://blizzard.com/games/war3/"),
                    new Link("Forums", "https://us.battle.net/forums/en/bnet/12790218/")
                }
            },
            new BNetApp()
            {
                ProductId = "W3X",
                InternalId = "Warcraft III",
                IconUrl = @"https://blznav.akamaized.net/img/games/logo-w3r-c8a76eea272dbd55.png",
                BackgroundUrl = @"https://bnetproduct-a.akamaihd.net//7/f79aee74f037d9c3a44736ecccc4373a-prod-mobile-bg.jpg",
                CoverUrl = "http://bnetproduct-a.akamaihd.net//fd9/a4b9e92295e20508bb62a0756577e925-prod-card-tall.jpg",
                Name = "Warcraft III: The Frozen Throne",
                Type = BNetAppType.Classic,
                ClassicExecutable = "Warcraft III Launcher.exe",
                Links = new List<Link>()
                {
                    new Link("Homepage", "http://blizzard.com/games/war3/"),
                    new Link("Forums", "https://us.battle.net/forums/en/bnet/12790218/")
                }
            },
            new BNetApp()
            {
                ApiId = 1447645266,
                ProductId = "VIPR",
                InternalId = "viper",
                IconUrl = @"https://blznav.akamaized.net/img/games/logo-codbo4-7794ee86f3e8be3e.png",
                BackgroundUrl = @"https://bnetproduct-a.akamaihd.net//5d/411c53766cdf6155fcc952f79f304b4a-prod-mobile-bg.jpg",
                CoverUrl = "http://bnetproduct-a.akamaihd.net//62/a346ee691a8d0829c5a895200dd17cbf-prod-card-tall-v2.jpg",
                Name = "Call of Duty: Black Ops 4",
                Type = BNetAppType.Default,
                Links = new List<Link>()
                {
                    new Link("Homepage", "https://www.callofduty.com/")
                }
            },
            new BNetApp()
            {
                ApiId = 1329875278,
                ProductId = "ODIN",
                InternalId = "odin",
                IconUrl = @"https://blznav.akamaized.net/img/games/logo-codmw-d57b296321d6b444.png",
                BackgroundUrl = @"https://bnetproduct-a.akamaihd.net//59/326ed260bc958ddd26713761683a4489-_Kronos-Bnet_Game-Shop_Background_Desktop-2280x910.jpg",
                CoverUrl = "https://bnetproduct-a.akamaihd.net//5e/294eb830c6db1959b3db3b4cbbcfe7fc-_Kronos-Bnet_Game-Card_Product_Vert-700x850.jpg",
                Name = "Call of Duty: Modern Warfare",
                Type = BNetAppType.Default,
                Links = new List<Link>()
                {
                    new Link("Homepage", "https://www.callofduty.com/")
                }
            },
            new BNetApp()
            {
                ApiId = 22323,
                ProductId = "W3",
                InternalId = "w3",
                IconUrl = @"https://blznav.akamaized.net/img/games/logo-w3r-c8a76eea272dbd55.png",
                BackgroundUrl = @"https://bnetproduct-a.akamaihd.net//faf/44004fe111706bac3ad1c9a5c7264d1f-WC3R_2020_Orc_Art_Shop_Product_Page_Assets_prod-full-bg_TS03.jpg",
                CoverUrl = "https://bnetproduct-a.akamaihd.net//5f/3d885e4077747a04a646186a17607769-WC3R_2020_Orc_Art_Shop_Product_Page_Assets_prod-card-vert_TS03.jpg",
                Name = "Warcraft III: Reforged",
                Type = BNetAppType.Default,
                Links = new List<Link>()
                {
                    new Link("Homepage", "https://playwarcraft3.com/"),
                    new Link("Forums", "https://us.forums.blizzard.com/en/warcraft3/")
                }
            },
            new BNetApp()
            {
                ApiId = 1279351378,
                ProductId = "LAZR",
                InternalId = "lazarus",
                IconUrl = @"https://blznav.akamaized.net/img/games/logo-codmw2cr-403ff7094aa97396.png",
                BackgroundUrl = @"https://bnetproduct-a.akamaihd.net//f90/ca5641d89e0495fcd468350d298097d5-Lazarus-Bnet_Placeholder-Shop_Background_Desktop-2280x910-For_20200331.jpg",
                CoverUrl = "https://bnetproduct-a.akamaihd.net//f/7e875975619de0671dc538c8d85ba550-Lazarus-Bnet_Placeholder-Card_Product_Vert-700x850-For_2020406-Corrected.jpg",
                Name = "Call of Duty: Modern Warfare 2 Campaign Remastered",
                Type = BNetAppType.Default,
                Links = new List<Link>()
                {
                    new Link("Homepage", "https://www.callofduty.com/mw2campaignremastered")
                }
            }
        };

        public static BNetApp GetAppDefinition(string productId)
        {
            return Games.FirstOrDefault(a => a.ProductId == productId);
        }
    }
}
