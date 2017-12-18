using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlayniteServices.Models.IGDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.IGDB
{
    [Route("api/igdb/game_modes")]
    public class GameModeController : Controller
    {
        [HttpGet("{modeId}")]
        public async Task<ServicesResponse<GameMode>> Get(UInt64 modeId, [FromQuery]string apiKey)
        {
            var cacheCollection = Program.DatabaseCache.GetCollection<GameMode>("IGBDGameModesCache");
            var cache = cacheCollection.FindById(modeId);
            if (cache != null)
            {
                return new ServicesResponse<GameMode>(cache, string.Empty);
            }

            var url = string.Format(@"game_modes/{0}?fields=name", modeId);
            var stringResult = await IGDB.SendStringRequest(url, apiKey);
            var gameMode = JsonConvert.DeserializeObject<List<GameMode>>(stringResult)[0];
            cacheCollection.Insert(gameMode);            
            return new ServicesResponse<GameMode>(gameMode, string.Empty);
        }
    }
}
