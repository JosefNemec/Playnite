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
    [Route("api/igdb/genre")]
    public class GenreController : Controller
    {
        private static Dictionary<UInt64, Genre> genreCache = new Dictionary<UInt64, Genre>();

        [HttpGet("{genreId}")]
        public async Task<ServicesResponse<Genre>> Get(UInt64 genreId)
        {
            if (genreCache.ContainsKey(genreId))
            {
                return new ServicesResponse<Genre>(genreCache[genreId], string.Empty);
            }

            var url = string.Format(IGDB.UrlBase + @"genres/{0}?fields=name", genreId);
            var stringResult = await IGDB.HttpClient.GetStringAsync(url);
            genreCache.Add(genreId, JsonConvert.DeserializeObject<List<Genre>>(stringResult)[0]);
            return new ServicesResponse<Genre>(genreCache[genreId], string.Empty);
        }
    }
}
