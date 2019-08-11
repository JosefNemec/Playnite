﻿using Playnite.API;
using Playnite.Database;
using Playnite.Controllers;
using Playnite.Scripting;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Playnite.Common;

namespace Playnite.Plugins
{
    public class LoadedPlugin
    {
        public Plugin Plugin { get; }
        public ExtensionDescription Description { get; }

        public LoadedPlugin(Plugin plugin, ExtensionDescription description)
        {
            Plugin = plugin;
            Description = description;
        }
    }

    public class ExtensionFactory : ObservableObject, IDisposable
    {
        private static ILogger logger = LogManager.GetLogger();
        private IGameDatabase database;
        private GameControllerFactory controllers;

        public const string ExtensionManifestFileName = "extension.yaml";

        public Dictionary<Guid, LoadedPlugin> Plugins
        {
            get;
        } = new Dictionary<Guid, LoadedPlugin>();

        public IEnumerable<LibraryPlugin> LibraryPlugins
        {
            get => Plugins.Where(a => a.Value.Description.Type == ExtensionType.GameLibrary).Select(a => (LibraryPlugin)a.Value.Plugin);
        }

        public  List<PlayniteScript> Scripts
        {
            get;
        } =  new List<PlayniteScript>();

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
                OnPropertyChanged();
                OnPropertyChanged(nameof(ExportedFunctions));
            }
        }

        private List<ExtensionFunction> pluginFunctions;
        public List<ExtensionFunction> PluginFunctions
        {
            get => pluginFunctions;
            set
            {
                pluginFunctions = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ExportedFunctions));
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

        public ExtensionFactory(IGameDatabase database, GameControllerFactory controllers)
        {
            this.database = database;
            this.controllers = controllers;
            controllers.Installed += Controllers_Installed;
            controllers.Starting += Controllers_Starting;
            controllers.Started += Controllers_Started;
            controllers.Stopped += Controllers_Stopped;
            controllers.Uninstalled += Controllers_Uninstalled;
        }

        public void Dispose()
        {
            DisposePlugins();
            DisposeScripts();
            controllers.Installed -= Controllers_Installed;
            controllers.Starting -= Controllers_Starting;
            controllers.Started -= Controllers_Installed;
            controllers.Stopped -= Controllers_Installed;
            controllers.Uninstalled -= Controllers_Installed;
        }

        private void DisposeScripts()
        {
            if (Scripts?.Any() == true)
            {
                foreach (var script in Scripts)
                {
                    try
                    {
                        script.Dispose();
                    }
                        catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, $"Failed to dispose script {script.Name}");
                    }
            }
            }

            Scripts?.Clear();
            ScriptFunctions = null;
        }

        private void DisposePlugins()
        {
            if (Plugins?.Any() == true)
            {
                foreach (var provider in Plugins.Keys)
                {
                    try
                    {
                        Plugins[provider].Plugin.Dispose();
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, $"Failed to dispose plugin {provider}");
                    }
                }
            }

            Plugins?.Clear();
            PluginFunctions = null;
        }

        public static void CreatePluginFolders()
        {
            FileSystem.CreateDirectory(PlaynitePaths.ExtensionsDataPath);
            FileSystem.CreateDirectory(PlaynitePaths.ExtensionsProgramPath);
            if (!PlayniteSettings.IsPortable)
            {
                FileSystem.CreateDirectory(PlaynitePaths.ExtensionsUserDataPath);
            }
        }

        public List<ExtensionDescription> GetExtensionDescriptors()
        {
            var descs = new List<ExtensionDescription>();
            foreach (var file in GetExtensionDescriptorFiles())
            {
                try
                {
                    descs.Add(ExtensionDescription.FromFile(file));
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e.InnerException, $"Failed to parse plugin description: {file}");
                    continue;
                }
            }

            return descs;
        }

        private List<string> GetExtensionDescriptorFiles()
        {
            var added = new List<string>();
            var plugins = new List<string>();

            if (!PlayniteSettings.IsPortable && Directory.Exists(PlaynitePaths.ExtensionsUserDataPath))
            {
                var enumerator = new SafeFileEnumerator(PlaynitePaths.ExtensionsUserDataPath, ExtensionManifestFileName, SearchOption.AllDirectories);
                foreach (var desc in enumerator)
                {       
                    plugins.Add(desc.FullName);
                    var info = new FileInfo(desc.FullName);
                    added.Add(info.Directory.Name);
                }
            }

            if (Directory.Exists(PlaynitePaths.ExtensionsProgramPath))
            {
                var enumerator = new SafeFileEnumerator(PlaynitePaths.ExtensionsProgramPath, ExtensionManifestFileName, SearchOption.AllDirectories);
                foreach (var desc in enumerator)
                {
                    plugins.Add(desc.FullName);
                    var info = new FileInfo(desc.FullName);
                    added.Add(info.Directory.Name);
                }
            }

            return plugins;
        }

        private bool VerifySdkReference(Assembly asm)
        {
            var sdkReference = asm.GetReferencedAssemblies().FirstOrDefault(a => a.Name == "Playnite.SDK");
            if (sdkReference == null)
            {
                logger.Error($"Assembly doesn't reference Playnite SDK.");
                return false;
            }

            if (sdkReference.Version.Major != SDK.Version.SDKVersion.Major)
            {
                logger.Error($"Plugin doesn't support this version of Playnite SDK.");
                return false;
            }

            return true;
        }

        public bool LoadScripts(IPlayniteAPI injectingApi, List<string> ignoreList)
        {
            var allSuccess = true;
            DisposeScripts();
            var functions = new List<ScriptFunctionExport>();

            foreach (ScriptExtensionDescription desc in GetExtensionDescriptors().Where(a => a.Type == ExtensionType.Script && !ignoreList.Contains(a.FolderName)))
            {
                PlayniteScript script = null;
                var scriptPath = Path.Combine(Path.GetDirectoryName(desc.DescriptionPath), desc.Module);
                if (!File.Exists(scriptPath))
                {
                    logger.Error($"Cannot load script extension, {scriptPath} not found.");
                    continue;
                }

                try
                {
                    script = PlayniteScript.FromFile(scriptPath);
                    if (script == null)
                    {
                        continue;
                    }

                    script.SetVariable("PlayniteApi", injectingApi);
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    allSuccess = false;
                    logger.Error(e, $"Failed to load script file {scriptPath}");
                    continue;
                }                
                
                Scripts.Add(script);
                logger.Info($"Loaded script extension: {scriptPath}");

                if (desc.Functions?.Any() == true)
                {
                    functions.AddRange(desc.Functions.Select(a => new ScriptFunctionExport(a.Description, a.FunctionName, script)));
                }
            }

            ScriptFunctions = functions;
            return allSuccess;
        }

        public void LoadPlugins(IPlayniteAPI injectingApi, List<string> ignoreList)
        {
            DisposePlugins();
            var funcs = new List<ExtensionFunction>();
            foreach (var desc in GetExtensionDescriptors().Where(a => a.Type != ExtensionType.Script && ignoreList?.Contains(a.FolderName) != true))
            {
                try
                {
                    var plugins = LoadPlugins(desc, injectingApi);
                    foreach (var plugin in plugins)
                    {                        
                        Plugins.Add(plugin.Id, new LoadedPlugin(plugin, desc));                        
                        var plugFunc = plugin.GetFunctions();
                        if (plugFunc?.Any() == true)
                        {
                            funcs.AddRange(plugFunc);
                        }

                        logger.Info($"Loaded plugin: {desc.Name}");
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e.InnerException, $"Failed to load plugin: {desc.Name}");
                }
            }

            PluginFunctions = funcs;
        }
        
        public IEnumerable<Plugin> LoadPlugins(ExtensionDescription descriptor, IPlayniteAPI injectingApi)
        {
            var asmPath = Path.Combine(Path.GetDirectoryName(descriptor.DescriptionPath), descriptor.Module);
            var asmName = AssemblyName.GetAssemblyName(asmPath);
            var assembly = Assembly.Load(asmName);
            if (VerifySdkReference(assembly))
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsInterface || type.IsAbstract)
                    {
                        continue;
                    }
                    else
                    {
                        if (typeof(Plugin).IsAssignableFrom(type))
                        {
                            yield return (Plugin)Activator.CreateInstance(type, new object[] { injectingApi });
                        }
                    }
                }
            }
        }

        public bool InvokeExtension(ExtensionFunction function, out string error)
        {
            try
            {
                logger.Debug($"Invoking extension function {function}");
                function.Invoke();
                error = null;
                return true;
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, $"Failed to execute extension function.");
                error = e.Message;
                return false;
            }
        }

        private void Controllers_Uninstalled(object sender, GameControllerEventArgs args)
        {
            foreach (var script in Scripts)
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

            foreach (var plugin in Plugins.Values)
            {
                try
                {
                    plugin.Plugin.OnGameUninstalled(args.Controller.Game);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnGameUninstalled method from {plugin.Description.Name} plugin.");
                }
            }
        }

        private void Controllers_Stopped(object sender, GameControllerEventArgs args)
        {
            foreach (var script in Scripts)
            {
                try
                {
                    script.OnGameStopped(database.Games[args.Controller.Game.Id], args.EllapsedTime);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnGameStopped method from {script.Name} script.");
                }
            }

            foreach (var plugin in Plugins.Values)
            {
                try
                {
                    plugin.Plugin.OnGameStopped(database.Games[args.Controller.Game.Id], args.EllapsedTime);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnGameStopped method from {plugin.Description.Name} plugin.");
                }
            }
        }

        private void Controllers_Starting(object sender, GameControllerEventArgs args)
        {
            foreach (var script in Scripts)
            {
                try
                {
                    script.OnGameStarting(database.Games[args.Controller.Game.Id]);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnGameStarting method from {script.Name} script.");
                }
            }

            foreach (var plugin in Plugins.Values)
            {
                try
                {
                    plugin.Plugin.OnGameStarting(database.Games[args.Controller.Game.Id]);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnGameStarting method from {plugin.Description.Name} plugin.");
                }
            }
        }

        private void Controllers_Started(object sender, GameControllerEventArgs args)
        {
            foreach (var script in Scripts)
            {
                try
                {
                    script.OnGameStarted(database.Games[args.Controller.Game.Id]);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnGameStarted method from {script.Name} script.");
                }
            }

            foreach (var plugin in Plugins.Values)
            {
                try
                {
                    plugin.Plugin.OnGameStarted(database.Games[args.Controller.Game.Id]);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnGameStarted method from {plugin.Description.Name} plugin.");
                }
            }
        }

        private void Controllers_Installed(object sender, GameControllerEventArgs args)
        {
            foreach (var script in Scripts)
            {
                try
                {
                    script.OnGameInstalled(database.Games[args.Controller.Game.Id]);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnGameInstalled method from {script.Name} script.");
                }
            }

            foreach (var plugin in Plugins.Values)
            {
                try
                {
                    plugin.Plugin.OnGameInstalled(database.Games[args.Controller.Game.Id]);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnGameInstalled method from {plugin.Description.Name} plugin.");
                }
            }
        }

        public void NotifiyOnApplicationStarted()
        {
            foreach (var script in Scripts)
            {
                try
                {
                    script.OnApplicationStarted();
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnScriptLoaded method from {script.Name} script.");
                    continue;
                }
            }

            foreach (var plugin in Plugins.Values)
            {
                try
                {
                    plugin.Plugin.OnApplicationStarted();
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to load execute OnLoaded method from {plugin.Description.Name} plugin.");
                }
            }
        }
    }
}
