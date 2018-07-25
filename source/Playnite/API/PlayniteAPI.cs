﻿using Playnite.Database;
using Playnite.Providers;
using Playnite.Scripting;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.API
{
    public class PlayniteAPI : ObservableObject, IDisposable, IPlayniteAPI
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private GameDatabase database;
        private GameControllerFactory controllers;

        private List<PlayniteScript> scripts;
        private List<Plugin> plugins;

        public bool HasExportedFunctions
        {
            get => ExportedFunctions?.Any() == true;
        }

        private List<ScriptFunctionExport> scriptFunctions;
        public List<ScriptFunctionExport> ScriptFunctions
        {
            get => scriptFunctions;
            set
            {
                scriptFunctions = value;
                OnPropertyChanged("ExportedFunctions");
                OnPropertyChanged("HasExportedFunctions");
            }
        }

        private List<ExtensionFunction> pluginFunctions;
        public List<ExtensionFunction> PluginFunctions
        {
            get => pluginFunctions;
            set
            {
                pluginFunctions = value;
                OnPropertyChanged("ExportedFunctions");
                OnPropertyChanged("HasExportedFunctions");
            }
        }

        public List<ExtensionFunction> ExportedFunctions
        {
            get
            {
                var funcs = new List<ExtensionFunction>();
                if (ScriptFunctions?.Any() == true)
                {
                    funcs.AddRange(ScriptFunctions);
                }

                if (PluginFunctions?.Any() == true)
                {
                    funcs.AddRange(PluginFunctions);
                }

                return funcs;
            }
        }

        public IDialogsFactory Dialogs
        {
            get;
        }

        public IGameDatabaseAPI Database
        {
            get;
        }

        public IMainViewAPI MainView
        {
            get; set;
        }

        public PlayniteAPI(GameDatabase database, GameControllerFactory controllers, IDialogsFactory dialogs, IMainViewAPI mainViewApi)
        {
            this.database = database;
            this.controllers = controllers;
            MainView = mainViewApi;
            Dialogs = dialogs;
            Database = new DatabaseAPI(database);
            LoadScripts();
            LoadPlugins();
            controllers.Installed += Controllers_Installed;
            controllers.Starting += Controllers_Starting;
            controllers.Started += Controllers_Started;
            controllers.Stopped += Controllers_Stopped;
            controllers.Uninstalled += Controllers_Uninstalled;
            database.DatabaseOpened += Database_DatabaseOpened;
        }

        public void Dispose()
        {
            DisposeScripts();
            DisposePlugins();
            controllers.Installed -= Controllers_Installed;
            controllers.Starting -= Controllers_Starting;
            controllers.Started -= Controllers_Installed;
            controllers.Stopped -= Controllers_Installed;
            controllers.Uninstalled -= Controllers_Installed;
            database.DatabaseOpened -= Database_DatabaseOpened;
        }

        private void DisposePlugins()
        {
            if (plugins == null)
            {
                return;
            }

            foreach (var plugin in plugins)
            {
                plugin.Dispose();
            }

            plugins = null;
            PluginFunctions = null;
        }

        private void DisposeScripts()
        {
            if (scripts == null)
            {
                return;
            }

            foreach (var script in scripts)
            {
                script.Dispose();                
            }

            scripts = null;
            ScriptFunctions = null;
        }

        public bool LoadScripts()
        {
            var allSuccess = true;
            DisposeScripts();
            scripts = new List<PlayniteScript>();
            foreach (var path in Scripts.GetScriptFiles())
            {
                PlayniteScript script = null;

                try
                {
                    script = PlayniteScript.FromFile(path);
                    if (script == null)
                    {
                        continue;
                    }
                }
                catch (Exception e)
                {
                    allSuccess = false;
                    logger.Error(e, $"Failed to load script file {path}");
                    Dialogs.ShowMessage(
                        $"Failed to load script file {Path.GetFileName(path)}:\n\n" + e.Message, "Script error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                logger.Info($"Loaded script extension {path}");
                script.SetVariable("PlayniteApi", this);
                scripts.Add(script);                
            }

            ScriptFunctions = scripts.Where(a => a.FunctionExports?.Any() == true).SelectMany(a => a.FunctionExports).ToList();
            return allSuccess;
        }

        public void LoadPlugins()
        {
            DisposePlugins();
            plugins = new List<Plugin>();
            foreach (var path in Plugins.Plugins.GetPluginFiles())
            {
                if (Path.GetFileNameWithoutExtension(path).Equals("PlayniteSDK", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                List<Plugin> plugin = null;

                try
                {
                    plugin = Plugins.Plugins.LoadPlugin(path, this);
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e.InnerException, $"Failed to load plugin file {path}");
                    Dialogs.ShowMessage(
                        $"Failed to load plugin file {Path.GetFileName(path)}:\n\n" + e.Message, "Plugin error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                logger.Info($"Loaded plugin extension {path}");
                plugins.AddRange(plugin);
            }

            PluginFunctions = plugins.Where(a => a.GetFunctions()?.Any() == true).SelectMany(a => a.GetFunctions()).ToList();
        }

        public void InvokeExtension(ExtensionFunction function)
        {
            try
            {
                logger.Debug($"Invoking extension function {function}");
                function.Invoke();
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to execute extension function.");
                Dialogs.ShowMessage(
                     $"Failed to execute extension function:\n\n" + e.Message, "Script error",
                     MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Controllers_Uninstalled(object sender, GameControllerEventArgs args)
        {
            foreach (var script in scripts)
            {
                try
                {
                    script.OnGameUninstalled(args.Controller.Game);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnGameUninstalled method from {script.Name} script.");
                }
            }

            foreach (var plugin in plugins)
            {
                try
                {
                    plugin.OnGameUninstalled(args.Controller.Game);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnGameUninstalled method from {plugin.Properties.PluginName} plugin.");
                }
            }
        }

        private void Controllers_Stopped(object sender, GameControllerEventArgs args)
        {
            foreach (var script in scripts)
            {
                try
                {
                    script.OnGameStopped(database.GetGame(args.Controller.Game.Id), args.EllapsedTime);
                }
                    catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnGameStopped method from {script.Name} script.");
                }
            }

            foreach (var plugin in plugins)
            {
                try
                {
                    plugin.OnGameStopped(database.GetGame(args.Controller.Game.Id), args.EllapsedTime);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnGameStopped method from {plugin.Properties.PluginName} plugin.");
                }
            }
        }

        private void Controllers_Starting(object sender, GameControllerEventArgs args)
        {
            foreach (var script in scripts)
            {
                try
                {
                    script.OnGameStarting(database.GetGame(args.Controller.Game.Id));
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnGameStarting method from {script.Name} script.");
                }
            }

            foreach (var plugin in plugins)
            {
                try
                {
                    plugin.OnGameStarting(database.GetGame(args.Controller.Game.Id));
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnGameStarting method from {plugin.Properties.PluginName} plugin.");
                }
            }
        }

        private void Controllers_Started(object sender, GameControllerEventArgs args)
        {
            foreach (var script in scripts)
            {
                try
                {
                    script.OnGameStarted(database.GetGame(args.Controller.Game.Id));
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnGameStarted method from {script.Name} script.");
                }
            }

            foreach (var plugin in plugins)
            {
                try
                {
                    plugin.OnGameStarted(database.GetGame(args.Controller.Game.Id));
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnGameStarted method from {plugin.Properties.PluginName} plugin.");
                }
            }
        }

        private void Controllers_Installed(object sender, GameControllerEventArgs args)
        {
            foreach (var script in scripts)
            {
                try
                {                    
                    script.OnGameInstalled(database.GetGame(args.Controller.Game.Id));
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnGameInstalled method from {script.Name} script.");
                }
            }

            foreach (var plugin in plugins)
            {
                try
                {
                    plugin.OnGameInstalled(database.GetGame(args.Controller.Game.Id));
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnGameInstalled method from {plugin.Properties.PluginName} plugin.");
                }
            }
        }

        private void Database_DatabaseOpened(object sender, EventArgs args)
        {
            foreach (var script in scripts)
            {
                try
                {
                    script?.OnScriptLoaded();
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnScriptLoaded method from {script.Name} script.");
                    continue;
                }
            }

            foreach (var plugin in plugins)
            {
                try
                {
                    plugin.OnLoaded();
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnLoaded method from {plugin.Properties.PluginName} plugin.");
                }
            }
        }

        public string ResolveGameVariables(Game game, string toResolve)
        {
            return game?.ResolveVariables(toResolve);
        }

        public ILogger CreateLogger(string name)
        {
            return new Logger(name);
        }
    }
}
