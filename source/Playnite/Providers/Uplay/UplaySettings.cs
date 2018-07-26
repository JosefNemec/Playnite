using Playnite.Common.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Providers.Uplay
{
    public class UplaySettings
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

        private bool libraryDownloadEnabled = false;
        public bool LibraryDownloadEnabled
        {
            get
            {
                return libraryDownloadEnabled;
            }

            set
            {
                if (libraryDownloadEnabled != value)
                {
                    libraryDownloadEnabled = value;
                }
            }
        }

        private bool integrationEnabled = true;
        public bool IntegrationEnabled
        {
            get
            {
                return integrationEnabled;
            }

            set
            {
                if (integrationEnabled != value)
                {
                    integrationEnabled = value;
                }
            }
        }
    }
}
