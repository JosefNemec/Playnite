using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.API.DesignData
{
    public class DesignPlayniteAPI : IPlayniteAPI
    {
        public IMainViewAPI MainView => throw new NotImplementedException();

        public IGameDatabaseAPI Database => throw new NotImplementedException();

        public IDialogsFactory Dialogs => throw new NotImplementedException();

        public IPlaynitePathsAPI Paths => throw new NotImplementedException();

        public INotificationsAPI Notifications { get; } = new DesignNotificationsAPI();

        public IPlayniteInfoAPI ApplicationInfo => throw new NotImplementedException();

        public IWebViewFactory WebViews => throw new NotImplementedException();

        public IResourceProvider Resources => throw new NotImplementedException();

        public ILogger CreateLogger(string name)
        {
            throw new NotImplementedException();
        }

        public ILogger CreateLogger()
        {
            throw new NotImplementedException();
        }

        public string ExpandGameVariables(Game game, string inputString)
        {
            throw new NotImplementedException();
        }

        public GameAction ExpandGameVariables(Game game, GameAction action)
        {
            throw new NotImplementedException();
        }

        public TConfig GetPluginConfiguration<TConfig>(IPlugin plugin) where TConfig : class
        {
            throw new NotImplementedException();
        }

        public string GetPluginUserDataPath(IPlugin plugin)
        {
            throw new NotImplementedException();
        }

        public TSettings LoadPluginSettings<TSettings>(IPlugin plugin) where TSettings : class
        {
            throw new NotImplementedException();
        }

        public void SavePluginSettings<TSettings>(IPlugin plugin, TSettings settings) where TSettings : class
        {
            throw new NotImplementedException();
        }
    }
}
