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
    public class GenreController : IgdbItemController
    {
        private static readonly object CacheLock = new object();

        private const string endpointPath = "genres";

        public async Task<ServicesResponse<Genre>> Get(ulong genreId)
        {
            return await GetItem(genreId);
        }

        public static async Task<ServicesResponse<Genre>> GetItem(ulong genreId)
        {
            return new ServicesResponse<Genre>(await GetItem<Genre>(genreId, endpointPath, CacheLock));
        }
    }
}
