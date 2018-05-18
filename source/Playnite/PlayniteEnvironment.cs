using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public static class PlayniteEnvironment
    {
        public static bool ThrowAllErrors
        {
            get
            {
                return Settings.GetAppConfigBoolValue("ThrowAllErrors");
            }
        }

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
