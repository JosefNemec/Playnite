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
        [BsonIndex(true)]
        public string Name
        {
            get; set;
        }

        public string Id
        {
            get; set;
        }

        public DateTime UpdateDate
        {
            get; set;
        }
    }

    public class SteamStoreAppCache
    {
        [BsonId(false)]
        [BsonIndex(true)]
        public string Id
        {
            get; set;
        }

        public string Data
        {
            get; set;
        }

        public DateTime CreationTime
        {
            get; set;
        }
    }

    public class SteamAppInfoCache
    {
        [BsonId(false)]
        [BsonIndex(true)]
        public string Id
        {
            get; set;
        }

        public string Data
        {
            get; set;
        }

        public DateTime CreationTime
        {
            get; set;
        }
    }
}
