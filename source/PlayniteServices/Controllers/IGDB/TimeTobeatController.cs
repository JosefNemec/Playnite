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
    public class TimeTobeatController : IgdbItemController
    {
        private static readonly object CacheLock = new object();

        private const string endpointPath = "time_to_beats";

        public async Task<ServicesResponse<TimeTobeat>> Get(ulong timetobeatId)
        {
            return new ServicesResponse<TimeTobeat>(await GetItem<TimeTobeat>(timetobeatId, endpointPath, CacheLock));
        }
    }
}
