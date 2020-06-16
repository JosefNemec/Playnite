using JsonApiSerializer;
using JsonApiSerializer.JsonApi;
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
                var allPatrons = new List<string>();
                var nextLink = "api/campaigns/1400397/pledges?include=patron.null&page%5Bcount%5D=9999";

                do
                {
                    var stringData = await Patreon.SendStringRequest(nextLink);
                    var document = JsonConvert.DeserializeObject<DocumentRoot<Pledge[]>>(stringData, new JsonApiSerializerSettings());
                    allPatrons.AddRange(document.Data.Where(a => a.declined_since == null).Select(a => a.patron.full_name));
                    if (document.Links.ContainsKey("next"))
                    {
                        nextLink = document.Links["next"].Href;
                    }
                    else
                    {
                        break;
                    }
                }
                while (!string.IsNullOrEmpty(nextLink));

                cache = new ListCache(allPatrons.OrderBy(a => a).ToList());
            }

            return new ServicesResponse<List<string>>(cache.Names);
        }
    }
}
