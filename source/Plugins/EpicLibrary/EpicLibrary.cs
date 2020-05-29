﻿using EpicLibrary.Models;
using EpicLibrary.Services;
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

namespace EpicLibrary
{
    public class EpicLibrary : LibraryPlugin
    {
        private ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI playniteApi;
        private const string dbImportMessageId = "epiclibImportError";
        internal readonly string TokensPath;
        internal readonly EpicLibrarySettings LibrarySettings;

        // For acces to staic function from EpicAccountClient / EpicMetadataProvider // WebStoreClient
        public static string _EpicLang { get; set; }

        public EpicLibrary(IPlayniteAPI api) : base(api)
        {
            playniteApi = api;
            LibrarySettings = new EpicLibrarySettings(this, api)
            {
                EpicLangs = GetEpicLangs()
            };
            TokensPath = Path.Combine(GetPluginUserDataPath(), "tokens.json");
            _EpicLang = LibrarySettings.EpicLang;
        }

        // Initialize available language list for Steam if exist in Playnite language.
        internal List<LocalEpicLang> GetEpicLangs()
        {
            //https://partner.steamgames.com/doc/store/localization?#supported_languages
            List<LocalEpicLang> _EpicLang = new List<LocalEpicLang>();

            _EpicLang.Add(new LocalEpicLang { EpicLangName = "zh-Hant", LocalLang = "繁體中文" });
            _EpicLang.Add(new LocalEpicLang { EpicLangName = "en-US", LocalLang = "english" });
            _EpicLang.Add(new LocalEpicLang { EpicLangName = "fr-FR", LocalLang = "Français" });
            _EpicLang.Add(new LocalEpicLang { EpicLangName = "de-DE", LocalLang = "Deutsch" });
            _EpicLang.Add(new LocalEpicLang { EpicLangName = "it-IT", LocalLang = "Italiano" });
            _EpicLang.Add(new LocalEpicLang { EpicLangName = "ja-JP", LocalLang = "日本語" });
            _EpicLang.Add(new LocalEpicLang { EpicLangName = "ko-KR", LocalLang = "한국어" });
            _EpicLang.Add(new LocalEpicLang { EpicLangName = "ru-RU", LocalLang = "Русский" });
            _EpicLang.Add(new LocalEpicLang { EpicLangName = "es-ES", LocalLang = "Español" });
            _EpicLang.Add(new LocalEpicLang { EpicLangName = "pl-PL", LocalLang = "Polski" });
            _EpicLang.Add(new LocalEpicLang { EpicLangName = "tr-TR", LocalLang = "Türkçe" });

            return _EpicLang.OrderBy(a => a.LocalLang).ToList();
        }

        internal Dictionary<string, GameInfo> GetInstalledGames()
        {
            var games = new Dictionary<string, GameInfo>();
            var appList = EpicLauncher.GetInstalledAppList();
            var manifests = EpicLauncher.GetInstalledManifests();

            foreach (var app in appList)
            {
                if (app.AppName.StartsWith("UE_"))
                {
                    continue;
                }

                var manifest = manifests.FirstOrDefault(a => a.AppName == app.AppName);
                var game = new GameInfo()
                {
                    Source = "Epic",
                    GameId = app.AppName,
                    Name = manifest?.DisplayName ?? Path.GetFileName(app.InstallLocation),
                    InstallDirectory = manifest?.InstallLocation ?? app.InstallLocation,
                    IsInstalled = true,
                    PlayAction = new GameAction()
                    {
                        Type = GameActionType.URL,
                        Path = string.Format(EpicLauncher.GameLaunchUrlMask, app.AppName),
                        IsHandledByPlugin = true
                    }
                };

                games.Add(game.GameId, game);
            }

            return games;
        }

