﻿using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Describes object providing Playnite API.
    /// </summary>
    public interface IPlayniteAPI
    {
        /// <summary>
        /// Gest main view API.
        /// </summary>
        IMainViewAPI MainView { get; }

        /// <summary>
        /// Gets database API.
        /// </summary>
        IGameDatabaseAPI Database { get; }

        /// <summary>
        /// Gets dialog API.
        /// </summary>
        IDialogsFactory Dialogs { get; }

        /// <summary>
        /// Gets paths API.
        /// </summary>
        IPlaynitePathsAPI Paths { get; }

        /// <summary>
        /// Gets notification API.
        /// </summary>
        INotificationsAPI Notifications { get; }

        /// <summary>
        /// Gets application info API.
        /// </summary>
        IPlayniteInfoAPI ApplicationInfo { get; }

        /// <summary>
        /// Gets web view API.
        /// </summary>
        IWebViewFactory WebViews { get; }

        /// <summary>
        /// Gets resources API.
        /// </summary>
        IResourceProvider Resources { get; }

        /// <summary>
        /// Gets path dedicated for plugins to store data.
        /// </summary>
        /// <param name="plugin">Source plugin.</param>
        /// <returns>Full directory path.</returns>
        string GetPluginUserDataPath(IPlugin plugin);

        /// <summary>
        /// Gets plugin configuration stored in plugin.cfg file.
        /// </summary>
        /// <typeparam name="TConfig">Plugin configuration type.</typeparam>
        /// <param name="plugin">Source plugin.</param>
        /// <returns>Plugin configuration.</returns>
        TConfig GetPluginConfiguration<TConfig>(IPlugin plugin) where TConfig : class;

        /// <summary>
        /// Gets plugin settings.
        /// </summary>
        /// <typeparam name="TSettings">Plugin settings type.</typeparam>
        /// <param name="plugin">Source plugin.</param>
        /// <returns>Plugin settings.</returns>
        TSettings LoadPluginSettings<TSettings>(IPlugin plugin) where TSettings : class;

        /// <summary>
        /// Saves plugin settings.
        /// </summary>
        /// <typeparam name="TSettings">Plugin settings type.</typeparam>
        /// <param name="plugin"></param>
        /// <param name="settings">Source plugin.</param>
        void SavePluginSettings<TSettings>(IPlugin plugin, TSettings settings) where TSettings : class;

        /// <summary>
        /// Expands dynamic game variables in specified string.
        /// </summary>
        /// <param name="game">Game to use dynamic variables from.</param>
        /// <param name="inputString">String containing dynamic variables.</param>
        /// <returns>String with replaces variables.</returns>
        string ExpandGameVariables(Game game, string inputString);

        /// <summary>
        /// Expands dynamic game variables in specified game action.
        /// </summary>
        /// <param name="game">Game to use dynamic variables from.</param>
        /// <param name="action">Game action to expand variables to.</param>
        /// <returns>Game action with expanded variables.</returns>
        GameAction ExpandGameVariables(Game game, GameAction action);

        /// <summary>
        /// Returns new instance of Playnite logger.
        /// </summary>
        /// <param name="name">Logger name.</param>
        /// <returns>Logger object.</returns>
        ILogger CreateLogger(string name);

        /// <summary>
        /// Creates new instance of Playnite logger with name of calling class.
        /// </summary>
        /// <returns>Logger object.</returns>
        ILogger CreateLogger();
    }
}
