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
    [Route("igdb/nameRecognition")]
    public class NameRecognitionController : Controller
    {
        [HttpGet("{gameName}")]
        public async Task<ServicesResponse<ulong>> Get(string gameName, [FromQuery]string apiKey)
        {
            //var cacheCollection = Program.DatabaseCache.GetCollection<SteamIdGame>("IGBDSteamIdCache");
            //var cache = cacheCollection.FindById(ulongId);
            //if (cache != null)
            //{
            //    var dateDiff = DateTime.Now - cache.creation_time;
            //    if (dateDiff.TotalHours <= (IGDB.CacheTimeout + 100))
            //    {
            //        return new ServicesResponse<ulong>(cache.igdbId, string.Empty);
            //    }
            //}

            var url = string.Format(@"https://namematcher.igdb.com/entityrecognition?subject={0}&limit=5", gameName);
            var libraryStringResult = await IGDB.SendDirectRequest(url);
            var games = JsonConvert.DeserializeObject<List<Game>>(libraryStringResult);
            if (games.Any())
            {
                //cacheCollection.Upsert(new SteamIdGame()
                //{
                //    steamId = ulongId,
                //    igdbId = games.First().id,
                //    creation_time = DateTime.Now
                //});

                return new ServicesResponse<ulong>(games.First().id);
            }
            else
            {
                return new ServicesResponse<ulong>(0);
            }
        }
    }
}
