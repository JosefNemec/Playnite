using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.API
{
    public class PlayniteAPI : IPlayniteAPI
    {
        private static ILogger logger = LogManager.GetLogger();

        private const string pluginSettingFileName = "config.json";

        public PlayniteAPI(            
            IGameDatabaseAPI databaseApi,
            IDialogsFactory dialogs,
            IMainViewAPI mainViewApi,
            IPlayniteInfoAPI infoApi,
            IPlaynitePathsAPI pathsApi,
            IWebViewFactory webViewFactory,
            IResourceProvider resources,
            INotificationsAPI notifications)
        {
            WebViews = webViewFactory;
            Paths = pathsApi;
            ApplicationInfo = infoApi;
            MainView = mainViewApi;
            Dialogs = dialogs;
            Database = databaseApi;
            Resources = resources;
            Notifications = notifications;
        }

        public IDialogsFactory Dialogs { get; }

        public IGameDatabaseAPI Database { get; }

        public IMainViewAPI MainView { get; set; }

        public IPlaynitePathsAPI Paths { get; }

        public IPlayniteInfoAPI ApplicationInfo { get; }

        public IWebViewFactory WebViews { get; }

        public IResourceProvider Resources { get; }

        public INotificationsAPI Notifications { get; }

        public string ExpandGameVariables(Game game, string inputString)
        {
            return game?.ExpandVariables(inputString);
        }

        public GameAction ExpandGameVariables(Game game, GameAction action)
        {
            return action?.ExpandVariables(game);
        }

        public ILogger CreateLogger(string name)
        {
            return new Logger(name);
        }

        public ILogger CreateLogger()
        {
            var className = (new StackFrame(1)).GetMethod().DeclaringType.Name;
            return CreateLogger(className);
        }

        public string GetPluginUserDataPath(IPlugin plugin)
        {
            var path = Path.Combine(PlaynitePaths.ExtensionsDataPath, plugin.Id.ToString());
            FileSystem.CreateDirectory(path);
            return path;
        }

        public TConfig GetPluginConfiguration<TConfig>(IPlugin plugin) where TConfig : class
        {
            var pluginDir = Path.GetDirectoryName(plugin.GetType().Assembly.Location);
            var pluginConfig = Path.Combine(pluginDir, "plugin.cfg");
            if (File.Exists(pluginConfig))
            {
                try
                {
                    return JsonConvert.DeserializeObject<TConfig>(File.ReadAllText(pluginConfig));
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to load plugin config: {pluginConfig}");
                }
            }

            return null;
        }

        public TSettings LoadPluginSettings<TSettings>(IPlugin plugin) where TSettings : class
        {
            var setFile = Path.Combine(GetPluginUserDataPath(plugin), pluginSettingFileName);
            if (!File.Exists(setFile))
            {
                return null;
            }

            var strConf = File.ReadAllText(setFile);
            return JsonConvert.DeserializeObject<TSettings>(strConf);

        }

        public void SavePluginSettings<TSettings>(IPlugin plugin, TSettings settings) where TSettings : class
        {
            var setDir = GetPluginUserDataPath(plugin);
            var setFile = Path.Combine(setDir, pluginSettingFileName);
            if (!Directory.Exists(setDir))
            {
                Directory.CreateDirectory(setDir);
            }

            var strConf = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(setFile, strConf);
        }
    }
}
