using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using SteamKit2;

namespace Playnite.Providers.Origin
{
    public class OriginSettings
    {
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

        public static string ClientExecPath
        {
            get
            {
                var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                var key = root.OpenSubKey(@"SOFTWARE\Origin");                    
                if (key != null)
                {
                    return key.GetValue("ClientPath").ToString();
                }                   

                return string.Empty;
            }
        }

        public static string InstallationPath
        {
            get
            {
                var path = ClientExecPath;
                if (!string.IsNullOrEmpty(path))
                {
                    return Path.GetDirectoryName(path);
                }

                return string.Empty;
            }
        }

        public static bool IsInstalled
        {
            get
            {
                if (string.IsNullOrEmpty(ClientExecPath) || !File.Exists(ClientExecPath))
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
