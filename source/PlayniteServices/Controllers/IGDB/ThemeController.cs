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
    [Route("api/igdb/themes")]
    public class ThemeController : Controller
    {
        [HttpGet("{themeId}")]
        public async Task<ServicesResponse<Theme>> Get(UInt64 themeId)
        {
            var cacheCollection = Program.DatabaseCache.GetCollection<Theme>("IGBDThemesCache");
            var cache = cacheCollection.FindById(themeId);
            if (cache != null)
            {
                return new ServicesResponse<Theme>(cache, string.Empty);
            }

            var url = string.Format(IGDB.UrlBase + @"themes/{0}?fields=name", themeId);
            var stringResult = await IGDB.HttpClient.GetStringAsync(url);
            var theme = JsonConvert.DeserializeObject<List<Theme>>(stringResult)[0];
            cacheCollection.Insert(theme);            
            return new ServicesResponse<Theme>(theme, string.Empty);
        }
    }
}
