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
    [Route("api/igdb/game")]
    public class GameController : Controller
    {
        [HttpGet("{gameId}")]
        public async Task<ServicesResponse<Game>> Get(UInt64 gameId)
        {            
            var url = string.Format(IGDB.UrlBase + @"games/{0}?fields=name%2Csummary%2Cdevelopers%2Cpublishers%2Cgenres%2Cfirst_release_date%2Ccover%2Cwebsites&limit=40&offset=0&search={0}", gameId);
            var libraryStringResult = await IGDB.HttpClient.GetStringAsync(url);
            var game = JsonConvert.DeserializeObject<List<Game>>(libraryStringResult);
            return new ServicesResponse<Game>(game[0], string.Empty);
        }
    }
}
