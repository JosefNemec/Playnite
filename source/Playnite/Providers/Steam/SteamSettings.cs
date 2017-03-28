using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using SteamKit2;

namespace Playnite.Providers.Steam
{
    public class SteamSettings
    {
        public static string DefaultIcon
        {
            get; set;
        }

        public static string DefaultImage
        {
            get; set;
        }

        public static string InstallationPath
        {
            get
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
                {
                    if (key != null)
                    {
                        return key.GetValue("SteamPath").ToString();
                    }
                }

                return string.Empty;
            }
        }

        public static List<string> GameDatabases
        {
            get
            {
                var dbs = new List<string>() { InstallationPath };
                var configPath = Path.Combine(InstallationPath, "steamapps", "libraryfolders.vdf");
                var kv = new KeyValue();
                kv.ReadFileAsText(configPath);

                foreach (var child in kv.Children)
                {
                    if (int.TryParse(child.Name, out int test))
                    {
                        dbs.Add(child.Value);
                    }
                }

                return dbs;
            }
        }

        /// <summary>
        /// By default is set to C:\ProgramData\Playnite\steam
        /// </summary>
        public static string DataCachePath
        {
            get
            {
                return Path.Combine(Paths.DataCachePath, "steam");
            }
        }

        /// <summary>
        /// By default is set to C:\ProgramData\Playnite\steam\library.json
        /// </summary>
        public static string LibraryCachePath
        {
            get
            {
                return Path.Combine(DataCachePath, "library.json");
            }
        }

        private string accountName = string.Empty;
        public string AccountName
        {
            get
            {
                return accountName;
            }

            set
            {
                if (accountName != value)
                {
                    accountName = value;
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

        private bool integrationEnabled = false;
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

        private static string descriptionTemplate;
        public static string DescriptionTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(descriptionTemplate))
                {
                    descriptionTemplate = DataResources.ReadFileFromResource("Playnite.Resources.description_steam.html");
                }

                return descriptionTemplate;
            }
        }
    }
}
