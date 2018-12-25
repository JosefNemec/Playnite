using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EpicLibrary.Models;
using Playnite;
using Playnite.Common;
using Playnite.Common.System;
using Playnite.SDK;

namespace EpicLibrary
{
    public class EpicLauncher
    {
        public const string GameLaunchUrlMask = @"com.epicgames.launcher://apps/{0}?action=launch&silent=true";

        public const string AllUsersPath = @"c:\Users\All Users\Epic\";

        public static string ClientExecPath
        {
            get
            {
                var path = InstallationPath;
                return string.IsNullOrEmpty(path) ? string.Empty : Path.Combine(path, "Launcher", "Portal", "Binaries", Environment.Is64BitOperatingSystem ? "Win64" : "Win32", "EpicGamesLauncher.exe");
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
                var progs = Programs.GetUnistallProgramsList().FirstOrDefault(a => a.DisplayName == "Epic Games Launcher");
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

        public static void StartClient()
        {
            ProcessStarter.StartProcess(ClientExecPath, string.Empty);
        }

        public static List<LauncherInstalled.InstalledApp> GetInstalledAppList()
        {
            if (!IsInstalled)
            {
                throw new Exception("Epic Launcher is not installed.");
            }

            var installListPath = Path.Combine(AllUsersPath, "UnrealEngineLauncher", "LauncherInstalled.dat");
            if (!File.Exists(installListPath))
            {
                return new List<LauncherInstalled.InstalledApp>();
            }

            var list = Serialization.FromJson<LauncherInstalled>(FileSystem.ReadFileAsStringSafe(installListPath));
            return list.InstallationList;
        }
    }
}
