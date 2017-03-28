using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Playnite.Providers.GOG
{
    public class GogSettings : INotifyPropertyChanged
    {
        public static string DefaultIcon
        {
            get; set;
        }

        public static string DefaultImage
        {
            get; set;
        }

        public static string DBStoragePath
        {
            get
            {
                return @"c:\ProgramData\GOG.com\Galaxy\storage\";
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
                    OnPropertyChanged("LibraryDownloadEnabled");
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
                    OnPropertyChanged("IntegrationEnabled");
                }
            }
        }

        private bool useRealIcon = false;
        public bool UseRealIcon
        {
            get
            {
                return useRealIcon;
            }

            set
            {
                if (useRealIcon != value)
                {
                    useRealIcon = value;
                    OnPropertyChanged("UseRealIcon");
                }
            }
        }

        private bool runViaGalaxy = false;
        public bool RunViaGalaxy
        {
            get
            {
                return runViaGalaxy;
            }

            set
            {
                if (runViaGalaxy != value)
                {
                    runViaGalaxy = value;
                    OnPropertyChanged("RunViaGalaxy");
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

        public static string InstallationPath
        {
            get
            {
                RegistryKey key;
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\GOG.com\GalaxyClient\paths");
                if (key == null)
                {
                    Registry.LocalMachine.OpenSubKey(@"SOFTWARE\GOG.com\GalaxyClient\paths");
                }

                if (key == null)
                {
                    return string.Empty;
                }

                return key.GetValue("client").ToString();            

            }
        }

        private static string descriptionTemplate;
        public static string DescriptionTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(descriptionTemplate))
                {
                    descriptionTemplate= DataResources.ReadFileFromResource("Playnite.Resources.description_gog.html");
                }

                return descriptionTemplate;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
