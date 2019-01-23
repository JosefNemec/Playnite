using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlayniteServices.Models.IGDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.IGDB
{
    [Route("igdb/themes")]
    public class ThemeController : IgdbItemController
    {
        private static readonly object CacheLock = new object();

        private const string endpointPath = "themes";

        [HttpGet("{themeId}")]
        public async Task<ServicesResponse<Theme>> Get(ulong themeId)
        {
            return new ServicesResponse<Theme>(await GetItem<Theme>(themeId, endpointPath, CacheLock));
        }
    }
}
