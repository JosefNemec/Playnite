using Microsoft.AspNetCore.Mvc;
using PlayniteServices.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.IGDB
{
    [ServiceFilter(typeof(ServiceKeyFilter))]
    [Route("igdb/maintenance")]
    public class MaintenanceController : Controller
    {
        private UpdatableAppSettings settings;

        public MaintenanceController(UpdatableAppSettings settings)
        {
            this.settings = settings;
        }

        [HttpDelete("cache/{collectionName}/{objectId}")]
        public IActionResult DeleteCache(string collectionName, uint objectId)
        {
            var itemPath = Path.Combine(
                settings.Settings.IGDB.CacheDirectory,
                collectionName,
                $"{objectId}.json");
            if (System.IO.File.Exists(itemPath))
            {
                System.IO.File.Delete(itemPath);
            }

            return Ok();
        }
    }
}
