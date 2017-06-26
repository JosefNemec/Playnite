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
        [HttpGet("{genreId}")]
        public async Task<ServicesResponse<Genre>> Get(UInt64 genreId)
        {
            var cacheCollection = Program.DatabaseCache.GetCollection<Genre>("IGBDGenresCache");
            var cache = cacheCollection.FindById(genreId);
            if (cache != null)
            {
                return new ServicesResponse<Genre>(cache, string.Empty);
            }

            var url = string.Format(IGDB.UrlBase + @"genres/{0}?fields=name", genreId);
            var stringResult = await IGDB.HttpClient.GetStringAsync(url);
            var genre = JsonConvert.DeserializeObject<List<Genre>>(stringResult)[0];
            cacheCollection.Insert(genre);
            return new ServicesResponse<Genre>(genre, string.Empty);
        }
    }
}
