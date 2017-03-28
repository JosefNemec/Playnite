using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Providers.Origin
{
    public static class OriginPaths
    {
        public const string DataPath = @"c:\ProgramData\Origin\";

        private static string cachePath = Path.Combine(Playnite.Paths.DataCachePath, "origincache");
        public static string CachePath
        {            
            get
            {
                return cachePath;
            }

            set
            {
                cachePath = value;
            }

        }
    }
}
