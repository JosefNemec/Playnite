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
    [Route("igdb/game_modes")]
    public class GameModeController : IgdbItemController
    {
        private static readonly object CacheLock = new object();

        private const string endpointPath = "game_modes";

        [HttpGet("{modeId}")]
        public async Task<ServicesResponse<GameMode>> Get(ulong modeId)
        {
            return new ServicesResponse<GameMode>(await GetItem<GameMode>(modeId, endpointPath, CacheLock));
        }
    }
}
