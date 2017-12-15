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
    public enum SteamIdSource
    {
        Name,
        LocalUser
    }

    public class SteamSettings
    {
        public static string InstallationPath
        {
            get
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
                {
                    if (key != null)
                    {
                        return key.GetValue("SteamPath").ToString().Replace('/', '\\');
                    }
                }

                return string.Empty;
            }
        }

        public static string LoginUsersPath
        {
            get => Path.Combine(InstallationPath, "config", "loginusers.vdf");
        }

        public SteamIdSource IdSource
        {
            get; set;
        } = SteamIdSource.Name;           

        public ulong AccountId
        {
            get; set;
        }

        public string AccountName
        {
            get; set;
        } = string.Empty;

        public bool PrivateAccount
        {
            get; set;
        } = false;

        public string APIKey
        {
            get; set;
        } = string.Empty;

        public bool LibraryDownloadEnabled
        {
            get; set;
        } = false;
                
        public bool IntegrationEnabled
        {
            get; set;
        } = false;

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
    }
}
