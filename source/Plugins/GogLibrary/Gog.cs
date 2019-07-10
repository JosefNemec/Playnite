using Microsoft.Win32;
using Playnite.Common.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GogLibrary
{
    public class Gog
    {
        public const string EnStoreLocaleString = "US_USD_en-US";

        public static string ClientExecPath
        {
            get
            {
                var path = InstallationPath;
                return string.IsNullOrEmpty(path) ? string.Empty : Path.Combine(path, "GalaxyClient.exe");
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

                if (key?.GetValueNames().Contains("client") == true)
                {
                    return key.GetValue("client").ToString();
                }          

                return string.Empty;
            }
        }

        public static string Icon => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\gogicon.png");

        public static string GetLoginUrl()
        {
            var loginUrl = string.Empty;
            var mainPage = HttpDownloader.DownloadString("https://www.gog.com/").Split('\n');
            foreach (var line in mainPage)
            {
                if (line.TrimStart().StartsWith("var galaxyAccounts"))
                {
                    var match = Regex.Match(line, "'(.*)','(.*)'");
                    if (match.Success)
                    {
                        loginUrl = match.Groups[1].Value;
                    }
                }
            }

            return loginUrl;
        }
    }
}
