using Microsoft.Win32;
using Playnite.Common.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLibrary
{
    public class Twitch
    {
        public const string GameRemoverPath = @"C:\ProgramData\Twitch\Games\Uninstaller\TwitchGameRemover.exe";

        public static string ClientExecPath
        {
            get
            {
                var path = InstallationPath;
                return string.IsNullOrEmpty(path) ? string.Empty : Path.Combine(path, "Bin", "Twitch.exe");
            }
        }

        public static bool IsInstalled
        {
            get
            {
                if (string.IsNullOrEmpty(InstallationPath) || !Directory.Exists(InstallationPath))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public static string InstallationPath
        {
            get
            {
                var program = Programs.GetUnistallProgramsList().FirstOrDefault(a => a.DisplayName == "Twitch");
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
                .FirstOrDefault(a => a.RegistryKeyName.Trim(new char[] { '{', '}' }).Equals(gameId, StringComparison.CurrentCultureIgnoreCase));
        }

    }
}
