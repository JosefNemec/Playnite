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
    public class FranchiseController : IgdbItemController
    {
        private static readonly object CacheLock = new object();

        private const string endpointPath = "franchises";

        public async Task<ServicesResponse<Franchise>> Get(ulong franchiseId)
        {
            return new ServicesResponse<Franchise>(await GetItem<Franchise>(franchiseId, endpointPath, CacheLock));
        }
    }
}
