using EpicLibrary.Services;
using Newtonsoft.Json.Linq;
using Playnite.SDK;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicLibrary
{
    public class EpicMetadataProvider : LibraryMetadataProvider
    {
        private readonly IPlayniteAPI api;
        private readonly EpicLibrary library;

        public EpicMetadataProvider(EpicLibrary library, IPlayniteAPI api)
        {
            this.api = api;
            this.library = library;
        }

        public override GameMetadata GetMetadata(Game game)
        {
            var gameInfo = new GameInfo() { Links = new List<Link>() };
            var metadata = new GameMetadata()
            {
                GameInfo = gameInfo                
            };

            using (var client = new WebStoreClient())
            {
                var catalogs = client.QuerySearch(game.Name).GetAwaiter().GetResult();
                if (catalogs.HasItems())
                {
                    var product = client.GetProductInfo(catalogs[0].productSlug).GetAwaiter().GetResult();
                    if (product.pages.HasItems())
                    {
                        var page = product.pages[0];
                        gameInfo.Description = page.data.about.description;
                        gameInfo.Developers = new List<string>() { page.data.about.developerAttribution };
                        metadata.BackgroundImage = new MetadataFile(page.data.hero.backgroundImageUrl);             
                        gameInfo.Links.Add(new Link(
                            library.PlayniteApi.Resources.GetString("LOCCommonLinksStorePage"),
                            "https://www.epicgames.com/store/en-US/product/" + catalogs[0].productSlug));

                        if (page.data.socialLinks.HasItems())
                        {
                            var links = page.data.socialLinks.
                                Where(a => a.Key.StartsWith("link") && !a.Value.IsNullOrEmpty()).
                                Select(a => new Link(a.Key.Replace("link", ""), a.Value)).ToList();
                            if (links.HasItems())
                            {
                                gameInfo.Links.AddRange(links);
                            }
                        }

                        if (!gameInfo.Description.IsNullOrEmpty())
                        {
                            gameInfo.Description = gameInfo.Description.Replace("\n", "\n<br>");
                        }
                    }
                }
            }

            gameInfo.Links.Add(new Link("PCGamingWiki", @"http://pcgamingwiki.com/w/index.php?search=" + game.Name));

            // There's not icon available on Epic servers so we will load one from EXE
            if (game.IsInstalled && string.IsNullOrEmpty(game.Icon))
            {
                var playAction = api.ExpandGameVariables(game, game.PlayAction);
                var executable = string.Empty;
                if (File.Exists(playAction.Path))
                {
                    executable = playAction.Path;
                }
                else if (!string.IsNullOrEmpty(playAction.WorkingDir))
                {
                    executable = Path.Combine(playAction.WorkingDir, playAction.Path);
                }

                var exeIcon = IconExtension.ExtractIconFromExe(executable, true);
                if (exeIcon != null)
                {
                    var iconName = Guid.NewGuid() + ".png";
                    metadata.Icon = new MetadataFile(iconName, exeIcon.ToByteArray(System.Drawing.Imaging.ImageFormat.Png));
                }
            }

            return metadata;
        }
    }
}
