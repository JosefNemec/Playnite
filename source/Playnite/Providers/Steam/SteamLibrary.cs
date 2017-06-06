using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using Newtonsoft.Json;
using NLog;
using Playnite.Models;
using Playnite.Providers.Steam;
using SteamKit2;
using Playnite.Services;
using Playnite.Database;
using System.Windows;

namespace Playnite.Providers.Steam
{
    public class SteamLibrary : ISteamLibrary
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        public IGame GetInstalledGameFromFile(string path)
        {
            var kv = new KeyValue();
            kv.ReadFileAsText(path);

            var name = string.Empty;
            if (string.IsNullOrEmpty(kv["name"].Value))
            {
                if (kv["UserConfig"]["name"].Value != null)
                {
                    name = kv["UserConfig"]["name"].Value;
                }
            }
            else
            {
                name = kv["name"].Value;
            }

            var game = new Game()
            {
                Provider = Provider.Steam,
                ProviderId = kv["appID"].Value,
                Name = name,
                InstallDirectory = Path.Combine((new FileInfo(path)).Directory.FullName, "common", kv["installDir"].Value),
                PlayTask = new GameTask()
                {
                    Name = "Play",
                    Type = GameTaskType.URL,
                    Path = @"steam://run/" + kv["appID"].Value,
                    IsPrimary = true,
                    IsBuiltIn = true
                }
            };

            return game;
        }

        public List<IGame> GetInstalledGamesFromFolder(string path)
        {
            var games = new List<IGame>();

            foreach (var file in Directory.GetFiles(path, @"appmanifest*"))
            {
                try
                {
                    var game = GetInstalledGameFromFile(Path.Combine(path, file));
                    games.Add(game);
                }
                catch (Exception exc)
                {
                    // Steam can generate invalid acf file according to issue #37
                    logger.Error(exc, "Failed to get information about installed game from {0}: ", path);
                }                
            }

            return games;
        }

        public List<IGame> GetInstalledGames()
        {
            var games = new List<IGame>();

            foreach (var folder in GetLibraryFolders())
            {
                games.AddRange(GetInstalledGamesFromFolder(Path.Combine(folder, "steamapps")));
            }

            return games;
        }

        public List<string> GetLibraryFolders()
        {
            var dbs = new List<string>() { SteamSettings.InstallationPath };
            var configPath = Path.Combine(SteamSettings.InstallationPath, "steamapps", "libraryfolders.vdf");
            var kv = new KeyValue();
            kv.ReadFileAsText(configPath);

            foreach (var child in kv.Children)
            {
                if (int.TryParse(child.Name, out int test))
                {
                    dbs.Add(child.Value);
                }
            }

            return dbs;
        }

        public List<IGame> GetLibraryGames(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new Exception("Steam user name cannot be empty.");
            }           

            var games = new List<IGame>();

            foreach (var game in (new ServicesClient()).GetSteamLibrary(userName))
            {
                games.Add(new Game()
                {
                    Provider = Provider.Steam,
                    ProviderId = game.appid.ToString(),
                    Name = game.name
                });
            }

