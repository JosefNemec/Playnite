using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlayniteServices.Filters;
using PlayniteServices.Models.IGDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.IGDB
{
    [ServiceFilter(typeof(PlayniteVersionFilter))]
    public class ThemeController : IgdbItemController
    {
        private static readonly object CacheLock = new object();

        private const string endpointPath = "themes";

        public async Task<ServicesResponse<Theme>> Get(ulong themeId)
        {
            return await GetItem(themeId);
        }

        public static async Task<ServicesResponse<Theme>> GetItem(ulong themeId)
        {
            return new ServicesResponse<Theme>(await GetItem<Theme>(themeId, endpointPath, CacheLock));
        }
    }
}
