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
    [Route("igdb/franchise")]
    public class FranchiseController : IgdbItemController
    {
        private static readonly object CacheLock = new object();

        private const string endpointPath = "franchises";

        [HttpGet("{franchiseId}")]
        public async Task<ServicesResponse<Franchise>> Get(ulong franchiseId)
        {
            return new ServicesResponse<Franchise>(await GetItem<Franchise>(franchiseId, endpointPath, CacheLock));
        }
    }
}
