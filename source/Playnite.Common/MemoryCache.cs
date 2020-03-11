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
using System.Windows.Media.Imaging;

namespace Playnite.Common
{
    public class CacheItem
    {
        public object CacheObject
        {
            get;
        }

        public DateTime LastAccess
        {
            get; internal set;
        }

        public DateTime CachedTime
        {
            get;
        }

        public long Size
        {
            get;
        }

        public Dictionary<string, object> Metadata
        {
            get;
        } = new Dictionary<string, object>();

        public CacheItem(object item, long size)
        {
            CacheObject = item;
            CachedTime = DateTime.Now;
            LastAccess = CachedTime;
            Size = size;
        }

        public CacheItem(object item, long size, Dictionary<string, object> metadata) : this (item, size)
        {
            Metadata = metadata;
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

        public void Clear()
        {
            cache.Clear();
            currentSize = 0;
            GC.Collect();
        }

        private void ReleaseOldestItems()
        {
            // ToArray reason:
            // https://stackoverflow.com/questions/11692389/getting-argument-exception-in-concurrent-dictionary-when-sorting-and-displaying
            var items = cache.ToArray().OrderBy(a => a.Value.LastAccess).ToList();
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

        public bool TryAdd(string id, object item, long size, Dictionary<string, object> metadata = null)
        {
            currentSize += size;
            if (currentSize > memorySizeLimit)
            {
                ReleaseOldestItems();
            }

            if (metadata == null)
            {
                return cache.TryAdd(id, new CacheItem(item, size));
            }
            else
            {
                return cache.TryAdd(id, new CacheItem(item, size, metadata));
            }
        }

        public bool TryRemove(string id, out CacheItem item)
        {
            if (cache.TryRemove(id, out var cacheItem))
            {
                item = cacheItem;
                currentSize -= cacheItem.Size;
                return true;
            }
            else
            {
                item = null;
                return false;
            }
        }

        public bool TryRemove(string id)
        {
            if (cache.TryRemove(id, out var cacheItem))
            {
                currentSize -= cacheItem.Size;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryGet(string id, out CacheItem item)
        {
            if (cache.TryGetValue(id, out var cacheItem))
            {
                cacheItem.LastAccess = DateTime.Now;
                item = cacheItem;
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

