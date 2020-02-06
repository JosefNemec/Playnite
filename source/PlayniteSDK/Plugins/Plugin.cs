using Newtonsoft.Json;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Playnite.SDK.Plugins
{
    /// <summary>
    /// Represents base Playnite plugin.
    /// </summary>
    public abstract class Plugin : IDisposable, IIdentifiable
    {
        private const string pluginSettingFileName = "config.json";

        /// <summary>
        /// Gets instance of runtime <see cref="IPlayniteAPI"/>.
        /// </summary>
        public readonly IPlayniteAPI PlayniteApi;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract Guid Id { get; }

        /// <summary>
        /// Creates new instance of <see cref="Plugin"/>.
        /// </summary>
        /// <param name="playniteAPI">Instance of Playnite API to be injected.</param>
        public Plugin(IPlayniteAPI playniteAPI)
        {
            PlayniteApi = playniteAPI;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Gets plugin settings or null if plugin doesn't provide any settings.
        /// </summary>
        public virtual ISettings GetSettings(bool firstRunSettings)
        {
            return null;
        }

        /// <summary>
        /// Gets plugin settings view or null if plugin doesn't provide settings view.
        /// </summary>
        public virtual UserControl GetSettingsView(bool firstRunView)
        {
            return null;
        }

        /// <summary>
        /// Returns list of plugin functions.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<ExtensionFunction> GetFunctions()
        {
            return null;
        }

        /// <summary>
        /// Called before game is started.
        /// </summary>
        /// <param name="game">Game that will be started.</param>
        public virtual void OnGameStarting(Game game)
        {
        }

        /// <summary>
        /// Called when game has started.
        /// </summary>
        /// <param name="game">Game that started.</param>
        public virtual void OnGameStarted(Game game)
        {
        }

        /// <summary>
        /// Called when game stopped running.
        /// </summary>
        /// <param name="game">Game that stopped running.</param>
        /// <param name="ellapsedSeconds">Time in seconds of how long the game was running.</param>
        public virtual void OnGameStopped(Game game, long ellapsedSeconds)
        {
        }

        /// <summary>
        /// Called when game has been installed.
        /// </summary>
        /// <param name="game">Game that's been installed.</param>
        public virtual void OnGameInstalled(Game game)
        {
        }

        /// <summary>
        /// Called when game has been uninstalled.
        /// </summary>
        /// <param name="game">Game that's been uninstalled.</param>
        public virtual void OnGameUninstalled(Game game)
        {
        }

        /// <summary>
        /// Called when game selection changed.
        /// </summary>
        /// <param name="args"></param>
        public virtual void OnGameSelected(GameSelectionEventArgs args)
        {
        }

        /// <summary>
        /// Called when appliaction is started and initialized.
        /// </summary>
        public virtual void OnApplicationStarted()
        {
        }

        /// <summary>
        /// Called when appliaction is stutting down.
        /// </summary>
        public virtual void OnApplicationStopped()
        {
        }

        /// <summary>
        /// Called library update has been finished.
        /// </summary>
        public virtual void OnLibraryUpdated()
        {
        }

        /// <summary>
        /// Gets path dedicated for plugins to store data.
        /// </summary>
        /// <returns>Full directory path.</returns>
        public string GetPluginUserDataPath()
        {
            var path = Path.Combine(PlayniteApi.Paths.ExtensionsDataPath, Id.ToString());
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        /// <summary>
        /// Gets plugin configuration stored in plugin.cfg file.
        /// </summary>
        /// <typeparam name="TConfig">Plugin configuration type.</typeparam>
        /// <returns>Plugin configuration.</returns>
        public TConfig GetPluginConfiguration<TConfig>() where TConfig : class
        {
            var pluginDir = Path.GetDirectoryName(GetType().Assembly.Location);
            var pluginConfig = Path.Combine(pluginDir, "plugin.cfg");
            if (File.Exists(pluginConfig))
            {
                return JsonConvert.DeserializeObject<TConfig>(File.ReadAllText(pluginConfig));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets plugin settings.
        /// </summary>
        /// <typeparam name="TSettings">Plugin settings type.</typeparam>
        /// <returns>Plugin settings.</returns>
        public TSettings LoadPluginSettings<TSettings>() where TSettings : class
        {
            var setFile = Path.Combine(GetPluginUserDataPath(), pluginSettingFileName);
            if (File.Exists(setFile))
            {
                var strConf = File.ReadAllText(setFile);
                return JsonConvert.DeserializeObject<TSettings>(strConf);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Saves plugin settings.
        /// </summary>
        /// <typeparam name="TSettings">Plugin settings type.</typeparam>
        /// <param name="settings">Source plugin.</param>
        public void SavePluginSettings<TSettings>(TSettings settings) where TSettings : class
        {
            var setDir = GetPluginUserDataPath();
            var setFile = Path.Combine(setDir, pluginSettingFileName);
            if (!Directory.Exists(setDir))
            {
                Directory.CreateDirectory(setDir);
            }

            var strConf = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(setFile, strConf);
        }

        /// <summary>
        /// Opens plugin's settings view. Only works in Desktop application mode!
        /// </summary>
        /// <returns>True if user saved any changes, False if dialog was canceled.</returns>
        public bool OpenSettingsView()
        {
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Fullscreen)
            {
                return false;
            }

            return PlayniteApi.MainView.OpenPluginSettings(Id);
        }
    }
}
