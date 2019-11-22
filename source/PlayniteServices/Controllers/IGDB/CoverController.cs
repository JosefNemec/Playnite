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
    public class CoverController : IgdbItemController
    {
        private static readonly object CacheLock = new object();

        private const string endpointPath = "covers";

        public async Task<ServicesResponse<GameImage>> Get(ulong coverId)
        {
            return await GetItem(coverId);
        }

        public static async Task<ServicesResponse<GameImage>> GetItem(ulong coverId)
        {
            return new ServicesResponse<GameImage>(await GetItem<GameImage>(coverId, endpointPath, CacheLock));
        }
    }
}
