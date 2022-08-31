﻿using Playnite.SDK;
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

        public IUriHandlerAPI UriHandler => throw new NotImplementedException();

        public IPlayniteSettingsAPI ApplicationSettings => throw new NotImplementedException();

        public IAddons Addons => throw new NotImplementedException();

        public IEmulationAPI Emulation => throw new NotImplementedException();

        public void AddCustomElementSupport(Plugin source, AddCustomElementSupportArgs args)
        {
            throw new NotImplementedException();
        }

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

        public void StartGame(Guid gameId)
        {
            throw new NotImplementedException();
        }

        public void InstallGame(Guid gameId)
        {
            throw new NotImplementedException();
        }

        public void UninstallGame(Guid gameId)
        {
            throw new NotImplementedException();
        }

        public void AddSettingsSupport(Plugin source, AddSettingsSupportArgs args)
        {
            throw new NotImplementedException();
        }

        public void AddConvertersSupport(Plugin source, AddConvertersSupportArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
