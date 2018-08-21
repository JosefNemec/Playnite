using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginLibrary
{
    public class Origin
    {
        public const string DataPath = @"c:\ProgramData\Origin\";

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

        public static string GetCachePath(string rootPath)
        {
            return Path.Combine(rootPath, "origincache");
        }
    }
}
