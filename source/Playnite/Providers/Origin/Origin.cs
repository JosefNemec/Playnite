using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using NLog;
using Playnite.Models;
using Playnite.Providers.Steam;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace Playnite.Providers.Origin
{
    public class Origin
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static string GetPathFromPlatformPath(string path)
        {
            if (!path.StartsWith("["))
            {
                return path;
            }

            var matchPath = Regex.Match(path, @"\[(.*?)\\(.*)\\(.*)\](.*)");
            if (!matchPath.Success)
            {
                logger.Warn("Uknown path format " + path);
                return string.Empty;
            }

            var root = matchPath.Groups[1].Value;
            RegistryKey rootKey = null;

            switch (root)
            {
                case "HKEY_LOCAL_MACHINE":
                    rootKey = Registry.LocalMachine;
                    break;

                case "HKEY_CURRENT_USER":
                    rootKey = Registry.CurrentUser;
                    break;

                default:
                    throw new Exception("Unknown registr root entry " + root);
            }

            var subPath = matchPath.Groups[2].Value.Trim(Path.DirectorySeparatorChar);
            var key = matchPath.Groups[3].Value;
            var executable = matchPath.Groups[4].Value.Trim(Path.DirectorySeparatorChar);
            var subKey = rootKey.OpenSubKey(subPath);
            if (subKey == null)
            {
                return string.Empty;
            }

            var keyValue = rootKey.OpenSubKey(subPath).GetValue(key);
            if (keyValue == null)
            {
                return string.Empty;
            }

            return Path.Combine(keyValue.ToString(), executable);
        }

        public static System.Collections.Specialized.NameValueCollection ParseOriginManifest(string path)
        {
            var text = File.ReadAllText(path);
            var data = HttpUtility.UrlDecode(text);
            return HttpUtility.ParseQueryString(data);
        }


        public static List<Game> GetInstalledGames(bool useDataCache = false)
        {
            var contentPath = Path.Combine(OriginPaths.DataPath, "LocalContent");
            var games = new List<Game>();

            if (Directory.Exists(contentPath))
            {
                var packages = Directory.GetFiles(contentPath, "*.mfst", SearchOption.AllDirectories);
                foreach (var package in packages)
                {
                    var gameId = Path.GetFileNameWithoutExtension(package);
                    if (!gameId.StartsWith("Origin"))
                    {
                        // Get game id by fixing file via adding : before integer part of the name
                        // for example OFB-EAST52017 converts to OFB-EAST:52017
                        var match = Regex.Match(gameId, @"(.*?)(\d+)");
                        if (!match.Success)
                        {
                            logger.Warn("Failed to get game id from file " + package);
                            continue;
                        }

                        gameId = match.Groups[1].Value + ":" + match.Groups[2].Value;
                    }

                    var newGame = new Game()
                    {
                        Provider = Provider.Origin,
                        ProviderId = gameId
                    };

                    GameLocalDataResponse localData;
                    var cacheFile = Path.Combine(OriginPaths.CachePath, Path.GetFileNameWithoutExtension(package) + ".json");
                    if (useDataCache == true && File.Exists(cacheFile))
                    {
                        localData = JsonConvert.DeserializeObject<GameLocalDataResponse>(File.ReadAllText(cacheFile, Encoding.UTF8));
                    }
                    else if (useDataCache == true && !File.Exists(cacheFile))
                    {
                        FileSystem.CreateFolder(OriginPaths.CachePath);
                        localData = WebApiClient.GetGameLocalData(gameId);
                        File.WriteAllText(cacheFile, JsonConvert.SerializeObject(localData), Encoding.UTF8);
                    }
                    else
                    {
                        localData = WebApiClient.GetGameLocalData(gameId);
                    }
                     
                    if (localData.offerType != "Base Game")
                    {
                        continue;
                    }

                    newGame.Name = localData.localizableAttributes.displayName;
                    var platform = localData.publishing.softwareList.software.FirstOrDefault(a => a.softwarePlatform == "PCWIN");

                    if (platform == null)
                    {
                        logger.Warn(gameId + " game doesn't have windows platform, skipping install import.");
                        continue;
                    }

                    var installPath = GetPathFromPlatformPath(platform.fulfillmentAttributes.installCheckOverride);
                    if (string.IsNullOrEmpty(installPath))
                    {
                        continue;
                    }

                    newGame.InstallDirectory = Path.GetDirectoryName(installPath);

                    var playTask = new GameTask()
                    {
                        IsBuiltIn = true,
                        IsPrimary = true
                    };

                    if (platform.fulfillmentAttributes.executePathOverride.Contains(@"://"))
                    {
                        playTask.Type = GameTaskType.URL;
                        playTask.Path = platform.fulfillmentAttributes.executePathOverride;
                    }
                    else
                    {
                        var executePath = GetPathFromPlatformPath(platform.fulfillmentAttributes.executePathOverride);
                        if (string.IsNullOrEmpty(executePath))
                        {
                            continue;
                        }

                        playTask.WorkingDir = newGame.InstallDirectory;
                        playTask.Path = executePath;
                    }

                    newGame.PlayTask = playTask;
                    games.Add(newGame);
                }                
            }

            return games;
        }

        public static List<AccountEntitlementsResponse.Entitlement> GetOwnedGames()
        {
            var api = new WebApiClient();
            if (api.GetLoginRequired())
            {
                throw new Exception("User is not logged in.");
            }

            var token = api.GetAccessToken();
            if (!string.IsNullOrEmpty(token.error))
            {
                throw new Exception("Access error: " + token.error);
            }

            var info = api.GetAccountInfo(token);
            if (!string.IsNullOrEmpty(info.error))
            {
                throw new Exception("Access error: " + info.error);
            }

            var games = api.GetOwnedGames(info.pid.pidId, token);
            return games;
        }

        public static OriginGameMetadata DownloadGameMetadata(string id)
        {
            var data = new OriginGameMetadata()
            {
                StoreDetails = WebApiClient.GetGameStoreData(id)
            };

            var imageUrl = data.StoreDetails.imageServer + data.StoreDetails.i18n.packArtLarge;
            var imageData = Web.DownloadData(imageUrl);
            var imageName = Guid.NewGuid() + Path.GetExtension(new Uri(imageUrl).AbsolutePath);
            data.Image = new OriginGameMetadata.ImageData()
            {
                Data = imageData,
                Name = imageName
            };

            return data;
        }
    }

    public class OriginGameMetadata
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

        public GameStoreDataResponse StoreDetails
        {
            get; set;
        }

        public ImageData Image
        {
            get; set;
        }
    }
}
