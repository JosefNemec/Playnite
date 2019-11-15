using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlayniteServices.Models.IGDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LiteDB;
using PlayniteServices.Filters;
using PlayniteServices.Databases;

namespace PlayniteServices.Controllers.IGDB
{
    [ServiceFilter(typeof(PlayniteVersionFilter))]
    [Route("igdb/gamesBySteamId")]
    public class GamesBySteamIdController : Controller
    {
        [HttpGet("{gameId}")]
        public async Task<ServicesResponse<ulong>> Get(ulong gameId)
        {
            return new ServicesResponse<ulong>(await GetIgdbMatch(gameId));
        }

        public static async Task<ulong> GetIgdbMatch(ulong gameId)
        {
            var cache = Database.SteamIgdbMatches.FindById(gameId);
            if (cache != null)
            {
                return cache.igdbId;
            }

            var libraryStringResult = await IGDB.SendStringRequest("games",
                $"fields id; where external_games.uid = \"{gameId}\" & external_games.category = 1; limit 1;");
            var games = JsonConvert.DeserializeObject<List<Game>>(libraryStringResult);
            if (games.Any())
            {
                Database.SteamIgdbMatches.Upsert(new SteamIdGame()
                {
                    steamId = gameId,
                    igdbId = games.First().id
                });

                return games.First().id;
            }
            else
            {
                return 0;
            }
        }
    }
}
