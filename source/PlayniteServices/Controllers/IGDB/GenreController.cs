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
    [Route("igdb/genre")]
    public class GenreController : IgdbItemController
    {
        private static readonly object CacheLock = new object();

        private const string endpointPath = "genres";

        [HttpGet("{genreId}")]
        public async Task<ServicesResponse<Genre>> Get(ulong genreId)
        {
            return new ServicesResponse<Genre>(await GetItem<Genre>(genreId, endpointPath, CacheLock));
        }
    }
}
