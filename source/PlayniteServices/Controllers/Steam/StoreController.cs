using Microsoft.AspNetCore.Mvc;
using PlayniteServices.Models.Steam;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.Steam
{
    [Route("api/steam/store")]
    public class StoreController : Controller
    {
        [HttpGet("{appId}")]
        public ServicesResponse<string> Get(string appId)
        {
            var appsColl = Program.DatabaseCache.GetCollection<SteamStoreAppCache>("SteamStoreCache");
            var cache = appsColl.FindById(appId);
            if (cache != null)
            {
                var dateDiff = DateTime.Now - cache.CreationTime;
                if (dateDiff.TotalHours <= Steam.StoreCacheTimeout)
                {
                    return new ServicesResponse<string>(cache.Data, string.Empty);
                }
            }

            return new ServicesResponse<string>(string.Empty, string.Empty);
        }

        [HttpPost("{appId}")]
        public async Task<IActionResult> PostApp(string appId)
        {
            string data = string.Empty;
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                data = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrEmpty(data))
            {
                return BadRequest(new GenericResponse(null, "No data provided."));
            }

            var appsColl = Program.DatabaseCache.GetCollection<SteamStoreAppCache>("SteamStoreCache");
            var storeItem = new SteamStoreAppCache()
            {
                Id = appId,
                Data = data,
                CreationTime = DateTime.Now
            };

            appsColl.Upsert(storeItem);
            return Ok();
        }
    }
}
