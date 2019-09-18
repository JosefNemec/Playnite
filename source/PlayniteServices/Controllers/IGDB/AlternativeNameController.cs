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
    [Route("igdb/alternative_names")]
    public class AlternativeNameController : IgdbItemController
    {
        private static readonly object CacheLock = new object();

        private const string endpointPath = "alternative_names";

        [HttpGet("{nameId}")]
        public async Task<ServicesResponse<AlternativeName>> Get(ulong nameId)
        {
            return await GetItem(nameId);
        }

        public static async Task<ServicesResponse<AlternativeName>> GetItem(ulong nameId)
        {
            return new ServicesResponse<AlternativeName>(await GetItem<AlternativeName>(nameId, endpointPath, CacheLock));
        }
    }
}
