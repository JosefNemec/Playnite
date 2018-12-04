using Playnite.SDK;
using Playnite.Settings;
using Playnite.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class HttpFileCache
    {
        private static ILogger logger = LogManager.GetLogger();

        public static string CacheDirectory { get; set; } = PlaynitePaths.ImagesCachePath;

        private static string GetFileNameFromUrl(string url)
        {
            var extension = Path.GetExtension(url);
            var md5 = url.MD5();
            return md5 + extension;
        }

        public static string GetWebFile(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            var cacheFile = Path.Combine(CacheDirectory, GetFileNameFromUrl(url));
            if (File.Exists(cacheFile) && (new FileInfo(cacheFile)).Length != 0)
            {
                logger.Debug($"Returning {url} from file cache {cacheFile}.");
                return cacheFile;
            }
            else
            {
                FileSystem.CreateDirectory(CacheDirectory);

                try
                {
                    HttpDownloader.DownloadFile(url, cacheFile);
                    return cacheFile;
                }
                catch (WebException e)
                {
                    if (e.Response == null)
                    {
                        throw;
                    }

                    var response = (HttpWebResponse)e.Response;
                    if (response.StatusCode != HttpStatusCode.NotFound)
                    {
                        throw;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
        }

        public static void ClearCache(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            var cacheFile = Path.Combine(CacheDirectory, GetFileNameFromUrl(url));
            if (File.Exists(cacheFile))
            {
                logger.Debug($"Removing {url} from file cache: {cacheFile}");
                FileSystem.DeleteFileSafe(cacheFile);
            }

        }
    }
}
