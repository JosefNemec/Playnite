using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Plugins
{
    public class Plugins
    {
        public static void CreatePluginFolders()
        {
            FileSystem.CreateDirectory(Paths.PluginsProgramPath);
            if (!Settings.IsPortable)
            {
                FileSystem.CreateDirectory(Paths.PluginsUserDataPath);
            }
        }

        public static IEnumerable<string> GetDllsFromFolder(string path)
        {
            foreach (var file in Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly))
            {
                yield return file;
            }
        }

        public static List<string> GetPluginFiles()
        {
            var plugins = new List<string>();
            if (Directory.Exists(Paths.PluginsProgramPath))
            {
                foreach (var dir in Directory.GetDirectories(Paths.PluginsProgramPath))
                {
                    plugins.AddRange(GetDllsFromFolder(dir));
                }
            }

            if (!Settings.IsPortable)
            {
                if (Directory.Exists(Paths.PluginsUserDataPath))
                {
                    foreach (var dir in Directory.GetDirectories(Paths.PluginsUserDataPath))
                    {
                        plugins.AddRange(GetDllsFromFolder(dir));
                    }
                }
            }

            return plugins;
        }

        public static List<Plugin> LoadPlugin(string path, IPlayniteAPI api)
        {
            var plugins = new List<Plugin>();
            var asmName = AssemblyName.GetAssemblyName(path);
            var assembly = Assembly.Load(asmName);
            var sdkReference = assembly.GetReferencedAssemblies().FirstOrDefault(a => a.Name == "PlayniteSDK");
            if (sdkReference == null)
            {
                return plugins;
            }

            if (sdkReference.Version.Major != SDK.Version.SDKVersion.Major)
            {
                throw new Exception($"Plugin doesn't support this version of Playnite SDK.");
            }

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
