using PlayniteServices.Models.Playnite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Models.Stats
{
    public class ServiceStats
    {
        public int UserCount
        {
            get; set;
        }

        public int LastWeekUserCount
        {
            get; set;
        }

        public Dictionary<string, int> UsersByVersion
        {
            get; set;
        }

        public Dictionary<string, int> UsersByWinVersion
        {
            get; set;
        }

        public List<User> RecentUsers
        {
            get; set;
        }
    }
}
