using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.Steam
{
    public class Steam
    {
        public static int StoreCacheTimeout
        {
            get
            {
                return int.Parse(Startup.Configuration.GetSection("Steam")["StoreCacheTimeout"]);
            }
        }

        public static int AppInfoCacheTimeout
        {
            get
            {
                return int.Parse(Startup.Configuration.GetSection("Stean")["AppInfoCacheTimeout"]);
            }
        }

        public static string ApiKey
        {
            get
            {
                return Startup.Configuration.GetSection("Steam")["ApiKey"];
            }
        }
    }
}
