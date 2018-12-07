using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.PlayniteTools
{
    public static class Playnite
    {
        public static string DiagsLocation
        {
            get
            {
                var path = Startup.Configuration.GetSection("DiagsDirectory").Value;
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

        public static string DiagsCrashLocation
        {
            get => Path.Combine(DiagsLocation, "crashes");
        }
    }
}
