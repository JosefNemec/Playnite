using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlayniteServices.Models.IGDB
{
    public class SteamIdGame
    {
        [BsonId(false)]
        public ulong steamId { get; set; }
        public ulong igdbId { get; set; }
        public DateTime creation_time { get; set; }
    }
}
