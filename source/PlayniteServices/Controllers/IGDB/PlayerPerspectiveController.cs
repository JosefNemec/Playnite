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
    public class PlayerPerspectiveController : IgdbItemController
    {
        private static readonly object CacheLock = new object();

        private const string endpointPath = "player_perspectives";

        public async Task<ServicesResponse<PlayerPerspective>> Get(ulong persId)
        {
            return await GetItem(persId);
        }

        public static async Task<ServicesResponse<PlayerPerspective>> GetItem(ulong persId)
        {
            return new ServicesResponse<PlayerPerspective>(await GetItem<PlayerPerspective>(persId, endpointPath, CacheLock));
        }
    }
}
