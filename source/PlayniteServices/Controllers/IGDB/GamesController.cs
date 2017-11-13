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
    [Route("api/igdb/games")]
    public class GamesController : Controller
    {
        [HttpGet("{gameName}")]
        public async Task<ServicesResponse<List<Game>>> Get(string gameName)
        {            
            var url = string.Format(IGDB.UrlBase + @"games/?fields=name,first_release_date&limit=40&offset=0&search={0}", gameName);
            var libraryStringResult = await IGDB.HttpClient.GetStringAsync(url);
            var games = JsonConvert.DeserializeObject<List<Game>>(libraryStringResult);
            return new ServicesResponse<List<Game>>(games, string.Empty);
        }
    }
}
