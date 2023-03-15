using Playnite.API;
using Playnite.Database;
using Playnite.Controllers;
using Playnite.Scripting;
using Playnite.SDK;
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
using Playnite.SDK.Models;
using Playnite.SDK.Events;

namespace Playnite.Plugins
{
    public class ExtensionsStatusBinder
    {
        public class Status : ObservableObject
        {
            private bool isInstalled;
            public bool IsInstalled { get => isInstalled; set => SetValue(ref isInstalled, value); }
        }

        public Status this[string pluginId]
        {
            get
            {
                var plugin = PlayniteApplication.Current.Extensions?.Plugins.FirstOrDefault(a => a.Value.Description.Id == pluginId).Value;
                return new Status { IsInstalled = plugin != null };
            }

            set { throw new NotSupportedException(); }
        }
    }

    public class LoadedPlugin
    {
        public Plugin Plugin { get; }
        public ExtensionManifest Description { get; }
        public string PluginIcon { get; }

        public LoadedPlugin(Plugin plugin, ExtensionManifest description)
        {
            Plugin = plugin;
            Description = description;
            if (!string.IsNullOrEmpty(description.Icon))
            {
                PluginIcon = Path.Combine(Path.GetDirectoryName(description.DescriptionPath), description.Icon);
            }
        }
    }

    public enum AddonLoadError
    {
        None,
        Uknown,
        SDKVersion
    }

    public class ExtensionFactory : ObservableObject, IDisposable
    {
        private static ILogger logger = LogManager.GetLogger();
        private readonly IGameDatabase database;
        private readonly GameControllerFactory controllers;
        private readonly Func<ExtensionManifest, IPlayniteAPI> apiGenerator;

        public List<(ExtensionManifest manifest, AddonLoadError error)> FailedExtensions { get; } = new List<(ExtensionManifest manifest, AddonLoadError error)>();

        public Dictionary<Guid, LoadedPlugin> Plugins
        {
            get; private set;
        } = new Dictionary<Guid, LoadedPlugin>();

        public List<LibraryPlugin> LibraryPlugins
        {
            get => Plugins.Where(a => a.Value.Description.Type == ExtensionType.GameLibrary).Select(a => (LibraryPlugin)a.Value.Plugin).ToList();
        }

        public List<MetadataPlugin> MetadataPlugins
        {
            get => Plugins.Where(a => a.Value.Description.Type == ExtensionType.MetadataProvider).Select(a => (MetadataPlugin)a.Value.Plugin).ToList();
        }

        public List<GenericPlugin> GenericPlugins
        {
            get => Plugins.Where(a => a.Value.Description.Type == ExtensionType.GenericPlugin).Select(a => (GenericPlugin)a.Value.Plugin).ToList();
        }

        public  List<PlayniteScript> Scripts
        {
            get; private set;
        } =  new List<PlayniteScript>();

        public ExtensionFactory(IGameDatabase database, GameControllerFactory controllers, Func<ExtensionManifest, IPlayniteAPI> apiGenerator)
        {
            this.database = database;
            this.controllers = controllers;
            this.apiGenerator = apiGenerator;
            controllers.Installed += Controllers_Installed;
            controllers.Starting += Controllers_Starting;
            controllers.Started += Controllers_Started;
            controllers.Stopped += Controllers_Stopped;
            controllers.Uninstalled += Controllers_Uninstalled;
            controllers.StartupCancelled += Controllers_StartupCancelled;
        }

