using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Models.Steam
{
    public class SteamNameCache
    {
        [BsonId(false)]
        public string Name;
        public string Id;
        public DateTime UpdateDate;
    }
}
