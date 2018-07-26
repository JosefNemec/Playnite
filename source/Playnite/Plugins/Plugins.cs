using Playnite.API;
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

namespace Playnite.Plugins
{
    public class PluginFactory
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void CreatePluginFolders()
        {
            FileSystem.CreateDirectory(PlaynitePaths.PluginsProgramPath);
            if (!PlayniteSettings.IsPortable)
            {
                FileSystem.CreateDirectory(PlaynitePaths.PluginsUserDataPath);
            }
        }

        public static List<PluginDescription> GetPluginDescriptors()
        {
            var descs = new List<PluginDescription>();
            foreach (var file in GetPluginDescriptorFiles())
            {
                try
                {
                    descs.Add(PluginDescription.FromFile(file));
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e.InnerException, $"Failed to parse plugin description: {file}");
                    continue;
                }
            }

            return descs;
        }

        public static List<string> GetPluginDescriptorFiles()
        {
            var plugins = new List<string>();
            if (Directory.Exists(PlaynitePaths.PluginsProgramPath))
            {
                foreach (var dir in Directory.GetDirectories(PlaynitePaths.PluginsProgramPath))
                {
                    var descriptorPath = Path.Combine(dir, "plugin.info");
                    if (File.Exists(descriptorPath))
                    {
                        plugins.Add(descriptorPath);
                    }
                }
            }

            if (!PlayniteSettings.IsPortable)
            {
                if (Directory.Exists(PlaynitePaths.PluginsUserDataPath))
                {
                    foreach (var dir in Directory.GetDirectories(PlaynitePaths.PluginsUserDataPath))
                    {
                        var descriptorPath = Path.Combine(dir, "plugin.info");
                        if (File.Exists(descriptorPath))
                        {
                            plugins.Add(descriptorPath);
                        }
                    }
                }
            }

            return plugins;
        }

        private static void VerifySdkReference(Assembly asm)
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

        public static List<ILibraryPlugin> LoadGameLibraryPlugin(PluginDescription descriptor, IPlayniteAPI injectingApi)
        {
            var plugins = new List<ILibraryPlugin>();
            var asmPath = Path.Combine(Path.GetDirectoryName(descriptor.Path), descriptor.Assembly);
            var asmName = AssemblyName.GetAssemblyName(asmPath);
            var assembly = Assembly.Load(asmName);
            VerifySdkReference(assembly);

            var asmTypes = assembly.GetTypes();
            var pluginTypes = new List<Type>();
            foreach (Type type in asmTypes)
            {
                if (type.IsInterface || type.IsAbstract)
                {
                    continue;
                }
                else
                {
                    if (typeof(ILibraryPlugin).IsAssignableFrom(type))
                    {
                        pluginTypes.Add(type);
                    }
                }
            }

            foreach (var type in pluginTypes)
            {
                ILibraryPlugin plugin = (ILibraryPlugin)Activator.CreateInstance(type, new object[] { injectingApi });
                plugins.Add(plugin);
            }

            return plugins;
        }

        public static List<Plugin> LoadGenericPlugin(string descriptorPath, IPlayniteAPI api)
        {
            var plugins = new List<Plugin>();
            var asmName = AssemblyName.GetAssemblyName(descriptorPath);
            var assembly = Assembly.Load(asmName);
            VerifySdkReference(assembly);

            var pluginTypes = new List<Type>();
            var asmTypes = assembly.GetTypes();
            foreach (Type type in asmTypes)
            {
                if (type.IsInterface || type.IsAbstract)
                {
                    continue;
                }
                else
                {
                    if (type.BaseType == typeof(Plugin))
                    {
                        pluginTypes.Add(type);
                    }
                }
            }
            
            foreach (var type in pluginTypes)
            {
                Plugin plugin = (Plugin)Activator.CreateInstance(type, new object[] { api });
                plugins.Add(plugin);
            }

            return plugins;
        }
    }
}
