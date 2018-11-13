using Playnite.Common.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BethesdaLibrary
{
    public class Bethesda
    {
        public static string ClientExecPath
        {
            get
            {
                var path = InstallationPath;
                return string.IsNullOrEmpty(path) ? string.Empty : Path.Combine(path, "BethesdaNetLauncher.exe");
            }
        }

        public static string InstallationPath
        {
            get
            {
                var progs = Programs.GetUnistallProgramsList().FirstOrDefault(a => a.DisplayName == "Bethesda.net Launcher");
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
                var progs = Programs.GetUnistallProgramsList().FirstOrDefault(a => a.DisplayName == "Bethesda.net Launcher");
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

        public static IEnumerable<UninstallProgram> GetBethesdaInstallEntried()
        {
            return Programs.GetUnistallProgramsList().Where(a => a.UninstallString?.Contains(@"bethesdanet://uninstall") == true);
        }
    }
}
