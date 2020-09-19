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
    public class AgeRatingController : IgdbItemController
    {
        private static readonly object CacheLock = new object();

        private const string endpointPath = "age_ratings";

        public async Task<ServicesResponse<AgeRating>> Get(ulong ageId)
        {
            return await GetItem(ageId);
        }

        public static async Task<ServicesResponse<AgeRating>> GetItem(ulong ageId)
        {
            return new ServicesResponse<AgeRating>(await GetItem<AgeRating>(ageId, endpointPath, CacheLock));
        }
    }
}
