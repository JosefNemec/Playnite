using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Playnite
{
    public class Web
    {
        public static string DownloadString(string url)
        {
            var webClient = new WebClient();
            return webClient.DownloadString(url);
        }

        public static void DownloadString(string url, string path)
        {
            var webClient = new WebClient();
            var data = webClient.DownloadString(url);
            File.WriteAllText(path, data);
        }

        public static byte[] DownloadData(string url)
        {
            var webClient = new WebClient();
            return webClient.DownloadData(url);
        }

        public static void DownloadFile(string url, string path)
        {
            FileSystem.CreateFolder(Path.GetDirectoryName(path));
            var webClient = new WebClient();
            webClient.DownloadFile(url, path);
        }

        public static string GetCachedWebFile(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            var extension = Path.GetExtension(url);
            var md5 = url.MD5();
            var cacheFile = Path.Combine(Paths.ImagesCachePath, md5 + extension);

            if (!File.Exists(cacheFile))
            {
                FileSystem.CreateFolder(Paths.ImagesCachePath);

                try
                {
                    DownloadFile(url, cacheFile);
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

            return cacheFile;
        }
    }
}
