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
    [Route("api/igdb/platforms")]
    public class PlatformController : Controller
    {
        [HttpGet("{platformId}")]
        public async Task<ServicesResponse<Platform>> Get(UInt64 platformId, [FromQuery]string apiKey)
        {
            var cacheCollection = Program.DatabaseCache.GetCollection<Platform>("IGBDPlatformsCache");
            var cache = cacheCollection.FindById(platformId);
            if (cache != null)
            {
                return new ServicesResponse<Platform>(cache, string.Empty);
            }

            var url = string.Format(@"platforms/{0}?fields=name,url", platformId);
            var stringResult = await IGDB.SendStringRequest(url, apiKey);
            var theme = JsonConvert.DeserializeObject<List<Platform>>(stringResult)[0];
            cacheCollection.Insert(theme);            
            return new ServicesResponse<Platform>(theme, string.Empty);
        }
    }
}
