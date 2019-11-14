using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlayniteServices.Filters;
using PlayniteServices.Models.IGDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.IGDB
{
    [ServiceFilter(typeof(PlayniteVersionFilter))]
    public class ScreenshotController : IgdbItemController
    {
        private static readonly object CacheLock = new object();

        private const string endpointPath = "screenshots";

        public async Task<ServicesResponse<GameImage>> Get(ulong screenshotId)
        {
            return await GetItem(screenshotId);
        }

        public static async Task<ServicesResponse<GameImage>> GetItem(ulong screenshotId)
        {
            return new ServicesResponse<GameImage>(await GetItem<GameImage>(screenshotId, endpointPath, CacheLock));
        }
    }
}
