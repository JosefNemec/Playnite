using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlayniteServices.Filters;
using PlayniteServices.Models.IGDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.IGDB.DataGetter
{
    public class AlternativeNames : DataGetter<AlternativeName>
    {
        private static readonly object cacheLock = new object();
        public AlternativeNames(IgdbApi igdbApi) : base(igdbApi, "alternative_names", cacheLock)
        {
        }
    }
}
