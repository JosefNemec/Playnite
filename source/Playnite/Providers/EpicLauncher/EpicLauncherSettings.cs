using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Providers.EpicLauncher
{
    public class EpicLauncherSettings
    {
        public static string ClientExecPath
        {
            get
            {
                var path = InstallationPath;
                return string.IsNullOrEmpty(path) ? string.Empty : Path.Combine(path, "Launcher", "Portal", "Binaries", Environment.Is64BitOperatingSystem ? "Win64" : "Win32", "EpicGamesLauncher.exe");
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
                var progs = Programs.GetUnistallProgramsList().FirstOrDefault(a => a.DisplayName == "Epic Games Launcher");
                if (progs == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
