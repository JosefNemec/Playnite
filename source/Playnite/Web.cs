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
            return DownloadString(url, Encoding.UTF8);
        }

        public static string DownloadString(string url, Encoding encoding)
        {
            var webClient = new WebClient { Encoding = encoding };
            return webClient.DownloadString(url);            
        }

        public static string DownloadString(string url, List<Cookie> cookies)
        {
            return DownloadString(url, cookies, Encoding.UTF8);
        }

        public static string DownloadString(string url, List<Cookie> cookies, Encoding encoding)
        {
            var webClient = new WebClient { Encoding = encoding };
            if (cookies?.Any() == true)
            {
                var cookieString = string.Join(";", cookies.Select(a => $"{a.Name}={a.Value}"));
                webClient.Headers.Add(HttpRequestHeader.Cookie, cookieString);
            }

            return webClient.DownloadString(url);
        }

        public static void DownloadString(string url, string path)
        {
            DownloadString(url, path, Encoding.UTF8);
        }

        public static void DownloadString(string url, string path, Encoding encoding)
        {
            var webClient = new WebClient { Encoding = Encoding.UTF8 };
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
            FileSystem.CreateDirectory(Path.GetDirectoryName(path));
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
                FileSystem.CreateDirectory(Paths.ImagesCachePath);

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
