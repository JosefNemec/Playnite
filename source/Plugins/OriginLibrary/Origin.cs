using Microsoft.Win32;
using Playnite.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OriginLibrary
{
    public class Origin
    {
        public const string DataPath = @"c:\ProgramData\Origin\";

        public static bool IsRunning
        {
            get
            {
                return Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ClientExecPath))?.Any() == true;
            }
        }

        public static string ClientExecPath
        {
            get
            {
                var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                var key = root.OpenSubKey(@"SOFTWARE\Origin");
                if (key?.GetValueNames().Contains("ClientPath") == true)
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

        public static string Icon => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\originicon.png");

        public static string GetCachePath(string rootPath)
        {
            return Path.Combine(rootPath, "origincache");
        }

        public static void StartClient()
        {
            ProcessStarter.StartProcess(ClientExecPath, string.Empty);
        }

        public static bool GetGameRequiresOrigin(string installDir)
        {
            if (string.IsNullOrEmpty(installDir) || !Directory.Exists(installDir))
            {
                return false;
            }

            var fileEnumerator = new SafeFileEnumerator(installDir, "Activation.*", SearchOption.AllDirectories);
            return fileEnumerator.Any() == true;
        }

        public static bool GetGameUsesEasyAntiCheat(string installDir)
        {
            if (string.IsNullOrEmpty(installDir) || !Directory.Exists(installDir))
            {
                return false;
            }

            var fileEnumerator = new SafeFileEnumerator(installDir, "EasyAntiCheat*.dll", SearchOption.AllDirectories);
            return fileEnumerator.Any() == true;
        }

        public static string GetLaunchString(string offerId)
        {
            return $"origin2://game/launch?offerIds={offerId}";
        }
    }
}
