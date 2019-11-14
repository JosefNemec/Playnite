using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlayniteServices.Filters;
using PlayniteServices.Models.IGDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.IGDB
{
    [ServiceFilter(typeof(PlayniteVersionFilter))]
    public class PlatformController : IgdbItemController
    {
        private static readonly object CacheLock = new object();

        private const string endpointPath = "platforms";

        public async Task<ServicesResponse<Platform>> Get(ulong platformId)
        {
            return new ServicesResponse<Platform>(await GetItem<Platform>(platformId, endpointPath, CacheLock));
        }
    }
}
