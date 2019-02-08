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
    [Route("igdb/company")]
    public class CompanyController : IgdbItemController
    {
        private static readonly object CacheLock = new object();

        private const string endpointPath = "companies";

        [HttpGet("{companyId}")]
        public async Task<ServicesResponse<Company>> Get(ulong companyId)
        {
            return new ServicesResponse<Company>(await GetItem<Company>(companyId, endpointPath, CacheLock));
        }
    }
}
