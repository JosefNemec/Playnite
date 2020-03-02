using Microsoft.Win32;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;
using System.Reflection;

namespace SteamLibrary
{
    public class Steam
    {
        public static string LoginUsersPath
        {
            get => Path.Combine(InstallationPath, "config", "loginusers.vdf");
        }

        public static string ClientExecPath
        {
            get
            {
                var path = InstallationPath;
                return string.IsNullOrEmpty(path) ? string.Empty : Path.Combine(path, "steam.exe");
            }
        }

        public static string InstallationPath
        {
            get
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
                {
                    if (key?.GetValueNames().Contains("SteamPath") == true)
                    {
                        return key.GetValue("SteamPath")?.ToString().Replace('/', '\\') ?? string.Empty;
                    }
                }

                return string.Empty;
            }
        }

        public static string ModInstallPath
        {
            get
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
                {
                    if (key?.GetValueNames().Contains("ModInstallPath") == true)
                    {
                        return key.GetValue("ModInstallPath")?.ToString().Replace('/', '\\') ?? string.Empty;
                    }
                }

                return string.Empty;
            }
        }

        public static string SourceModInstallPath
        {
            get
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
                {
                    if (key?.GetValueNames().Contains("SourceModInstallPath") == true)
                    {
                        return key.GetValue("SourceModInstallPath")?.ToString().Replace('/', '\\') ?? string.Empty;
                    }
                }

                return string.Empty;
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

        public static string Icon => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\steamicon.png");

        public static AppState GetAppState(GameID id)
        {
            var state = new AppState();
            var rootString = @"Software\Valve\Steam\Apps\" + id.AppID.ToString();
            var root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
            var appKey = root.OpenSubKey(rootString);
            if (appKey != null)
            {
                if (appKey.GetValue("Installed")?.ToString() == "1")
                {
                    state.Installed = true;
                }

                if (appKey.GetValue("Launching")?.ToString() == "1")
                {
                    state.Launching = true;
                }

                if (appKey.GetValue("Running")?.ToString() == "1")
                {
                    state.Running = true;
                }

                if (appKey.GetValue("Updating")?.ToString() == "1")
                {
                    state.Installing = true;
                }
            }

            if (id.IsMod && state.Installed)
            {
                // Base app is installed, but the mod itself might not be.
                bool foundMod = false;

                if (ModInfo.GetModTypeOfGameID(id) == ModInfo.ModType.HL)
                {
                    // GoldSrc / HL(1) mod
                    if (!string.IsNullOrEmpty(Steam.ModInstallPath))
                    {
                        foreach (var folder in Directory.GetDirectories(Steam.ModInstallPath))
                        {
                            var modInfo = ModInfo.GetFromFolder(folder, ModInfo.ModType.HL);
                            if (modInfo?.GameId == id)
                            {
                                foundMod = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // Source mod
                    if (!string.IsNullOrEmpty(Steam.SourceModInstallPath))
                    {
                        foreach (var folder in Directory.GetDirectories(Steam.SourceModInstallPath))
                        {
                            var modInfo = ModInfo.GetFromFolder(folder, ModInfo.ModType.HL2);
                            if (modInfo?.GameId == id)
                            {
                                foundMod = true;
                                break;
                            }
                        }
                    }
                }

                if (!foundMod)
                {
                    state.Installed = false;
                }
            }

            return state;
        }
    }
}
