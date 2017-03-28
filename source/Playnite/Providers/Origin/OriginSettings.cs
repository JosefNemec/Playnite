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
        public static string DefaultIcon
        {
            get; set;
        }

        public static string DefaultImage
        {
            get; set;
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
