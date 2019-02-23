using Playnite.API;
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

namespace Playnite.Plugins
{
    public class LoadedLibraryPlugin
    {
        public ILibraryPlugin Plugin { get; set; }
        public ExtensionDescription Description { get; set; }

        public LoadedLibraryPlugin(ILibraryPlugin plugin, ExtensionDescription description)
        {
            Plugin = plugin;
            Description = description;
        }
    }

    public class LoadedGenericPlugin
    {
        public IGenericPlugin Plugin { get; set; }
        public ExtensionDescription Description { get; set; }

        public LoadedGenericPlugin(IGenericPlugin plugin, ExtensionDescription description)
        {
            Plugin = plugin;
            Description = description;
        }
    }

    public class ExtensionFactory : ObservableObject, IDisposable
    {
        private static ILogger logger = LogManager.GetLogger();
        private GameDatabase database;
        private GameControllerFactory controllers;

        public const string ExtensionManifestFileName = "extension.yaml";

        public Dictionary<Guid, LoadedLibraryPlugin> LibraryPlugins
        {
            get;
        } = new Dictionary<Guid, LoadedLibraryPlugin>();

        public Dictionary<Guid, LoadedGenericPlugin> GenericPlugins
        {
            get;
        } = new Dictionary<Guid, LoadedGenericPlugin>();
        
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

        public ExtensionFactory(GameDatabase database, GameControllerFactory controllers)
        {
            this.database = database;
            this.controllers = controllers;
            controllers.Installed += Controllers_Installed;
            controllers.Starting += Controllers_Starting;
            controllers.Started += Controllers_Started;
            controllers.Stopped += Controllers_Stopped;
            controllers.Uninstalled += Controllers_Uninstalled;
            database.DatabaseOpened += Database_DatabaseOpened;
        }

        public void Dispose()
        {
            DisposeLibraryPlugins();
            DisposeGenericPlugins();
            DisposeScripts();
            controllers.Installed -= Controllers_Installed;
            controllers.Starting -= Controllers_Starting;
            controllers.Started -= Controllers_Installed;
            controllers.Stopped -= Controllers_Installed;
            controllers.Uninstalled -= Controllers_Installed;
            database.DatabaseOpened -= Database_DatabaseOpened;
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
                        logger.Error(e, $"Failed to dispose library plugin {script.Name}");
                    }
            }
            }

            Scripts?.Clear();
            ScriptFunctions = null;
        }

        private void DisposeLibraryPlugins()
        {
            if (LibraryPlugins?.Any() == true)
            {
                foreach (var provider in LibraryPlugins.Keys)
                {
                    try
                    {
                        LibraryPlugins[provider].Plugin.Dispose();
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, $"Failed to dispose library plugin {provider}");
                    }
                }
            }

            LibraryPlugins?.Clear();
        }

        private void DisposeGenericPlugins()
        {
            if (GenericPlugins?.Any() == true)
            {
                foreach (var provider in GenericPlugins.Keys)
                {
                    try
                    {
                        GenericPlugins[provider].Plugin.Dispose();
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, $"Failed to dispose generic plugin {provider}");
                    }
                }
            }

            GenericPlugins?.Clear();
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

        private void VerifySdkReference(Assembly asm)
        {
            var sdkReference = asm.GetReferencedAssemblies().FirstOrDefault(a => a.Name == "Playnite.SDK");
            if (sdkReference == null)
            {
                throw new Exception($"Assembly doesn't reference Playnite SDK.");
            }

            if (sdkReference.Version.Major != SDK.Version.SDKVersion.Major)
            {
                throw new Exception($"Plugin doesn't support this version of Playnite SDK.");
            }
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

        public void LoadLibraryPlugins(IPlayniteAPI injectingApi, List<string> ignoreList)
        {
            DisposeLibraryPlugins();
            foreach (var desc in GetExtensionDescriptors().Where(a => a.Type == ExtensionType.GameLibrary && ignoreList?.Contains(a.FolderName) != true))
            {
                try
                {
                    var plugin =  LoadPlugin<ILibraryPlugin>(desc, injectingApi);
                    LibraryPlugins.Add(plugin.Id, new LoadedLibraryPlugin(plugin, desc));
                    logger.Info($"Loaded library plugin: {desc.Name}");
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e.InnerException, $"Failed to load library plugin: {desc.Name}");
                }
            }
        }

        public void LoadGenericPlugins(IPlayniteAPI injectingApi, List<string> ignoreList)
        {
            DisposeGenericPlugins();
            var funcs = new List<ExtensionFunction>();
            foreach (var desc in GetExtensionDescriptors().Where(a => a.Type == ExtensionType.GenericPlugin && ignoreList?.Contains(a.FolderName) != true))
            {
                try
                {
                    var plugin = LoadPlugin<IGenericPlugin>(desc, injectingApi);
                    GenericPlugins.Add(plugin.Id, new LoadedGenericPlugin(plugin, desc));
                    var plugFunc = plugin.GetFunctions();
                    if (plugFunc?.Any() == true)
                    {
                        funcs.AddRange(plugFunc);
                    }

                    logger.Info($"Loaded generic plugin: {desc.Name}");
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e.InnerException, $"Failed to load generic plugin: {desc.Name}");
                }
            }

            PluginFunctions = funcs;
        }

        // TODO: support multiple plugins from one assembly (not sure if it's worth it)
        public TPlugin LoadPlugin<TPlugin>(ExtensionDescription descriptor, IPlayniteAPI injectingApi) where TPlugin : IPlugin
        {
            var plugins = new List<TPlugin>();
            var asmPath = Path.Combine(Path.GetDirectoryName(descriptor.DescriptionPath), descriptor.Module);
            var asmName = AssemblyName.GetAssemblyName(asmPath);
            var assembly = Assembly.Load(asmName);
            VerifySdkReference(assembly);

            var asmTypes = assembly.GetTypes();
            Type pluginType = null;
            foreach (Type type in asmTypes)
            {
                if (type.IsInterface || type.IsAbstract)
                {
                    continue;
                }
                else
                {
                    if (typeof(TPlugin).IsAssignableFrom(type))
                    {
                        pluginType = type;
                        break;
                    }
                }
            }

            if (pluginType != null)
            {
                return (TPlugin)Activator.CreateInstance(pluginType, new object[] { injectingApi });
            }

            return default(TPlugin);
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

            foreach (var plugin in GenericPlugins.Values)
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

            foreach (var plugin in GenericPlugins.Values)
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

            foreach (var plugin in GenericPlugins.Values)
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

            foreach (var plugin in GenericPlugins.Values)
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

            foreach (var plugin in GenericPlugins.Values)
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

        private void Database_DatabaseOpened(object sender, EventArgs args)
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

            foreach (var plugin in GenericPlugins.Values)
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
