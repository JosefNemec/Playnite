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
using System.Collections.ObjectModel;
using Playnite.Database;
using System.Net;

namespace Playnite.Providers.Origin
{
    public class OriginLibrary : IOriginLibrary
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        public string GetPathFromPlatformPath(string path, RegistryView platformView)
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
                    rootKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, platformView);
                    break;

                case "HKEY_CURRENT_USER":
                    rootKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, platformView);
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

        public string GetPathFromPlatformPath(string path)
        {
            var resultPath = GetPathFromPlatformPath(path, RegistryView.Registry64);

            if (string.IsNullOrEmpty(resultPath))
            {
                resultPath = GetPathFromPlatformPath(path, RegistryView.Registry32);
            }

            return resultPath;
        }

        public System.Collections.Specialized.NameValueCollection ParseOriginManifest(string path)
        {
            var text = File.ReadAllText(path);
            var data = HttpUtility.UrlDecode(text);
            return HttpUtility.ParseQueryString(data);
        }

        public GameLocalDataResponse GetLocalManifest(string id, string packageName = null, bool useDataCache = false)
        {
            var package = packageName;

            if (string.IsNullOrEmpty(package))
            {
                package = id.Replace(":", "");
            }

            var cacheFile = Path.Combine(OriginPaths.CachePath, Path.GetFileNameWithoutExtension(package) + ".json");
            if (useDataCache == true && File.Exists(cacheFile))
            {
                return JsonConvert.DeserializeObject<GameLocalDataResponse>(File.ReadAllText(cacheFile, Encoding.UTF8));
            }
            else if (useDataCache == true && !File.Exists(cacheFile))
            {
                logger.Debug($"Downloading game manifest {id}");
                FileSystem.CreateDirectory(OriginPaths.CachePath);

                try
                {
                    var data = WebApiClient.GetGameLocalData(id);
                    File.WriteAllText(cacheFile, JsonConvert.SerializeObject(data), Encoding.UTF8);
                    return data;
                }
                catch (WebException exc) when ((exc.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
                {
                    logger.Info($"Origin manifest {id} not found on EA server, generating fake manifest.");
                    var data = new GameLocalDataResponse()
                    {
                        offerId = id,
                        offerType = "Doesn't exists"
                    };

                    File.WriteAllText(cacheFile, JsonConvert.SerializeObject(data), Encoding.UTF8);
                    return data;
                }
            }
            else
            {
                return WebApiClient.GetGameLocalData(id);
            }
        }

        public GameTask GetGamePlayTask(GameLocalDataResponse manifest)
        {
            var platform = manifest.publishing.softwareList.software.FirstOrDefault(a => a.softwarePlatform == "PCWIN");
            var playTask = new GameTask()
            {
                IsBuiltIn = true,
                IsPrimary = true
            };

            if (string.IsNullOrEmpty(platform.fulfillmentAttributes.executePathOverride))
            {
                return null;
            }

            if (platform.fulfillmentAttributes.executePathOverride.Contains(@"://"))
            {
                playTask.Type = GameTaskType.URL;
                playTask.Path = platform.fulfillmentAttributes.executePathOverride;
            }
            else
            {
                var executePath = GetPathFromPlatformPath(platform.fulfillmentAttributes.executePathOverride);
                playTask.WorkingDir = Path.GetDirectoryName(GetPathFromPlatformPath(platform.fulfillmentAttributes.installCheckOverride));
                playTask.Path = executePath;
            }

            return playTask;
        }

        public List<IGame> GetInstalledGames(bool useDataCache = false)
        {
            var contentPath = Path.Combine(OriginPaths.DataPath, "LocalContent");
            var games = new List<IGame>();

            if (Directory.Exists(contentPath))
            {
                var packages = Directory.GetFiles(contentPath, "*.mfst", SearchOption.AllDirectories);
                foreach (var package in packages)
                {
                    try
                    {
                        var gameId = Path.GetFileNameWithoutExtension(package);
                        if (!gameId.StartsWith("Origin"))
                        {
                            // Get game id by fixing file via adding : before integer part of the name
                            // for example OFB-EAST52017 converts to OFB-EAST:52017
                            var match = Regex.Match(gameId, @"^(.*?)(\d+)$");
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

                        GameLocalDataResponse localData = null;

                        try
                        {
                            localData = GetLocalManifest(gameId, package, useDataCache);
                        }
                        catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                        {
                            logger.Error(e, $"Failed to get Origin manifest for a {gameId}, {package}");
                            continue;
                        }

                        if (localData.offerType != "Base Game" && localData.offerType != "DEMO")
                        {
                            continue;
                        }

                        newGame.Name = StringExtensions.NormalizeGameName(localData.localizableAttributes.displayName);
                        var platform = localData.publishing.softwareList.software.FirstOrDefault(a => a.softwarePlatform == "PCWIN");

                        if (platform == null)
                        {
                            logger.Warn(gameId + " game doesn't have windows platform, skipping install import.");
                            continue;
                        }

                        var installPath = GetPathFromPlatformPath(platform.fulfillmentAttributes.installCheckOverride);
                        if (string.IsNullOrEmpty(installPath) || !File.Exists(installPath))
                        {
                            continue;
                        }

                        newGame.InstallDirectory = Path.GetDirectoryName(installPath);
                        newGame.PlayTask = GetGamePlayTask(localData);
                        if (newGame.PlayTask?.Type == GameTaskType.File)
                        {
                            newGame.PlayTask.WorkingDir = newGame.PlayTask.WorkingDir.Replace(newGame.InstallDirectory, "{InstallDir}");
                            newGame.PlayTask.Path = newGame.PlayTask.Path.Replace(newGame.InstallDirectory, "{InstallDir}");
                        }

                        games.Add(newGame);
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, $"Failed to import installed Origin game {package}.");
                    }
                }                
            }

            return games;
        }

        public List<IGame> GetLibraryGames()
        {
            using (var api = new WebApiClient())
            {
                if (api.GetLoginRequired())
                {
                    throw new Exception("User is not logged in.");
                }

                var token = api.GetAccessToken();
                if (token == null)
                {
                    throw new Exception("Failed to get access to user account.");
                }

                if (!string.IsNullOrEmpty(token.error))
                {
                    throw new Exception("Access error: " + token.error);
                }

                var info = api.GetAccountInfo(token);
                if (!string.IsNullOrEmpty(info.error))
                {
                    throw new Exception("Access error: " + info.error);
                }

                var games = new List<IGame>();

                foreach (var game in api.GetOwnedGames(info.pid.pidId, token).Where(a => a.offerType == "basegame"))
                {
                    games.Add(new Game()
                    {
                        Provider = Provider.Origin,
                        ProviderId = game.offerId,
                        Name = game.offerId
                    });
                }

                return games;
            }
        }

        public OriginGameMetadata DownloadGameMetadata(string id)
        {
            var data = new OriginGameMetadata()
            {
                StoreDetails = WebApiClient.GetGameStoreData(id)
            };

            var imageUrl = data.StoreDetails.imageServer + data.StoreDetails.i18n.packArtLarge;
            var imageData = Web.DownloadData(imageUrl);
            var imageName = Guid.NewGuid() + Path.GetExtension(new Uri(imageUrl).AbsolutePath);

            data.Image = new FileDefinition(          
                string.Format("images/origin/{0}/{1}", id.Replace(":", ""), imageName),
                imageName,
                imageData
            );

            return data;
        }

        public OriginGameMetadata UpdateGameWithMetadata(IGame game)
        {
            var metadata = DownloadGameMetadata(game.ProviderId);
            game.Name = StringExtensions.NormalizeGameName(metadata.StoreDetails.i18n.displayName);
            game.Links = new ObservableCollection<Link>()
            {
                new Link("Store", @"https://www.origin.com/store" + metadata.StoreDetails.offerPath),
                new Link("Wiki", @"http://pcgamingwiki.com/w/index.php?search=" + game.Name)
            };

            if (!string.IsNullOrEmpty(metadata.StoreDetails.i18n.gameForumURL))
            {
                game.Links.Add(new Link("Forum", metadata.StoreDetails.i18n.gameForumURL));
            }

            game.Description = metadata.StoreDetails.i18n.longDescription;
            game.Developers = new ComparableList<string>() { metadata.StoreDetails.developerFacetKey };
            game.Publishers = new ComparableList<string>() { metadata.StoreDetails.publisherFacetKey };
            game.Genres = new ComparableList<string>(metadata.StoreDetails.genreFacetKey?.Split(','));
            game.ReleaseDate = metadata.StoreDetails.platforms.First(a => a.platform == "PCWIN").releaseDate;

            if (!string.IsNullOrEmpty(metadata.StoreDetails.i18n.gameManualURL))
            {
                game.OtherTasks = new ObservableCollection<GameTask>()
                {
                    new GameTask()
                    {
                        IsBuiltIn = true,
                        Type = GameTaskType.URL,
                        Path = metadata.StoreDetails.i18n.gameManualURL,
                        Name = "Manual"
                    }
                };
            }

            // There's not icon available on Origin servers so we will load one from EXE
            if (game.IsInstalled && string.IsNullOrEmpty(game.Icon))
            {
                var exeIcon = IconExtension.ExtractIconFromExe(game.PlayTask.Path, true);
                if (exeIcon != null)
                {
                    var iconName = Guid.NewGuid() + ".png";

                    metadata.Icon = new FileDefinition(
                        string.Format("images/origin/{0}/{1}", game.ProviderId.Replace(":", ""), iconName),
                        iconName,
                        exeIcon.ToByteArray(System.Drawing.Imaging.ImageFormat.Png)
                    );
                }
            }

            return metadata;
        }
    }

    public class OriginGameMetadata : GameMetadata
    {
        public GameStoreDataResponse StoreDetails
        {
            get; set;
        }
    }
}
