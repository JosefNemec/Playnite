using NLog;
using Playnite.Database;
using Playnite.Providers;
using Playnite.Scripting;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.API
{
    public class PlayniteAPI : ObservableObject, IDisposable, IPlayniteAPI
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private GameDatabase database;
        private GameControllerFactory controllers;
        private List<PlayniteScript> scripts;

        private List<ScriptFunctionExport> exportedFunctions;
        public List<ScriptFunctionExport> ExportedFunctions
        {
            get => exportedFunctions;
            set
            {
                exportedFunctions = value;
                OnPropertyChanged("ExportedFunctions");
            }
        }

        public IDialogsFactory Dialogs
        {
            get;
        }

        public IGameDataseAPI Database
        {
            get;
        }

        public PlayniteAPI(GameDatabase database, GameControllerFactory controllers, IDialogsFactory dialogs)
        {
            this.database = database;
            this.controllers = controllers;
            Dialogs = dialogs;
            Database = new DatabaseAPI(database);
            LoadScripts();
            controllers.Installed += Controllers_Installed;
            controllers.Started += Controllers_Started;
            controllers.Stopped += Controllers_Stopped;
            controllers.Uninstalled += Controllers_Uninstalled;
            database.DatabaseOpened += Database_DatabaseOpened;
        }

        public void Dispose()
        {
            DisposeScripts();
            controllers.Installed -= Controllers_Installed;
            controllers.Started -= Controllers_Installed;
            controllers.Stopped -= Controllers_Installed;
            controllers.Uninstalled -= Controllers_Installed;
            database.DatabaseOpened -= Database_DatabaseOpened;
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
            ExportedFunctions = null;
        }

        public void LoadScripts()
        {
            DisposeScripts();
            scripts = new List<PlayniteScript>();
            foreach (var path in Scripts.GetScriptFiles())
            {
                PlayniteScript script = null;

                try
                {
                    script = PlayniteScript.FromFile(path);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load script file {path}");
                    Dialogs.ShowMessage(
                        $"Failed to load script file {path}:\n\n" + e.Message, "Script error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                script.SetVariable("PlayniteApi", this);
                scripts.Add(script);                
            }

            ExportedFunctions = scripts.Where(a => a.FunctionExports?.Any() == true).SelectMany(a => a.FunctionExports).ToList();
        }

        public void InvokeExtension(IExtensionFunction function)
        {
            try
            {
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
        }

        private void Controllers_Stopped(object sender, GameControllerEventArgs args)
        {
            foreach (var script in scripts)
            {
                try
                {
                    script.OnGameStopped(args.Controller.Game, args.ElapsedTime);
                }
                    catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnGameStopped method from {script.Name} script.");
                }
            }
        }

        private void Controllers_Started(object sender, GameControllerEventArgs args)
        {
            foreach (var script in scripts)
            {
                try
                {
                    script.OnGameStarted(args.Controller.Game);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnGameStarted method from {script.Name} script.");
                }
            }
        }

        private void Controllers_Installed(object sender, GameControllerEventArgs args)
        {
            foreach (var script in scripts)
            {
                try
                {
                    script.OnGameInstalled(args.Controller.Game);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnGameInstalled method from {script.Name} script.");
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
        }
    }
}
