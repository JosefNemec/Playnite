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
    [Route("api/igdb/games")]
    public class GamesController : Controller
    {
        [HttpGet("{gameName}")]
        public async Task<ServicesResponse<List<Game>>> Get(string gameName, [FromQuery]string apiKey)
        {
            gameName = gameName.ToLower();
            var cacheCollection = Program.DatabaseCache.GetCollection<GamesSearch>("IGBDSearchCache");
            var cache = cacheCollection.FindById(gameName);
            if (cache != null)
            {
                var dateDiff = DateTime.Now - cache.creation_time;
                if (dateDiff.TotalHours <= IGDB.CacheTimeout)
                {
                    return new ServicesResponse<List<Game>>(cache.results, string.Empty);
                }
            }

            var url = string.Format(@"games/?fields=name,first_release_date&limit=40&offset=0&search={0}", gameName);
            var libraryStringResult = await IGDB.SendStringRequest(url, apiKey);
            var games = JsonConvert.DeserializeObject<List<Game>>(libraryStringResult);
            cacheCollection.Upsert(new GamesSearch()
            {
                keyword = gameName,
                results = games,
                creation_time = DateTime.Now
            });

            return new ServicesResponse<List<Game>>(games, string.Empty);
        }
    }
}
