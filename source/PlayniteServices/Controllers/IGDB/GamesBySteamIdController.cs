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
    [Route("api/igdb/gamesBySteamId")]
    public class GamesBySteamIdController : Controller
    {
        [HttpGet("{gameId}")]
        public async Task<ServicesResponse<ulong>> Get(string gameId, [FromQuery]string apiKey)
        {
            var ulongId = ulong.Parse(gameId);
            var cacheCollection = Program.DatabaseCache.GetCollection<SteamIdGame>("IGBDSteamIdCache");
            var cache = cacheCollection.FindById(ulongId);
            if (cache != null)
            {
                var dateDiff = DateTime.Now - cache.creation_time;
                if (dateDiff.TotalHours <= (IGDB.CacheTimeout + 100))
                {
                    return new ServicesResponse<ulong>(cache.igdbId, string.Empty);
                }
            }

            var url = string.Format(@"/games/?fields=name,id&filter[external.steam][eq]={0}&limit=1", gameId);
            var libraryStringResult = await IGDB.SendStringRequest(url, apiKey);
            var games = JsonConvert.DeserializeObject<List<Game>>(libraryStringResult);
            if (games.Any())
            {
                cacheCollection.Upsert(new SteamIdGame()
                {
                    steamId = ulongId,
                    igdbId = games.First().id,
                    creation_time = DateTime.Now
                });

                return new ServicesResponse<ulong>(games.First().id, string.Empty);
            }
            else
            {
                return new ServicesResponse<ulong>(0, string.Empty);
            }
        }
    }
}
