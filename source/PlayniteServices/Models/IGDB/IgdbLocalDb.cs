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
    }

    public class GameIdMatch
    {
        [BsonId(false)]
        public string Id { get; set; }
        public Guid Library { get; set; }
        public string GameId { get; set; }
        public ulong IgdbId { get; set; }
    }

    public class SearchIdMatch
    {
        [BsonId(false)]
        public string Id { get; set; }
        public string Term { get; set; }
        public ulong IgdbId { get; set; }
    }

}