            return games;
        }

        public SteamGameMetadata DownloadGameMetadata(int id)
        {
            var metadata = new SteamGameMetadata();
            var productInfo = SteamApiClient.GetProductInfo(id).GetAwaiter().GetResult();
            metadata.ProductDetails = productInfo;

            // Steam may return 429 if we put too many request
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    metadata.StoreDetails = WebApiClient.GetStoreAppDetail(id);
                    break;
                }
                catch (WebException e)
                {
                    if (i + 1 == 10)
                    {
                        throw;
                    }

                    if (e.Message.Contains("429"))
                    {
                        Thread.Sleep(2500);
                        continue;
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Icon
            var iconRoot = @"https://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}.ico";
            var icon = productInfo["common"]["clienticon"];
            var iconUrl = string.Empty;
            if (icon.Name != null)
            {
                iconUrl = string.Format(iconRoot, id, icon.Value);
            }
            else
            {
                var newIcon = productInfo["common"]["icon"];
                if (newIcon.Name != null)
                {
                    iconRoot = @"https://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}.jpg";
                    iconUrl = string.Format(iconRoot, id, newIcon.Value);   
                }
            }

            // There might be no icon assigned to game
            if (!string.IsNullOrEmpty(iconUrl))
            {
                var iconName = Path.GetFileName(new Uri(iconUrl).AbsolutePath);
                var iconData = Web.DownloadData(iconUrl);
                metadata.Icon = new FileDefinition(
                
                    string.Format("images/steam/{0}/{1}", id.ToString(), iconName),
                    iconName,
                    iconData
                );
            }
             

            // Image
            var imageRoot = @"http://cdn.akamai.steamstatic.com/steam/apps/{0}/header.jpg";
            var imageUrl = string.Format(imageRoot, id);
            byte[] imageData = null;

            try
            {
                imageData = Web.DownloadData(imageUrl);
            }
            catch (WebException e)
            {
                var response = (HttpWebResponse)e.Response;
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    imageRoot = @"https://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}.jpg";
                    var image = productInfo["common"]["logo"];
                    if (image.Name != null)
                    {
                        imageUrl = string.Format(imageRoot, id, image.Value);
                        imageData = Web.DownloadData(imageUrl);
                    }
                }
                else
                {
                    throw;
                }
            }

            if (imageData != null)
            {
                var imageName = Path.GetFileName(new Uri(imageUrl).AbsolutePath);
                metadata.Image = new FileDefinition(                
                    string.Format("images/steam/{0}/{1}", id.ToString(), imageName),
                    imageName,
                    imageData
                );
            }

            // Background Image
            metadata.BackgroundImage = string.Format(@"https://steamcdn-a.akamaihd.net/steam/apps/{0}/page_bg_generated_v6b.jpg", id);

            return metadata;
        }

        public SteamGameMetadata UpdateGameWithMetadata(IGame game)
        {
            var metadata = DownloadGameMetadata(int.Parse(game.ProviderId));
            game.Name = metadata.ProductDetails["common"]["name"].Value;
            game.Links = new ObservableCollection<Link>()
            {
                new Link("Forum", @"https://steamcommunity.com/app/" + game.ProviderId),
                new Link("Store", @"http://store.steampowered.com/app/" + game.ProviderId),
                new Link("Wiki", @"http://pcgamingwiki.com/api/appid.php?appid=" + game.ProviderId)
            };

            if (metadata.StoreDetails != null)
            {
                game.Description = metadata.StoreDetails.detailed_description;
                game.Genres = metadata.StoreDetails.genres?.Select(a => a.description).ToList();
                game.Developers = metadata.StoreDetails.developers;
                game.Publishers = metadata.StoreDetails.publishers;
                game.ReleaseDate = metadata.StoreDetails.release_date.date;
            }

            var tasks = new ObservableCollection<GameTask>();
            var launchList = metadata.ProductDetails["config"]["launch"].Children;
            foreach (var task in launchList.Skip(1))
            {
                var properties = task["config"];
                if (properties.Name != null)
                {
                    if (properties["oslist"].Name != null)
                    {
                        if (properties["oslist"].Value != "windows")
                        {
                            continue;
                        }
                    }
                }

                // Ignore action without name  - shoudn't be visible to end user
                if (task["description"].Name != null)
                {
                    var newTask = new GameTask()
                    {
                        Name = task["description"].Value,
                        Arguments = task["arguments"].Value ?? string.Empty,
                        Path = task["executable"].Value,
                        IsBuiltIn = true,
                        WorkingDir = game.InstallDirectory
                    };

                    tasks.Add(newTask);
                }
            }

            var manual = metadata.ProductDetails["extended"]["gamemanualurl"];
            if (manual.Name != null)
            {
                tasks.Add((new GameTask()
                {
                    Name = "Manual",
                    Type = GameTaskType.URL,
                    Path = manual.Value,
                    IsBuiltIn = true
                }));
            }

            game.OtherTasks = tasks;

            if (metadata.Image != null)
            {
                using (var imageStream = new MemoryStream())
                {
                    using (var tempStream = new MemoryStream(metadata.Image.Data))
                    {
                        using (var backStream = Application.GetResourceStream(new Uri("pack://application:,,,/Playnite;component/Resources/Images/steam_cover_background.png")).Stream)
                        {
                            CoverCreator.CreateCover(backStream, tempStream, imageStream);
                            imageStream.Seek(0, SeekOrigin.Begin);
                            metadata.Image.Data = imageStream.ToArray();
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(metadata.BackgroundImage))
            {
                game.BackgroundImage = metadata.BackgroundImage;
            }

            game.IsProviderDataUpdated = true;
            return metadata;
        }
    }

    public class SteamGameMetadata : GameMetadata
    {
        public KeyValue ProductDetails
        {
            get;set;
        }

        public StoreAppDetailsResult.AppDetails StoreDetails
        {
            get; set;
        }
    }
}
