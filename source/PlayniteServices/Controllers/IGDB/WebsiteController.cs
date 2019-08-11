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
    [Route("igdb/websites")]
    public class WebsiteController : IgdbItemController
    {
        private static readonly object CacheLock = new object();

        private const string endpointPath = "websites";

        [HttpGet("{websiteId}")]
        public async Task<ServicesResponse<Website>> Get(ulong websiteId)
        {
            return await GetItem(websiteId);
        }

        public static async Task<ServicesResponse<Website>> GetItem(ulong websiteId)
        {
            return new ServicesResponse<Website>(await GetItem<Website>(websiteId, endpointPath, CacheLock));
        }
    }
}