        internal List<GameInfo> GetLibraryGames()
        {
            var cacheDir = GetCachePath("catalogcache");
            var games = new List<GameInfo>();
            var accountApi = new EpicAccountClient(playniteApi, TokensPath);
            var assets = accountApi.GetAssets();
            if (!assets?.Any() == true)
            {
                logger.Warn("Found no assets on Epic accounts.");
            }

            foreach (var gameAsset in assets.Where(a => a.@namespace != "ue"))
            {
                var cacheFile = Paths.GetSafeFilename($"{gameAsset.@namespace}_{gameAsset.catalogItemId}_{gameAsset.buildVersion}.json");
                cacheFile = Path.Combine(cacheDir, cacheFile);
                var catalogItem = accountApi.GetCatalogItem(gameAsset.@namespace, gameAsset.catalogItemId, cacheFile);
                if (catalogItem?.categories?.Where(a => a.path == "applications").Any() != true)
                {
                    continue;
                }

                games.Add(new GameInfo()
                {
                    Source = "Epic",
                    GameId = gameAsset.appName,
                    Name = catalogItem.title,
                });
            }

            return games;
        }

        #region ILibraryPlugin

        public override LibraryClient Client => new EpicClient();

        public override string Name => "Epic";

        public override string LibraryIcon => EpicLauncher.Icon;

        public override Guid Id => Guid.Parse("00000002-DBD1-46C6-B5D0-B1BA559D10E4");

        public override LibraryPluginCapabilities Capabilities { get; } = new LibraryPluginCapabilities
        {
            CanShutdownClient = true
        };

        public override ISettings GetSettings(bool firstRunSettings)
        {
            LibrarySettings.IsFirstRunUse = firstRunSettings;
            return LibrarySettings;
        }

        public override UserControl GetSettingsView(bool firstRunView)
        {
            return new EpicLibrarySettingsView();
        }

        public override IGameController GetGameController(Game game)
        {
            return new EpicGameController(game, playniteApi, LibrarySettings);
        }

        public override IEnumerable<GameInfo> GetGames()
        {
            var allGames = new List<GameInfo>();
            var installedGames = new Dictionary<string, GameInfo>();
            Exception importError = null;

            if (LibrarySettings.ImportInstalledGames)
            {
                try
                {
                    installedGames = GetInstalledGames();
                    logger.Debug($"Found {installedGames.Count} installed Epic games.");
                    allGames.AddRange(installedGames.Values.ToList());
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to import installed Epic games.");
                    importError = e;
                }
            }

            if (LibrarySettings.ConnectAccount)
            {
                try
                {
                    var libraryGames = GetLibraryGames();
                    logger.Debug($"Found {libraryGames.Count} library Epic games.");

                    if (!LibrarySettings.ImportUninstalledGames)
                    {
                        libraryGames = libraryGames.Where(lg => installedGames.ContainsKey(lg.GameId)).ToList();
                    }

                    foreach (var game in libraryGames)
                    {
                        if (installedGames.TryGetValue(game.GameId, out var installed))
                        {
                            installed.Playtime = game.Playtime;
                            installed.LastActivity = game.LastActivity;
                            installed.Name = game.Name;
                        }
                        else
                        {
                            allGames.Add(game);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to import linked account Epic games details.");
                    importError = e;
                }
            }

            if (importError != null)
            {
                playniteApi.Notifications.Add(new NotificationMessage(
                    dbImportMessageId,
                    string.Format(playniteApi.Resources.GetString("LOCLibraryImportError"), Name) +
                    System.Environment.NewLine + importError.Message,
                    NotificationType.Error,
                    () => OpenSettingsView()));
            }
            else
            {
                playniteApi.Notifications.Remove(dbImportMessageId);
            }

            return allGames;
        }

        public override LibraryMetadataProvider GetMetadataDownloader()
        {
            return new EpicMetadataProvider(this, PlayniteApi);
        }

        public string GetCachePath(string dirName)
        {
            return Path.Combine(GetPluginUserDataPath(), dirName);
        }

        #endregion ILibraryPlugin
    }
}
