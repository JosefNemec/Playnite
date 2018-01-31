using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.Steam
{
    public class Steam
    {
        private static int? storeCacheTimeout;
        public static int StoreCacheTimeout
        {
            get
            {
                if (storeCacheTimeout == null)
                {
                    storeCacheTimeout = int.Parse(Startup.Configuration.GetSection("SteamStoreCacheTimeout").Value);
                }

                return storeCacheTimeout.Value;
            }
        }

        private static int? appInfoCacheTimeout;
        public static int AppInfoCacheTimeout
        {
            get
            {
                if (appInfoCacheTimeout == null)
                {
                    appInfoCacheTimeout = int.Parse(Startup.Configuration.GetSection("SteamAppInfoCacheTimeout").Value);
                }

                return appInfoCacheTimeout.Value;
            }
        }
    }
}
