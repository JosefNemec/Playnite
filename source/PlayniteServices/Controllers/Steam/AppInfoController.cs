using Microsoft.AspNetCore.Mvc;
using Playnite;
using PlayniteServices.Filters;
using PlayniteServices.Models.Steam;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.Steam
{
    [ServiceFilter(typeof(PlayniteVersionFilter))]
    [Route("steam/appinfo")]
    public class AppInfoController : Controller
    {
        private static readonly object CacheLock = new object();

        private const string cacheDir = "appinfo";

        [HttpGet("{appId}")]
        public ServicesResponse<string> Get(string appId)
        {
            var cachePath = Path.Combine(Steam.CacheDirectory, cacheDir, appId + ".txt");
            lock (CacheLock)
            {
                if (System.IO.File.Exists(cachePath))
                {
                    var fileInfo = new FileInfo(cachePath);
                    fileInfo.Refresh();
                    if ((DateTime.Now - fileInfo.LastWriteTime).TotalHours <= Steam.AppInfoCacheTimeout)
                    {
                        return new ServicesResponse<string>(System.IO.File.ReadAllText(cachePath));
                    }
                }
            }

            return new ServicesResponse<string>(string.Empty);
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
                return BadRequest(new ErrorResponse(new Exception("No data provided.")));
            }

            var cachePath = Path.Combine(Steam.CacheDirectory, cacheDir, appId + ".txt");
            lock (CacheLock)
            {
                Playnite.Common.FileSystem.PrepareSaveFile(cachePath);
                System.IO.File.WriteAllText(cachePath, data);
            }

            return Ok();
        }
    }
}
