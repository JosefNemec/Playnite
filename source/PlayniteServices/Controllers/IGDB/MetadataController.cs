using Microsoft.AspNetCore.Mvc;
using Playnite.SDK;
using Playnite.SDK.Models;
using PlayniteServices.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.IGDB
{
    [ServiceFilter(typeof(PlayniteVersionFilter))]
    [Route("igdb/metadata")]
    public class MetadataController : Controller
    {
        private static ILogger logger = LogManager.GetLogger();

        [HttpGet]
        public async Task<ServicesResponse<object>> Get([FromBody]Game game)
        {


            return new ServicesResponse<object>(null);
        }
    }
}
