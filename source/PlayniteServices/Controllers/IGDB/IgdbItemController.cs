using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Playnite;
using Playnite.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.IGDB
{
    public class IgdbItemController : Controller
    {
        public async Task<TItem> GetItem<TItem>(ulong itemId, string endpointPath, object cacheLock)
        {
            var cachePath = Path.Combine(IGDB.CacheDirectory, endpointPath, itemId + ".json");
            lock (cacheLock)
            {
                if (System.IO.File.Exists(cachePath))
                {
                    var cacheItem = JsonConvert.DeserializeObject<TItem>(System.IO.File.ReadAllText(cachePath));
                    if (cacheItem != null)
                    {
                        return cacheItem;
                    }
                }
            }

            var url = string.Format(endpointPath + @"/{0}?fields=*", itemId);
            var stringResult = await IGDB.SendStringRequest(url);
            var item = Serialization.FromJson<List<TItem>>(stringResult)[0];
            lock (cacheLock)
            {
                FileSystem.PrepareSaveFile(cachePath);
                System.IO.File.WriteAllText(cachePath, Serialization.ToJson(item));
            }

            return item;
        }
    }
}
