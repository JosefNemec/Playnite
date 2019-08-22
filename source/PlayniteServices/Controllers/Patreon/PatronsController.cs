using JsonApiSerializer;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlayniteServices.Filters;
using PlayniteServices.Models.Patreon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.Patreon
{
    [ServiceFilter(typeof(PlayniteVersionFilter))]
    [Route("patreon/patrons")]
    public class PatronsController : Controller
    {
        public class ListCache
        {
            public DateTime Created;
            public List<string> Names;

            public ListCache(List<string> names)
            {
                Created = DateTime.Now;
                Names = names;
            }
        }

        private static ListCache cache;

        [HttpGet]
        public async Task<ServicesResponse<List<string>>> Get()
        {
            if (cache == null || (DateTime.Now - cache.Created).TotalSeconds > 60)
            {
                var stringData = await Patreon.SendStringRequest("api/campaigns/1400397/pledges?include=patron.null&page%5Bcount%5D=9999");
                var pledges = JsonConvert.DeserializeObject<Pledge[]>(stringData, new JsonApiSerializerSettings());
                var patrons = pledges.Select(a => a.patron.full_name).ToList();
                cache = new ListCache(patrons);
            }

            return new ServicesResponse<List<string>>(cache.Names);
        }
    }
}
