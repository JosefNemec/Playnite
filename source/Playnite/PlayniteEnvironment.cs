﻿using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public enum ReleaseChannel
    {
        Stable,
        Beta,
        Patreon
    }

    public static class PlayniteEnvironment
    {
        public static ReleaseChannel ReleaseChannel
        {
            get
            {
                switch (PlayniteSettings.GetAppConfigValue("UpdateBranch"))
                {
                    case "stable":
                        return ReleaseChannel.Stable;
                    case "patreon":
                        return ReleaseChannel.Patreon;
                    case "beta":
                        return ReleaseChannel.Beta;
                    default:
                        return ReleaseChannel.Stable;
                }
            }
        }

        public static string AppBranch => PlayniteSettings.GetAppConfigValue("AppBranch");

        public static bool ThrowAllErrors => PlayniteSettings.GetAppConfigBoolValue("ThrowAllErrors") && Debugger.IsAttached;

        public static bool InOfflineMode => PlayniteSettings.GetAppConfigBoolValue("OfflineMode");

        public static bool IsDebuggerAttached => Debugger.IsAttached;

        public static bool IsDebugBuild
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsElevated
        {
            get
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
        }
    }
}
