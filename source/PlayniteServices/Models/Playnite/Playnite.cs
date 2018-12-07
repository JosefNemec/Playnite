using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Models.Playnite
{
    public class User
    {
        [BsonId(false)]
        public string Id;
        public string WinVersion;
        public string PlayniteVersion;
        public DateTime LastLaunch;
        public bool Is64Bit;
    }
}
