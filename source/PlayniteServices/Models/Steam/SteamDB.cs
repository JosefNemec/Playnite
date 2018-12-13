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
        public string Name { get; set; }
        public string Id { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
