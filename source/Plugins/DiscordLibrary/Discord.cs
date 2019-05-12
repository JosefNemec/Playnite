using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Playnite.Common;

namespace DiscordLibrary
{
    public class Discord
    {
        public static string ClientExecPath
        {
            get
            {
                var path = InstallationPath;
                // The installation path contains an updater that launches Discord.exe at startup.
                // Discord.exe is in a versioned subdirectory with an unpredicitable path, just look for Update.exe
                return string.IsNullOrEmpty(path) ? string.Empty : Path.Combine(path, "Update.exe");
            }
        }

        public static bool IsInstalled
        {
            get
            {
                return !string.IsNullOrEmpty(InstallationPath) && Directory.Exists(InstallationPath);
            }
        }

        public static string InstallationPath
        {
            get
            {
                var program = Programs.GetUnistallProgramsList().FirstOrDefault(a => a.DisplayName == "Discord");
                if (program == null)
                {
                    return null;
                }

                return program.InstallLocation;
            }
        }

        public static UninstallProgram GetUninstallRecord(string gameId)
        {
            return Programs.GetUnistallProgramsList()
                .FirstOrDefault(a => a.RegistryKeyName.Equals(gameId, StringComparison.OrdinalIgnoreCase));
        }

        public static string Icon => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\discordicon.png");
    }
}
