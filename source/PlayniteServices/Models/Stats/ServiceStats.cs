using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Models.Stats
{
    public class ServiceStats
    {
        public int UserCount;
        public int LastWeekUserCount;
        public Dictionary<string, int> UsersByVersion;
        public Dictionary<string, int> UsersByWinVersion;
        public int X86Count;
        public int X64Count;
    }
}
