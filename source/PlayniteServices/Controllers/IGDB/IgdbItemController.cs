using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Playnite;
using Playnite.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.IGDB
{
    public class IgdbItemController : Controller
    {
        private static readonly char[] arrayTrim = new char[] { '[', ']' };
        private static readonly JsonSerializer jsonSerializer = new JsonSerializer();

        public static async Task<TItem> GetItem<TItem>(ulong itemId, string endpointPath, object cacheLock)
        {
            var cachePath = Path.Combine(IGDB.CacheDirectory, endpointPath, itemId + ".json");
            lock (cacheLock)
            {
                if (System.IO.File.Exists(cachePath))
                {
                    using (var fs = new FileStream(cachePath, FileMode.Open, FileAccess.Read))
                    using (var sr = new StreamReader(fs))
                    using (var reader = new JsonTextReader(sr))
                    {
                        var cacheItem = jsonSerializer.Deserialize<TItem>(reader);
                        if (cacheItem != null)
                        {
                            return cacheItem;
                        }
                    }
                }
            }

            var stringResult = await IGDB.SendStringRequest(endpointPath, $"fields *; where id = {itemId};");
            var items = Serialization.FromJson<List<TItem>>(stringResult);

            TItem item;
            // IGDB resturns empty results of the id is a duplicate of another game
            if (items.Count > 0)
            {
                item = items[0];
            }
            else
            {
                item = typeof(TItem).CrateInstance<TItem>();
            }

            lock (cacheLock)
            {
                FileSystem.PrepareSaveFile(cachePath);

                if (items.Count > 0)
                {
                    System.IO.File.WriteAllText(cachePath, stringResult.Trim(arrayTrim), Encoding.UTF8);
                }
                else
                {
                    System.IO.File.WriteAllText(cachePath, Serialization.ToJson(item), Encoding.UTF8);
                }
            }

            return item;
        }
    }
}
