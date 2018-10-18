using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.Playnite
{
    public static class Playnite
    {
        public static string DiagsLocation
        {
            get
            {
                return Startup.Configuration.GetSection("DiagsLocation")?.Value;
            }
        }
    }
}
