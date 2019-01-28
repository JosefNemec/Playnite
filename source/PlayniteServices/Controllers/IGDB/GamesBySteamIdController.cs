using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlayniteServices.Models.IGDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LiteDB;

namespace PlayniteServices.Controllers.IGDB
{
    [Route("igdb/gamesBySteamId")]
    public class GamesBySteamIdController : Controller
    {
        private const string cacheDir = "steam_ids";

        public const string DbCollectionName = "IGBDSteamIdCache";

        private static LiteCollection<SteamIdGame> cacheCollection = Program.Database.GetCollection<SteamIdGame>(DbCollectionName);

        [HttpGet("{gameId}")]
        public async Task<ServicesResponse<ulong>> Get(ulong gameId)
        {
            var cache = cacheCollection.FindById(gameId);
            if (cache != null)
            {
                return new ServicesResponse<ulong>(cache.igdbId);
            }

            var url = string.Format(@"/games/?fields=name,id&filter[external.steam][eq]={0}&limit=1", gameId);
            var libraryStringResult = await IGDB.SendStringRequest(url);
            var games = JsonConvert.DeserializeObject<List<Game>>(libraryStringResult);
            if (games.Any())
            {
                cacheCollection.Upsert(new SteamIdGame()
                {
                    steamId = gameId,
                    igdbId = games.First().id,
                    creation_time = DateTime.Now
                });

                return new ServicesResponse<ulong>(games.First().id);
            }
            else
            {
                return new ServicesResponse<ulong>(0);
            }
        }
    }
}