        public void Dispose()
        {
            DisposePlugins();
            DisposeScripts();
            controllers.Installed -= Controllers_Installed;
            controllers.Starting -= Controllers_Starting;
            controllers.Started -= Controllers_Started;
            controllers.Stopped -= Controllers_Stopped;
            controllers.Uninstalled -= Controllers_Uninstalled;
            controllers.StartupCancelled -= Controllers_StartupCancelled;
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

            Scripts = new List<PlayniteScript>();
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

            Plugins = new Dictionary<Guid, LoadedPlugin>();
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

        private static IEnumerable<ExtensionManifest> GetManifestsFromPath(string path)
        {
            if (Directory.Exists(path))
            {
                var man = GetManifestFromFile(Path.Combine(path, PlaynitePaths.ExtensionManifestFileName));
                if (man != null)
                {
                    yield return man;
                }
            }
            else if (File.Exists(path))
            {
                foreach (var dirPath in File.ReadAllLines(path).Where(a => !a.IsNullOrWhiteSpace() && !a.StartsWith("#")))
                {
                    ExtensionManifest man = null;
                    try
                    {
                        if (Directory.Exists(dirPath))
                        {
                            man = GetManifestFromFile(Path.Combine(dirPath.Trim(), PlaynitePaths.ExtensionManifestFileName));
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Failed to read extension dev file.");
                    }

                    if (man != null)
                    {
                        yield return man;
                    }
                }
            }
        }

        private static ExtensionManifest GetManifestFromFile(string file)
        {
            if (File.Exists(file))
            {
                try
                {
                    return ExtensionManifest.FromFile(file);
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to parse plugin description: {file}");
                    return null;
                }
            }

            return null;
        }

        internal static IEnumerable<BaseExtensionManifest> DeduplicateExtList(List<BaseExtensionManifest> list)
        {
            return list.GroupBy(a => a.Id).Select(g => g.OrderByDescending(x => x.Version).First());
        }

        public static List<ExtensionManifest> GetInstalledManifests(List<string> externalPaths = null)
        {
            var externals = new List<BaseExtensionManifest>();
            var user = new List<BaseExtensionManifest>();
            var install = new List<BaseExtensionManifest>();
            if (externalPaths.HasItems())
            {
                foreach (var ext in externalPaths)
                {
                    foreach (var man in GetManifestsFromPath(ext))
                    {
                        externals.Add(man);
                        man.IsExternalDev = true;
                    }
                }
            }

            if (!PlayniteSettings.IsPortable && Directory.Exists(PlaynitePaths.ExtensionsUserDataPath))
            {
                var enumerator = new SafeFileEnumerator(PlaynitePaths.ExtensionsUserDataPath, PlaynitePaths.ExtensionManifestFileName, SearchOption.AllDirectories);
                foreach (var desc in enumerator)
                {
                    var man = GetManifestFromFile(desc.FullName);
                    if (man?.Id.IsNullOrEmpty() == false)
                    {
                        if (externals.Any(a => a.Id == man.Id))
                        {
                            continue;
                        }
                        else
                        {
                            user.Add(man);
                        }
                    }
                }
            }

            if (Directory.Exists(PlaynitePaths.ExtensionsProgramPath))
            {
                var enumerator = new SafeFileEnumerator(PlaynitePaths.ExtensionsProgramPath, PlaynitePaths.ExtensionManifestFileName, SearchOption.AllDirectories);
                foreach (var desc in enumerator)
                {
                    var man = GetManifestFromFile(desc.FullName);
                    if (man?.Id.IsNullOrEmpty() == false)
                    {
                        if (externals.Any(a => a.Id == man.Id) || user.Any(a => a.Id == man.Id))
                        {
                            continue;
                        }
                        else
                        {
                            install.Add(man);
                        }
                    }
                }
            }

            var result = new List<ExtensionManifest>();
            result.AddRange(DeduplicateExtList(externals).Cast<ExtensionManifest>());
            result.AddRange(DeduplicateExtList(user).Cast<ExtensionManifest>());
            result.AddRange(DeduplicateExtList(install).Cast<ExtensionManifest>());
            return result;
        }

        private bool VerifyAssemblyReferences(Assembly asm, ExtensionManifest manifest)
        {
            var references = asm.GetReferencedAssemblies();
            if (references.Any(a => a.Name == "Playnite" || a.Name == "Playnite.Common") &&
                !BuiltinExtensions.BuiltinExtensionIds.Contains(manifest.Id))
            {
                logger.Error($"Unsupported Playnite assemblies are referenced by {manifest.Name} plugin.");
                return false;
            }

            var sdkReference = references.FirstOrDefault(a => a.Name == "Playnite.SDK");
            if (sdkReference == null)
            {
                logger.Error($"Assembly doesn't reference Playnite SDK.");
                return false;
            }

            if (sdkReference.Version.Major != SDK.SdkVersions.SDKVersion.Major ||
                sdkReference.Version > SDK.SdkVersions.SDKVersion)
            {
                logger.Error($"Plugin doesn't support current version of Playnite SDK, supports {sdkReference.Version}");
                return false;
            }

            return true;
        }

        public bool LoadScripts(List<string> ignoreList, bool builtInOnly, List<string> externals)
        {
            var allSuccess = true;
            DisposeScripts();
            var manifests = GetInstalledManifests(externals).Where(a => a.Type == ExtensionType.Script && !ignoreList.Contains(a.Id)).ToList();
            foreach (var desc in manifests)
            {
                if (desc.Id.IsNullOrEmpty())
                {
                    logger.Error($"Extension {desc.Name}, doesn't have ID.");
                    continue;
                }

                if (builtInOnly && !BuiltinExtensions.BuiltinExtensionIds.Contains(desc.Id))
                {
                    logger.Warn($"Skipping load of {desc.Name}, builtInOnly is enabled.");
                    continue;
                }

                PlayniteScript script = null;
                var scriptPath = Path.Combine(Path.GetDirectoryName(desc.DescriptionPath), desc.Module);
                if (!File.Exists(scriptPath))
                {
                    logger.Error($"Cannot load script extension, {scriptPath} not found.");
                    FailedExtensions.Add((desc, AddonLoadError.Uknown));
                    continue;
                }

                try
                {
                    script = PlayniteScript.FromFile(scriptPath, $"{desc.DirectoryName}#PS");
                    if (script == null)
                    {
                        FailedExtensions.Add((desc, AddonLoadError.Uknown));
                        continue;
                    }

                    Localization.LoadAddonLocalization(desc.DirectoryPath);
                    script.SetVariable("PlayniteApi", apiGenerator(desc));
                    script.SetVariable("CurrentExtensionInstallPath", desc.DirectoryPath);
                    if (!desc.Id.IsNullOrEmpty())
                    {
                        var extDir = Path.Combine(PlaynitePaths.ExtensionsDataPath, Paths.GetSafePathName(desc.Id));
                        FileSystem.CreateDirectory(extDir);
                        script.SetVariable("CurrentExtensionDataPath", extDir);
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    allSuccess = false;
                    logger.Error(e, $"Failed to load script file {scriptPath}");
                    FailedExtensions.Add((desc, AddonLoadError.Uknown));
                    continue;
                }

                Scripts.Add(script);
                logger.Info($"Loaded script extension: {scriptPath}, version {desc.Version}");
            }

            return allSuccess;
        }

        public void LoadPlugins(List<string> ignoreList, bool builtInOnly, List<string> externals)
        {
            if (Plugins.HasItems())
            {
                throw new Exception("Plugin can be loaded only once!");
            }

            var manifests = GetInstalledManifests(externals).Where(a => a.Type != ExtensionType.Script && ignoreList?.Contains(a.Id) != true).ToList();
            foreach (var desc in manifests)
            {
                if (desc.Id.IsNullOrEmpty())
                {
                    logger.Error($"Extension {desc.Name}, doesn't have ID.");
                    continue;
                }

                if (builtInOnly && !BuiltinExtensions.BuiltinExtensionIds.Contains(desc.Id))
                {
                    logger.Warn($"Skipping load of {desc.Name}, builtInOnly is enabled.");
                    continue;
                }

                try
                {
                    Localization.LoadAddonLocalization(desc.DirectoryPath);
                    var plugins = LoadPlugins(desc, apiGenerator);
                    foreach (var plugin in plugins)
                    {
                        if (Plugins.ContainsKey(plugin.Id))
                        {
                            logger.Warn($"Plugin {plugin.Id} is already loaded.");
                            continue;
                        }

                        Plugins.Add(plugin.Id, new LoadedPlugin(plugin, desc));
                        logger.Info($"Loaded plugin: {desc.Name}, version {desc.Version}");
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e.InnerException, $"Failed to load plugin: {desc.Name}");
                    if (e.InnerException == null)
                    {
                        logger.Error(e, string.Empty);
                    }

                    if (e is ReflectionTypeLoadException reflectionTypeLoadException)
                    {
                        foreach (var loaderException in reflectionTypeLoadException.LoaderExceptions)
                        {
                            logger.Error(loaderException, string.Empty);
                        }
                    }

                    FailedExtensions.Add((desc, AddonLoadError.Uknown));
                }
            }
        }

        private IEnumerable<Plugin> LoadPlugins(ExtensionManifest descriptor, Func<ExtensionManifest, IPlayniteAPI> apiGenerator)
        {
            var asmPath = Path.Combine(Path.GetDirectoryName(descriptor.DescriptionPath), descriptor.Module);
            var asmName = AssemblyName.GetAssemblyName(asmPath);
            var assembly = Assembly.Load(asmName);
            if (VerifyAssemblyReferences(assembly, descriptor))
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsInterface || type.IsAbstract)
                    {
                        continue;
                    }
                    else
                    {
                        if (typeof(GenericPlugin).IsAssignableFrom(type) || typeof(LibraryPlugin).IsAssignableFrom(type) || typeof(MetadataPlugin).IsAssignableFrom(type))
                        {
                            var ignore = Attribute.IsDefined(type, typeof(IgnorePluginAttribute));
                            var load = Attribute.IsDefined(type, typeof(LoadPluginAttribute));
                            if ((ignore && load) || !ignore)
                            {
                                yield return (Plugin)Activator.CreateInstance(type, new object[] { apiGenerator(descriptor) });
                            }
                        }
                    }
                }
            }
            else
            {
                logger.Error($"Plugin dependencices are not compatible: {descriptor.Name}");
                FailedExtensions.Add((descriptor, AddonLoadError.SDKVersion));
                // TODO: Unload assembly once Playnite switches to .NET Core
            }
        }

