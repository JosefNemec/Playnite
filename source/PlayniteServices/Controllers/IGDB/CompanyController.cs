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
    [Route("api/igdb/company")]
    public class CompanyController : Controller
    {
        private static Dictionary<UInt64, Company> companyCache = new Dictionary<UInt64, Company>();

        [HttpGet("{companyId}")]
        public async Task<ServicesResponse<Company>> Get(UInt64 companyId)
        {
            if (companyCache.ContainsKey(companyId))
            {
                return new ServicesResponse<Company>(companyCache[companyId], string.Empty);
            }

            var url = string.Format(IGDB.UrlBase + @"companies/{0}?fields=name", companyId);
            var stringResult = await IGDB.HttpClient.GetStringAsync(url);
            companyCache.Add(companyId, JsonConvert.DeserializeObject<List<Company>>(stringResult)[0]);
            return new ServicesResponse<Company>(companyCache[companyId], string.Empty);
        }
    }
}
