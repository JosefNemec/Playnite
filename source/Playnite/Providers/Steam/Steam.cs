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

namespace Playnite.Providers.Steam
{
    public class Steam
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static Game GetInstalledGamesFromFile(string path)
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

        public static List<Game> GetInstalledGamesFromFolder(string path)
        {
            var games = new List<Game>();
            var appsFolder = Path.Combine(path, "steamapps");

            foreach (var file in Directory.GetFiles(appsFolder, @"appmanifest*"))
            {
                var game = GetInstalledGamesFromFile(Path.Combine(appsFolder, file));
                games.Add(game);
            }

            return games;
        }

        public static List<Game> GetInstalledGames()
        {
            var games = new List<Game>();

            foreach (var folder in SteamSettings.GameDatabases)
            {
                games.AddRange(Steam.GetInstalledGamesFromFolder(folder));
            }

            return games;
        }

        public static List<GetOwnedGamesResult.Game> GetOwnedGames(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new Exception("Steam user name cannot be empty.");
            }
            
            return (new ServicesClient()).GetSteamLibrary(userName);
        }

        public static SteamGameMetadata DownloadGameMetadata(int id)
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
                var iconData = Web.DownloadData(iconUrl);
                metadata.Icon = new SteamGameMetadata.ImageData()
                {
                    Name = Path.GetFileName(new Uri(iconUrl).AbsolutePath),
                    Data = iconData
                };
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
                metadata.Image = new SteamGameMetadata.ImageData()
                {
                    Name = Path.GetFileName(new Uri(imageUrl).AbsolutePath),
                    Data = imageData
                };
            }

            // Background Image
            metadata.BackgroundImage = string.Format(@"https://steamcdn-a.akamaihd.net/steam/apps/{0}/page_bg_generated_v6b.jpg", id);

            return metadata;
        }
    }

    public class SteamGameMetadata
    {
        public class ImageData
        {
            public string Name
            {
                get; set;
            }

            public byte[] Data
            {
                get; set;
            }
        }

        public KeyValue ProductDetails
        {
            get;set;
        }

        public StoreAppDetailsResult.AppDetails StoreDetails
        {
            get; set;
        }

        public ImageData Icon
        {
            get; set;
        }

        public ImageData Image
        {
            get; set;
        }

        public string BackgroundImage
        {
            get; set;
        }
    }
}
