using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EpicLibrary.Models;
using Playnite.Common;

namespace EpicLibrary
{
    public class EpicLauncher
    {
        public const string GameLaunchUrlMask = @"com.epicgames.launcher://apps/{0}?action=launch&silent=true";

        public static string AllUsersPath => Path.Combine(Environment.ExpandEnvironmentVariables("%PROGRAMDATA%"), "Epic");

        public static string ClientExecPath
        {
            get
            {
                var path = InstallationPath;
                return string.IsNullOrEmpty(path) ? string.Empty : GetExecutablePath(path);
            }
        }

        public static string PortalConfigPath
        {
            get
            {
                var path = InstallationPath;
                return string.IsNullOrEmpty(path) ? string.Empty : Path.Combine(path, "Launcher", "Portal", "Config", "DefaultPortalRegions.ini");
            }
        }

        public static string InstallationPath
        {
            get
            {
                var progs = Programs.GetUnistallProgramsList().
                    FirstOrDefault(a =>
                        a.DisplayName == "Epic Games Launcher" &&
                        !a.InstallLocation.IsNullOrEmpty() &&
                        File.Exists(GetExecutablePath(a.InstallLocation)));
                if (progs == null)
                {
                    return string.Empty;
                }
                else
                {
                    return progs.InstallLocation;
                }
            }
        }

        public static bool IsInstalled
        {
            get
            {
                var path = InstallationPath;
                return !string.IsNullOrEmpty(path) && Directory.Exists(path);
            }
        }

        public static string Icon => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\epicicon.png");

        public static void StartClient()
        {
            ProcessStarter.StartProcess(ClientExecPath, string.Empty);
        }

        internal static string GetExecutablePath(string rootPath)
        {
            // Always prefer 32bit executable
            // https://github.com/JosefNemec/Playnite/issues/1552
            var p32 = Path.Combine(rootPath, "Launcher", "Portal", "Binaries", "Win32", "EpicGamesLauncher.exe");
            if (File.Exists(p32))
            {
                return p32;
            }
            else
            {
                return Path.Combine(rootPath, "Launcher", "Portal", "Binaries", "Win64", "EpicGamesLauncher.exe");
            }
        }

        public static List<LauncherInstalled.InstalledApp> GetInstalledAppList()
        {
            var installListPath = Path.Combine(AllUsersPath, "UnrealEngineLauncher", "LauncherInstalled.dat");
            if (!File.Exists(installListPath))
            {
                return new List<LauncherInstalled.InstalledApp>();
            }

            var list = Serialization.FromJson<LauncherInstalled>(FileSystem.ReadFileAsStringSafe(installListPath));
            return list.InstallationList;
        }

        public static List<InstalledManifiest> GetInstalledManifests()
        {
            var manifests = new List<InstalledManifiest>();
            var installListPath = Path.Combine(AllUsersPath, "EpicGamesLauncher", "Data", "Manifests");
            if (!Directory.Exists(installListPath))
            {
                return manifests;
            }

            foreach (var manFile in Directory.GetFiles(installListPath, "*.item"))
            {
                var manifest = Serialization.FromJson<InstalledManifiest>(FileSystem.ReadFileAsStringSafe(manFile));
                if (manifest != null)
                // Some weird issue causes manifest to be created empty by Epic client
                {
                    manifests.Add(manifest);
                }
            }

            return manifests;
        }
    }
}
