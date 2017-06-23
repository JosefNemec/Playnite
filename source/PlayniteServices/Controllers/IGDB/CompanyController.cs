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
        [HttpGet("{companyId}")]
        public async Task<ServicesResponse<Company>> Get(UInt64 companyId)
        {
            var cacheCollection = Program.DatabaseCache.GetCollection<Company>("IGBDCompaniesCache");
            var cache = cacheCollection.FindById(companyId);
            if (cache != null)
            {
                return new ServicesResponse<Company>(cache, string.Empty);
            }

            var url = string.Format(IGDB.UrlBase + @"companies/{0}?fields=name", companyId);
            var stringResult = await IGDB.HttpClient.GetStringAsync(url);
            var company = JsonConvert.DeserializeObject<List<Company>>(stringResult)[0];
            cacheCollection.Insert(company);            
            return new ServicesResponse<Company>(company, string.Empty);
        }
    }
}
