using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    public class PluginProperties
    {
        public string Author
        {
            get; set;
        }

        public string PluginName
        {
            get; set;
        }

        public string Version
        {
            get; set;
        }

        public PluginProperties(string pluginName, string author, string version)
        {
            PluginName = pluginName;
            Author = author;
            Version = version;
        }
    }

    public interface IPlugin : IDisposable
    {
        PluginProperties Properties
        {
            get;
        }

        int CompatibilityVersion
        {
            get;
        }

        List<ExtensionFunction> GetFunctions();

        void OnLoaded();
        void OnGameStarted(Game game);
        void OnGameStopped(Game game, long ellapsedSeconds);
        void OnGameInstalled(Game game);
        void OnGameUninstalled(Game game);
    }

    public abstract class Plugin : IPlugin
    {
        public PluginProperties Properties
        {
            get => GetPluginProperties();
        }

        public int CompatibilityVersion
        {
            get => GetCompatibilityVersion();
        }

        public IPlayniteAPI PlayniteApi
        {
            get; private set;
        }

        public Plugin(IPlayniteAPI api)
        {
            PlayniteApi = api;
        }

        public abstract PluginProperties GetPluginProperties();
        public abstract int GetCompatibilityVersion();

        public virtual List<ExtensionFunction> GetFunctions()
        {
            return null;
        }

        public virtual void Dispose()
        {
        }

        public virtual void OnLoaded()
        {
        }

        public virtual void OnGameStarted(Game game)
        {
        }

        public virtual void OnGameStopped(Game game, long ellapsedSeconds)
        {
        }

        public virtual void OnGameInstalled(Game game)
        {
        }

        public virtual void OnGameUninstalled(Game game)
        {
        }
    }        
}
