using EpicLibrary.Models;
using EpicLibrary.Services;
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
    public class EpicLibrary : ILibraryPlugin
    {        
        private ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI playniteApi;
        private const string dbImportMessageId = "epiclibImportError";
        internal readonly string TokensPath;
        internal readonly EpicLibrarySettings LibrarySettings;

        public EpicLibrary(IPlayniteAPI api)
        {
            playniteApi = api;
            LibrarySettings = new EpicLibrarySettings(this, api);
            LibraryIcon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\epicicon.png");
            TokensPath = Path.Combine(api.GetPluginUserDataPath(this), "tokens.json");
        }

        internal Dictionary<string, Game> GetInstalledGames()
        {
            var games = new Dictionary<string, Game>();
            foreach (var app in EpicLauncher.GetInstalledAppList())
            {
                if (app.AppName.StartsWith("UE_"))
                {
                    continue;
                }

                var game = new Game()
                {
                    PluginId = Id,
                    Source = "Epic",
                    GameId = app.AppName,
                    Name = Path.GetFileName(app.InstallLocation),
                    InstallDirectory = app.InstallLocation,
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

        internal List<Game> GetLibraryGames()
        {
            var games = new List<Game>();
            var accountApi = new EpicAccountClient(playniteApi, TokensPath);
            var assets = accountApi.GetAssets();
            if (!assets?.Any() == true)
            {
                logger.Warn("Found no assets on Epic accounts.");
            }
            
            foreach (var gameAsset in assets.Where(a => a.@namespace != "ue"))
            {
                var catalogItem = accountApi.GetCatalogItem(gameAsset.@namespace, gameAsset.catalogItemId);
                if (catalogItem?.categories?.Where(a => a.path == "applications").Any() != true)
                {
                    continue;
                }

                games.Add(new Game()
                {
                    PluginId = Id,
                    Source = "Epic",
                    GameId = gameAsset.appName,
                    Name = catalogItem.title,
                });                
            }

            return games;
        }

        #region ILibraryPlugin

        public ILibraryClient Client { get; } = new EpicClient();

        public string Name { get; } = "Epic";

        public string LibraryIcon { get; }

        public bool IsClientInstalled => EpicLauncher.IsInstalled;

        public Guid Id { get; } = Guid.Parse("00000002-DBD1-46C6-B5D0-B1BA559D10E4");

        public void Dispose()
        {

        }

        public ISettings GetSettings(bool firstRunSettings)
        {
            return LibrarySettings;
        }

        public UserControl GetSettingsView(bool firstRunView)
        {
            return new EpicLibrarySettingsView();
        }

        public IGameController GetGameController(Game game)
        {
            return new EpicGameController(game, playniteApi);
        }

        public IEnumerable<Game> GetGames()
        {
            var allGames = new List<Game>();
            var installedGames = new Dictionary<string, Game>();
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

            if (LibrarySettings.ImportUninstalledGames)
            {
                try
                {
                    var uninstalled = GetLibraryGames();
                    logger.Debug($"Found {uninstalled.Count} library Epic games.");

                    foreach (var game in uninstalled)
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
                    logger.Error(e, "Failed to import uninstalled Epic games.");
                    importError = e;
                }
            }

            if (importError != null)
            {
                playniteApi.Notifications.Add(
                    dbImportMessageId,
                    string.Format(playniteApi.Resources.FindString("LOCLibraryImportError"), Name) +
                    System.Environment.NewLine + importError.Message,
                    NotificationType.Error);
            }
            else
            {
                playniteApi.Notifications.Remove(dbImportMessageId);
            }

            return allGames;
        }

        public ILibraryMetadataProvider GetMetadataDownloader()
        {
            return null;
        }

        #endregion ILibraryPlugin
    }
}
