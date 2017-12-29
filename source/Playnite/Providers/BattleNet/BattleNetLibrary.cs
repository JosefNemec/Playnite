using AngleSharp.Parser.Html;
using Playnite.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Playnite.Providers.BattleNet
{
    public class BattleNetLibrary : IBattleNetLibrary
    {
        public enum BNetAppType
        {
            Default,
            Classic
        }

        public class BNetApp
        {
            public string ProductId;
            public string InternalId;
            public string WebLibraryId;
            public string PurchaseId;
            public string IconUrl;
            public string BackgroundUrl;
            public string CoverUrl;
            public string Name;
            public BNetAppType Type;
            public string ClassicExecutable;
            public List<Link> Links;
        }

        public static readonly List<BNetApp> BattleNetProducts = new List<BNetApp>()
        {
            new BNetApp()
            {
                ProductId = "WoW",
                InternalId = "wow",
                WebLibraryId = "game-list-wow",
                PurchaseId = "wowc-starter-link",
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
                ProductId = "D3",
                InternalId = "diablo3",
                WebLibraryId = "game-list-d3",
                PurchaseId = "d3-starter-link",
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
                ProductId = "S2",
                InternalId = "s2",
                WebLibraryId = "game-list-s2",
                PurchaseId = "s2-starter-link",
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
                ProductId = "S1",
                InternalId = "s1",
                WebLibraryId = "game-list-sc",
                PurchaseId = "",
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
                ProductId = "WTCG",
                InternalId = "hs_beta",
                WebLibraryId = "game-list-hearthstone",
                PurchaseId = "",
                IconUrl = @"https://blznav.akamaized.net/img/games/logo-hs-beb1a37bc84beefb.png",
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
                ProductId = "Hero",
                InternalId = "heroes",
                WebLibraryId = "game-list-bas",
                PurchaseId = "",
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
                ProductId = "Pro",
                InternalId = "prometheus",
                WebLibraryId = "game-list-overwatch",
                PurchaseId = "overwatch-purchase-link",
                IconUrl = @"https://blznav.akamaized.net/img/games/logo-ow-1dd54d69712651a9.png",
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
                ProductId = "DST2",
                InternalId = "destiny2",
                WebLibraryId = "game-list-destiny2",
                PurchaseId = "destiny2-purchase-link",
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
                WebLibraryId = "D2DV-se",
                PurchaseId = "",
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
                WebLibraryId = "D2XP-se",
                PurchaseId = "",
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
                WebLibraryId = "WAR3-se",
                PurchaseId = "",
                IconUrl = @"https://bneteu-a.akamaihd.net/account/static/local-common/images/game-icons/war3-32.1N2FK.png",
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
                WebLibraryId = "W3XP-se",
                PurchaseId = "",
                IconUrl = @"https://bneteu-a.akamaihd.net/account/static/local-common/images/game-icons/w3xp-32.15Wgr.png",
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
            }
        };

        public static BNetApp GetAppDefinition(string productId)
        {
            return BattleNetProducts.First(a => a.ProductId == productId);
        }

        public static UninstallProgram GetUninstallEntry(BNetApp app)
        {
            foreach (var prog in Programs.GetUnistallProgramsList())
            {
                if (app.Type == BNetAppType.Classic)
                {
                    if (prog.DisplayName == app.InternalId)
                    {
                        return prog;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(prog.UninstallString))
                    {
                        continue;
                    }

                    var match = Regex.Match(prog.UninstallString, string.Format(@"Battle\.net.*--uid={0}.*\s", app.InternalId));
                    if (match.Success)
                    {
                        return prog;
                    }
                }                     
            }

            return null;
        }

        public GameTask GetGamePlayTask(string id)
        {
            return new GameTask()
            {
                Type = GameTaskType.URL,
                Path = $"battlenet://{id}/",
                IsPrimary = true,
                IsBuiltIn = true
            };
        }

        public List<IGame> GetInstalledGames()
        {
            var games = new List<IGame>();
            foreach (var prog in Programs.GetUnistallProgramsList())
            {
                if (string.IsNullOrEmpty(prog.UninstallString))
                {
                    continue;
                }

                if (prog.Publisher == "Blizzard Entertainment" && BattleNetProducts.Any(a => a.Type == BNetAppType.Classic && prog.DisplayName == a.InternalId))
                {
                    var products = BattleNetProducts.Where(a => a.Type == BNetAppType.Classic && prog.DisplayName == a.InternalId);
                    foreach (var product in products)
                    {
                        var game = new Game()
                        {
                            Provider = Provider.BattleNet,
                            ProviderId = product.ProductId,
                            Name = product.Name,
                            PlayTask = new GameTask()
                            {
                                Type = GameTaskType.File,
                                WorkingDir = @"{InstallDir}",
                                Path = @"{InstallDir}\" + product.ClassicExecutable
                            },
                            InstallDirectory = prog.InstallLocation
                        };

                        games.Add(game);
                    }
                }
                else
                {
                    var match = Regex.Match(prog.UninstallString, @"Battle\.net.*--uid=(.*?)\s");
                    if (!match.Success)
                    {
                        continue;
                    }

                    var iId = match.Groups[1].Value;
                    var product = BattleNetProducts.FirstOrDefault(a => a.Type == BNetAppType.Default && iId.StartsWith(a.InternalId));
                    if (product == null)
                    {
                        continue;
                    }

                    var game = new Game()
                    {
                        Provider = Provider.BattleNet,
                        ProviderId = product.ProductId,
                        Name = product.Name,
                        PlayTask = GetGamePlayTask(product.ProductId),
                        InstallDirectory = prog.InstallLocation
                    };

                    games.Add(game);
                }                
            }

            return games;
        }

        public GameMetadata UpdateGameWithMetadata(IGame game)
        {
            var metadata = new GameMetadata();
            var product = BattleNetProducts.FirstOrDefault(a => a.ProductId == game.ProviderId);
            if (product == null)
            {
                return metadata;
            }

            if (string.IsNullOrEmpty(product.IconUrl))
            {
                return metadata;
            }

            game.Name = product.Name;
            var icon = Web.DownloadData(product.IconUrl);
            var iconFile = Path.GetFileName(product.IconUrl);
            metadata.Icon = new Database.FileDefinition($"images/battlenet/{game.ProviderId}/{iconFile}", iconFile, icon);
            var cover = Web.DownloadData(product.CoverUrl);
            var coverFile = Path.GetFileName(product.CoverUrl);
            metadata.Image = new Database.FileDefinition($"images/battlenet/{game.ProviderId}/{coverFile}", coverFile, cover);
            game.BackgroundImage = product.BackgroundUrl;
            metadata.BackgroundImage = product.BackgroundUrl;
            game.Links = new ObservableCollection<Link>(product.Links);
            return metadata;
        }

        public List<IGame> GetLibraryGames()
        {
            using (var api = new WebApiClient())
            {
                if (api.GetLoginRequired())
                {
                    throw new Exception("User is not logged in.");
                }

                var page = api.GetOwnedGames();
                var games = new List<IGame>();
                var parser = new HtmlParser();
                var document = parser.Parse(page);

                foreach (var product in BattleNetProducts)
                {
                    if (product.Type == BNetAppType.Default)
                    {
                        var documentProduct = document.QuerySelector($"#{product.WebLibraryId}");
                        if (documentProduct == null)
                        {
                            continue;
                        }

                        if (!string.IsNullOrEmpty(product.PurchaseId))
                        {
                            var saleOffer = documentProduct.QuerySelector($"#{product.PurchaseId}");
                            if (saleOffer != null)
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        var documentProduct = document.QuerySelector($".{product.WebLibraryId}");
                        if (documentProduct == null)
                        {
                            continue;
                        }
                    }

                    var game = new Game()
                    {
                        Provider = Provider.BattleNet,
                        ProviderId = product.ProductId,
                        Name = product.Name
                    };

                    games.Add(game);
                }

                return games;
            }
        }
    }
}
