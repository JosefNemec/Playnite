using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Represents information about plugin.
    /// </summary>
    public class PluginProperties
    {
        /// <summary>
        /// Gets or sets author's name.
        /// </summary>
        public string Author
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets plugin name.
        /// </summary>
        public string PluginName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets plugin version.
        /// </summary>
        public string Version
        {
            get; set;
        }

        /// <summary>
        /// Creates new instance of PluginProperties.
        /// </summary>
        /// <param name="pluginName">Plugin name.</param>
        /// <param name="author">Plugin author.</param>
        /// <param name="version">Plugin version.</param>
        public PluginProperties(string pluginName, string author, string version)
        {
            PluginName = pluginName;
            Author = author;
            Version = version;
        }
    }

    /// <summary>
    /// Represents Playnite plugin.
    /// </summary>
    public abstract class Plugin
    {
        /// <summary>
        /// Gets plugin properties.
        /// </summary>
        public PluginProperties Properties
        {
            get => GetPluginProperties();
        }

        /// <summary>
        /// Gets Playnite API.
        /// </summary>
        public IPlayniteAPI PlayniteApi
        {
            get; private set;
        }

        /// <summary>
        /// Creates new instance of Plugin.
        /// </summary>
        /// <param name="api">PLaynite API provider.</param>
        public Plugin(IPlayniteAPI api)
        {
            PlayniteApi = api;
        }

        /// <summary>
        /// Returns plugin properties.
        /// </summary>
        /// <returns></returns>
        public abstract PluginProperties GetPluginProperties();

        /// <summary>
        /// Returns list of plugin functions.
        /// </summary>
        /// <returns></returns>
        public virtual List<ExtensionFunction> GetFunctions()
        {
            return null;
        }
        
        /// <summary>
        /// Disposes plugin resources.
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Called when plugin is being loaded.
        /// </summary>
        public virtual void OnLoaded()
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
    }        
}
