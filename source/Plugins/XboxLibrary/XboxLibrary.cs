using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using XboxLibrary.Models;
using XboxLibrary.Services;

namespace XboxLibrary
{
    public class XboxLibrary : LibraryPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private const string dbImportMessageId = "xboxlibImportError";
        private readonly string pfnInfoCacheDir;

        internal XboxLibrarySettings Settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("7e4fbb5e-2ae3-48d4-8ba0-6b30e7a4e287");

        public override string Name => "Xbox";

        public override LibraryClient Client => new XboxLibraryClient(Settings);

        public override string LibraryIcon => Xbox.Icon;

        public XboxLibrary(IPlayniteAPI api) : base(api)
        {
            Settings = new XboxLibrarySettings(this);
            pfnInfoCacheDir = Path.Combine(GetPluginUserDataPath(), "PfnInfoCache");
        }

        internal GameInfo GetGameInfoFromTitle(TitleHistoryResponse.Title title)
        {
            var newGame = new GameInfo
            {
                GameId = title.pfn,
                Name = title.name.
                Replace("(PC)", "").
                Replace("(Windows)", "").
                Replace("for Windows 10", "").
                Replace("- Windows 10", "").
                Trim(),
                Platform = "PC",
                Source = "Xbox"
            };

            if (title.detail != null)
            {
                if (title.detail.releaseDate != null)
                {
                    newGame.ReleaseDate = title.detail.releaseDate;
                }

                if (!title.detail.publisherName.IsNullOrEmpty())
                {
                    newGame.Publishers = title.detail.publisherName.Split(new char[] { '|' }).Select(a => a.Trim()).ToList();
                }

                if (!title.detail.developerName.IsNullOrEmpty())
                {
                    newGame.Developers = title.detail.developerName.Split(new char[] { '|' }).Select(a => a.Trim()).ToList();
                }
            }

            return newGame;
        }

        public List<TitleHistoryResponse.Title> GetAppDataCache()
        {
            var items = new List<TitleHistoryResponse.Title>();
            if (Directory.Exists(pfnInfoCacheDir))
            {
                foreach (var file in Directory.GetFiles(pfnInfoCacheDir, "*.json", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        items.Add(Serialization.FromJsonFile<TitleHistoryResponse.Title>(file));
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, $"Failed to get app info from cache {file}.");
                    }
                }
            }

            return items;
        }

        public void WriteAppDataCache(TitleHistoryResponse.Title data)
        {
            var filePath = Path.Combine(pfnInfoCacheDir, data.pfn + ".json");
            FileSystem.PrepareSaveFile(filePath);
            File.WriteAllText(filePath, Serialization.ToJson(data));
        }

        public override IEnumerable<GameInfo> GetGames()
        {
            var installedGames = new Dictionary<string, GameInfo>();
            Exception importError = null;
            var allGames = new List<GameInfo>();
            if (!Settings.ConnectAccount)
            {
                return allGames;
            }

            if (Computer.WindowsVersion != WindowsVersion.Win10)
            {
                logger.Error("Xbox game library is only supported on Windows 10.");
                return allGames;
            }

            var titles = new List<TitleHistoryResponse.Title>();
            var client = new XboxAccountClient(this);

            try
            {
                titles = client.GetLibraryTitles().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to Xbox profile titles.");
                importError = e;
            }

            var appDataCache = GetAppDataCache();
            var pcTitles = titles.Where(title => !title.pfn.IsNullOrEmpty() &&
                    title.type == "Game" &&
                    title.devices?.Contains("PC") == true).ToList();

            if (Settings.ImportInstalledGames)
            {
                try
                {
                    var installedApps = Programs.GetUWPApps();
                    foreach (var installedApp in installedApps)
                    {
                        var import = false;
                        var libTitle = pcTitles.FirstOrDefault(a => a.pfn == installedApp.AppId);
                        if (libTitle != null)
                        {
                            import = true;
                        }
                        else // Check if it's a game that was not started at least once (won't appear in user API data)
                        {
                            try
                            {
                                libTitle = appDataCache.FirstOrDefault(a => a.pfn == installedApp.AppId);
                                if (libTitle == null)
                                {
                                    libTitle = client.GetTitleInfo(installedApp.AppId).GetAwaiter().GetResult();
                                    WriteAppDataCache(libTitle);
                                }

                                if (libTitle.type == "Game")
                                {
                                    import = true;
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Error(e, $"Failed to get info about installed UWP package {installedApp.AppId}.");
                            }
                        }

                        if (import)
                        {
                            var game = GetGameInfoFromTitle(libTitle);
                            game.IsInstalled = true;
                            game.InstallDirectory = installedApp.WorkDir;
                            game.Icon = installedApp.Icon;
                            game.PlayAction = new GameAction
                            {
                                Type = GameActionType.File,
                                Arguments = installedApp.Arguments,
                                Path = installedApp.Path,
                                IsHandledByPlugin = true
                            };

                            installedGames.Add(libTitle.pfn, game);
                        }
                    }

                    logger.Debug($"Found {installedGames.Count} installed Xbox games.");
                    allGames.AddRange(installedGames.Values.ToList());
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to import installed Xbox games.");
                    importError = e;
                }
            }

            if (Settings.ImportUninstalledGames)
            {
                try
                {
                    logger.Debug($"Found {pcTitles.Count} Xbox PC games.");
                    foreach (var libTitle in pcTitles)
                    {
                        if (!installedGames.TryGetValue(libTitle.pfn, out var installed))
                        {
                            allGames.Add(GetGameInfoFromTitle(libTitle));
                        }
                    }

                    foreach (var title in titles)
                    {
                        if (title.devices.HasItems() &&
                            (title.devices.Contains("Xbox360") || title.devices.Contains("XboxOne")) &&
                            !title.devices.Contains("PC"))
                        {
                            var addGame = false;
                            var platform = "";
                            if (Settings.Import360Games && title.devices.Contains("Xbox360"))
                            {
                                addGame = true;
                                platform = "Microsoft Xbox 360";
                            }
                            else if (Settings.ImportXboneGames && title.devices.Contains("XboxOne"))
                            {
                                addGame = true;
                                platform = "Microsoft Xbox One";
                            }

                            if (addGame)
                            {
                                var newGame = GetGameInfoFromTitle(title);
                                newGame.GameId = $"CONSOLE_{title.titleId}_{title.mediaItemType}";
                                newGame.Platform = platform;
                                allGames.Add(newGame);
                            };
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to import linked account Xbox games details.");
                    importError = e;
                }
            }

            if (importError != null)
            {
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    dbImportMessageId,
                    string.Format(PlayniteApi.Resources.GetString("LOCLibraryImportError"), Name) +
                    System.Environment.NewLine + importError.Message,
                    NotificationType.Error,
                    () => OpenSettingsView()));
            }
            else
            {
                PlayniteApi.Notifications.Remove(dbImportMessageId);
            }

            return allGames;
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            Settings.IsFirstRunUse = firstRunSettings;
            return Settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new XboxLibrarySettingsView();
        }

        public override IGameController GetGameController(Game game)
        {
            return new XboxGameController(game, PlayniteApi, Settings);
        }
    }
}