        public bool InvokeExtension(ExtensionFunction function, out Exception error)
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
                error = e;
                return false;
            }
        }

        private void Controllers_Uninstalled(object sender, GameUninstalledEventArgs args)
        {
            if (args.Source?.Game == null)
            {
                logger.Error("No game controller information found!");
                return;
            }

            var callbackArgs = new SDK.Events.OnGameUninstalledEventArgs { Game = args.Source.Game };
            foreach (var script in Scripts)
            {
                try
                {
                    script.OnGameUninstalled(callbackArgs);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to execute OnGameUninstalled method from {script.Name} script.");
                }
            }

            foreach (var plugin in Plugins.Values)
            {
                try
                {
                    plugin.Plugin.OnGameUninstalled(callbackArgs);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to execute OnGameUninstalled method from {plugin.Description.Name} plugin.");
                }
            }
        }

        private void Controllers_Stopped(object sender, GameStoppedEventArgs args)
        {
            if (args.Source?.Game?.Id == null)
            {
                logger.Error("No game controller information found!");
                return;
            }
        }

        public void InvokeOnGameStopped(Game game, ulong ellapsedTime, bool manuallyStopped)
        {
            var callbackArgs = new SDK.Events.OnGameStoppedEventArgs
            {
                Game = database.Games[game.Id],
                ElapsedSeconds = ellapsedTime,
                ManuallyStopped = manuallyStopped
            };

            foreach (var script in Scripts)
            {
                try
                {
                    script.OnGameStopped(callbackArgs);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to execute OnGameStopped method from {script.Name} script.");
                }
            }

            foreach (var plugin in Plugins.Values)
            {
                try
                {
                    plugin.Plugin.OnGameStopped(callbackArgs);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to execute OnGameStopped method from {plugin.Description.Name} plugin.");
                }
            }
        }

        private void Controllers_StartupCancelled(object sender, OnGameStartupCancelledEventArgs args)
        {
            foreach (var script in Scripts)
            {
                try
                {
                    script.OnGameStartupCancelled(args);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to execute OnGameStartupCancelled method from {script.Name} script.");
                }
            }

            foreach (var plugin in Plugins.Values)
            {
                try
                {
                    plugin.Plugin.OnGameStartupCancelled(args);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to execute OnGameStartupCancelled method from {plugin.Description.Name} plugin.");
                }
            }
        }

        private void Controllers_Starting(object sender, OnGameStartingEventArgs args)
        {
            foreach (var script in Scripts)
            {
                try
                {
                    script.OnGameStarting(args);
                    if (args.CancelStartup)
                    {
                        return;
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to execute OnGameStarting method from {script.Name} script.");
                }
            }

            foreach (var plugin in Plugins.Values)
            {
                try
                {
                    plugin.Plugin.OnGameStarting(args);
                    if (args.CancelStartup)
                    {
                        return;
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to execute OnGameStarting method from {plugin.Description.Name} plugin.");
                }
            }
        }

        private void Controllers_Started(object sender, GameStartedEventArgs args)
        {
            if (args.Source?.Game?.Id == null)
            {
                logger.Error("No game controller information found!");
                return;
            }

            var callbackArgs = new OnGameStartedEventArgs
            {
                Game = database.Games[args.Source.Game.Id],
                SourceAction = (args.Source as GenericPlayController)?.StartingArgs?.SourceAction?.GetClone(),
                SelectedRomFile = (args.Source as GenericPlayController)?.StartingArgs.SelectedRomFile,
                StartedProcessId = args.StartedProcessId
            };

            foreach (var script in Scripts)
            {
                try
                {
                    script.OnGameStarted(callbackArgs);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to execute OnGameStarted method from {script.Name} script.");
                }
            }

            foreach (var plugin in Plugins.Values)
            {
                try
                {
                    plugin.Plugin.OnGameStarted(callbackArgs);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to execute OnGameStarted method from {plugin.Description.Name} plugin.");
                }
            }
        }

        private void Controllers_Installed(object sender, GameInstalledEventArgs args)
        {
            if (args.Source?.Game?.Id == null)
            {
                logger.Error("No game controller information found!");
                return;
            }

            var callbackArgs = new SDK.Events.OnGameInstalledEventArgs { Game = database.Games[args.Source.Game.Id] };
            foreach (var script in Scripts)
            {
                try
                {
                    script.OnGameInstalled(callbackArgs);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to execute OnGameInstalled method from {script.Name} script.");
                }
            }

            foreach (var plugin in Plugins.Values)
            {
                try
                {
                    plugin.Plugin.OnGameInstalled(callbackArgs);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to execute OnGameInstalled method from {plugin.Description.Name} plugin.");
                }
            }
        }

        public void InvokeOnGameSelected(List<Game> oldValue, List<Game> newValue)
        {
            var args = new SDK.Events.OnGameSelectedEventArgs(oldValue, newValue);
            foreach (var script in Scripts)
            {
                try
                {
                    script.OnGameSelected(args);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to execute OnGameSelected method from {script.Name} script.");
                }
            }

            foreach (var plugin in Plugins.Values)
            {
                try
                {
                    plugin.Plugin.OnGameSelected(args);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to execute OnGameSelected method from {plugin.Description.Name} plugin.");
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
                    logger.Error(e, $"Failed to execute OnApplicationStarted method from {script.Name} script.");
                }
            }

            foreach (var plugin in Plugins.Values)
            {
                try
                {
                    plugin.Plugin.OnApplicationStarted(new SDK.Events.OnApplicationStartedEventArgs());
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to execute OnApplicationStarted method from {plugin.Description.Name} plugin.");
                }
            }
        }

        public void NotifiyOnApplicationStopped()
        {
            foreach (var script in Scripts)
            {
                try
                {
                    script.OnApplicationStopped();
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to execute OnApplicationStopped method from {script.Name} script.");
                }
            }

            foreach (var plugin in Plugins.Values)
            {
                try
                {
                    plugin.Plugin.OnApplicationStopped(new SDK.Events.OnApplicationStoppedEventArgs());
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to execute OnApplicationStopped method from {plugin.Description.Name} plugin.");
                }
            }
        }

        public void NotifiyOnLibraryUpdated()
        {
            foreach (var script in Scripts)
            {
                try
                {
                    script.OnLibraryUpdated();
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to execute OnLibraryUpdated method from {script.Name} script.");
                }
            }

            foreach (var plugin in Plugins.Values)
            {
                try
                {
                    plugin.Plugin.OnLibraryUpdated(new SDK.Events.OnLibraryUpdatedEventArgs());
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to execute OnLibraryUpdated method from {plugin.Description.Name} plugin.");
                }
            }
        }

        public LibraryPlugin GetLibraryPlugin(Guid pluginId)
        {
            if (pluginId == Guid.Empty)
            {
                return null;
            }

            return LibraryPlugins.FirstOrDefault(a => a.Id == pluginId);
        }

        public List<PluginUiElementSupport> CustomElementList = new List<PluginUiElementSupport>();
        public void AddCustomElementSupport(Plugin source, AddCustomElementSupportArgs args)
        {
            if (CustomElementList.Any(a => a.Source == source))
            {
                return;
            }

            var elemSupport = args.GetClone<AddCustomElementSupportArgs, PluginUiElementSupport>();
            elemSupport.Source = source;
            CustomElementList.Add(elemSupport);
        }

        public List<PluginSettingsSupport> SettingsSupportList = new List<PluginSettingsSupport>();
        public void AddSettingsSupport(Plugin source, AddSettingsSupportArgs args)
        {
            if (SettingsSupportList.Any(a => a.Source == source))
            {
                return;
            }

            var elemSupport = args.GetClone<AddSettingsSupportArgs, PluginSettingsSupport>();
            elemSupport.Source = source;
            SettingsSupportList.Add(elemSupport);
        }

        public List<PluginConvertersSupport> ConvertersSupportList = new List<PluginConvertersSupport>();
        public void AddConvertersSupport(Plugin source, AddConvertersSupportArgs args)
        {
            if (ConvertersSupportList.Any(a => a.Source == source))
            {
                return;
            }

            ConvertersSupportList.Add(new PluginConvertersSupport
            {
                Source = source,
                Converters = args.Converters,
                SourceName = args.SourceName
            });
        }

        public List<TopPanelItem> GetTopPanelPluginItems()
        {
            var res = new List<TopPanelItem>();
            foreach (var plugin in Plugins.Values)
            {
                try
                {
                    var items = plugin.Plugin.GetTopPanelItems().ToList();
                    if (items.HasItems())
                    {
                        res.AddRange(items);
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to get top panel itesm from {plugin.Description.Id}");
                }
            }

            return res;
        }
    }

    public class PluginUiElementSupport : AddCustomElementSupportArgs
    {
        public Plugin Source { get; set; }
    }

    public class PluginSettingsSupport : AddSettingsSupportArgs
    {
        public Plugin Source { get; set; }
    }

    public class PluginConvertersSupport : AddConvertersSupportArgs
    {
        public Plugin Source { get; set; }
    }
}