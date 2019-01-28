using Playnite;
using Playnite.Common.System;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UplayLibrary
{
    public class Uplay
    {
        public static string ClientExecPath
        {
            get
            {
                var path = InstallationPath;
                return string.IsNullOrEmpty(path) ? string.Empty : Path.Combine(path, "Uplay.exe");
            }
        }

        public static string InstallationPath
        {
            get
            {
                var progs = Programs.GetUnistallProgramsList().FirstOrDefault(a => a.DisplayName == "Uplay");
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
                var progs = Programs.GetUnistallProgramsList().FirstOrDefault(a => a.DisplayName == "Uplay");
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

        public static bool GetGameRequiresUplay(Game game)
        {
            if (string.IsNullOrEmpty(game.InstallDirectory) || !Directory.Exists(game.InstallDirectory))
            {
                return false;
            }

            var fileEnumerator = new SafeFileEnumerator(game.InstallDirectory, "uplay_*_loader*", SearchOption.AllDirectories);
            return fileEnumerator.Any() == true;
        }
    }
}
