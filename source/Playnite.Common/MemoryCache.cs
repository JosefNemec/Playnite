using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Common
{
   
    public class CacheItem
    {
        private object cacheItem;

        public DateTime LastAccess
        {
            get; private set;
        }

        public DateTime CachedTime
        {
            get;
        }

        public long Size
        {
            get;
        }

        public CacheItem(object item, long size)
        {
            cacheItem = item;
            CachedTime = DateTime.Now;
            LastAccess = CachedTime;
            Size = size;
        }

        public object GetItem()
        {
            LastAccess = DateTime.Now;
            return cacheItem;            
        }
    }

    public class MemoryCache
    {
        private ConcurrentDictionary<string, CacheItem> cache = new ConcurrentDictionary<string, CacheItem>();
        private long memorySizeLimit = 0;
        private long currentSize = 0;

        public MemoryCache(long memoryLimit)
        {            
            memorySizeLimit = memoryLimit;
        }

        private void ReleaseOldestItems()
        {
            var items = cache.OrderBy(a => a.Value.LastAccess).ToList();
            foreach (var item in items)
            {
                if (currentSize > memorySizeLimit)
                {
                    TryRemove(item.Key, out var removed);
                }
                else
                {
                    break;
                }
            }
        }

        public bool TryAdd(string id, object item, long size)
        {
            currentSize += size;
            if (currentSize > memorySizeLimit)
            {
                ReleaseOldestItems();
            }

            return cache.TryAdd(id, new CacheItem(item, size));
        }

        public bool TryRemove(string id, out object item)
        {
            if (cache.TryRemove(id, out var cacheItem))
            {
                item = cacheItem.GetItem();
                currentSize -= cacheItem.Size;
                return true;
            }
            else
            {
                item = null;
                return false;
            }
        }

        public bool TryGet(string id, out object item)
        {
            if (cache.TryGetValue(id, out var cacheItem))
            {
                item = cacheItem.GetItem();
                return true;
            }
            else
            {
                item = null;
                return false;
            }
        }
    }
}

