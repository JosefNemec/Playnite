using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public static class PlayniteEnvironment
    {
        public static bool ThrowAllErrors => PlayniteSettings.GetAppConfigBoolValue("ThrowAllErrors");

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
    }
}
