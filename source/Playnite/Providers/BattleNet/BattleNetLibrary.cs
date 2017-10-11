using AngleSharp.Parser.Html;
using Playnite.Models;
using System;
using System.Collections.Generic;
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
            public string Name;
            public BNetAppType Type;
            public string ClassicExecutable;
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
                Name = "World of Warcraft",
                Type = BNetAppType.Default
            },
            new BNetApp()
            {
                ProductId = "D3",
                InternalId = "diablo3",
                WebLibraryId = "game-list-d3",
                PurchaseId = "d3-starter-link",
                IconUrl = @"https://blznav.akamaized.net/img/games/logo-d3-ab08e4045fed09ee.png",
                Name = "Diablo III",
                Type = BNetAppType.Default
            },
            new BNetApp()
            {
                ProductId = "S2",
                InternalId = "s2",
                WebLibraryId = "game-list-s2",
                PurchaseId = "s2-starter-link",
                IconUrl = @"https://blznav.akamaized.net/img/games/logo-sc2-6e33583ba0547b6a.png",
                Name = "StarCraft II",
                Type = BNetAppType.Default
            },
            new BNetApp()
            {
                ProductId = "S1",
                InternalId = "s1",
                WebLibraryId = "game-list-sc",
                PurchaseId = "",
                IconUrl = @"https://blznav.akamaized.net/img/games/logo-scr-fef4f892c20f584c.png",
                Name = "StarCraft",
                Type = BNetAppType.Default
            },
            new BNetApp()
            {
                ProductId = "WTCG",
                InternalId = "hs_beta",
                WebLibraryId = "game-list-hearthstone",
                PurchaseId = "",
                IconUrl = @"https://blznav.akamaized.net/img/games/logo-hs-beb1a37bc84beefb.png",
                Name = "Hearthstone",
                Type = BNetAppType.Default
            },
            new BNetApp()
            {
                ProductId = "Hero",
                InternalId = "heroes",
                WebLibraryId = "game-list-bas",
                PurchaseId = "",
                IconUrl = @"https://blznav.akamaized.net/img/games/logo-heroes-78cae505b7a524fb.png",
                Name = "Heroes of the Storm",
                Type = BNetAppType.Default
            },
            new BNetApp()
            {
                ProductId = "Pro",
                InternalId = "prometheus",
                WebLibraryId = "game-list-overwatch",
                PurchaseId = "overwatch-purchase-link",
                IconUrl = @"https://blznav.akamaized.net/img/games/logo-ow-1dd54d69712651a9.png",
                Name = "Overwatch",
                Type = BNetAppType.Default
            },
            new BNetApp()
            {
                ProductId = "DST2",
                InternalId = "destiny2",
                WebLibraryId = "game-list-destiny2",
                PurchaseId = "destiny2-presale-link",
                IconUrl = @"https://blznav.akamaized.net/img/games/logo-dest2-933dcf397eb647e0.png",
                Name = "Destiny 2",
                Type = BNetAppType.Default
            },
            new BNetApp()
            {
                ProductId = "D2",
                InternalId = "Diablo II",
                WebLibraryId = "D2DV-se",
                PurchaseId = "",
                IconUrl = @"https://bneteu-a.akamaihd.net/account/static/local-common/images/game-icons/d2dv-32.4PqK2.png",
                Name = "Diablo II",
                Type = BNetAppType.Classic,
                ClassicExecutable = "Diablo II.exe"
            },
            new BNetApp()
            {
                ProductId = "D2X",
                InternalId = "Diablo II",
                WebLibraryId = "D2XP-se",
                PurchaseId = "",
                IconUrl = @"https://bneteu-a.akamaihd.net/account/static/local-common/images/game-icons/d2xp.1gR7W.png",
                Name = "Diablo II: Lord of Destruction",
                Type = BNetAppType.Classic,
                ClassicExecutable = "Diablo II.exe"
            },
            new BNetApp()
            {
                ProductId = "W3",
                InternalId = "Warcraft III",
                WebLibraryId = "WAR3-se",
                PurchaseId = "",
                IconUrl = @"https://bneteu-a.akamaihd.net/account/static/local-common/images/game-icons/war3-32.1N2FK.png",
                Name = "Warcraft III: Reign of Chaos",
                Type = BNetAppType.Classic,
                ClassicExecutable = "Warcraft III Launcher.exe"
            },
            new BNetApp()
            {
                ProductId = "W3X",
                InternalId = "Warcraft III",
                WebLibraryId = "W3XP-se",
                PurchaseId = "",
                IconUrl = @"https://bneteu-a.akamaihd.net/account/static/local-common/images/game-icons/w3xp-32.15Wgr.png",
                Name = "Warcraft III: The Frozen Throne",
                Type = BNetAppType.Classic,
                ClassicExecutable = "Warcraft III Launcher.exe"
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

            var icon = Web.DownloadData(product.IconUrl);            
            var iconFile = Path.GetFileName(product.IconUrl);
            metadata.Icon = new Database.FileDefinition($"images/battlenet/{game.ProviderId}/{iconFile}", iconFile, icon);

            game.IsProviderDataUpdated = true;
            return metadata;
        }

        public List<IGame> GetLibraryGames()
        {            
            var api = new WebApiClient();
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
