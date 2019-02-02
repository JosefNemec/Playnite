using System;
using System.Collections.Generic;
using System.IO;
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
                return int.Parse(Startup.Configuration.GetSection("Steam")["AppInfoCacheTimeout"]);
            }
        }

        public static string ApiKey
        {
            get
            {
                return Startup.Configuration.GetSection("Steam")["ApiKey"];
            }
        }

        public static string CacheDirectory
        {
            get
            {
                var path = Startup.Configuration.GetSection("Steam")["CacheDirectory"];
                if (Path.IsPathRooted(path))
                {
                    return path;
                }
                else
                {
                    return Path.Combine(Paths.ExecutingDirectory, path);
                }
            }
        }
    }
}
