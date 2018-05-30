using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Web
{
    public interface IDownloader
    {
        string DownloadString(IEnumerable<string> mirrors);

        string DownloadString(string url);

        string DownloadString(string url, Encoding encoding);

        string DownloadString(string url, List<Cookie> cookies);

        string DownloadString(string url, List<Cookie> cookies, Encoding encoding);

        void DownloadString(string url, string path);

        void DownloadString(string url, string path, Encoding encoding);

        byte[] DownloadData(string url);

        void DownloadFile(string url, string path);

        string GetCachedWebFile(string url);
    }

    public class Downloader : IDownloader
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public Downloader()
        {
        }

        public string DownloadString(IEnumerable<string> mirrors)
        {
            foreach (var mirror in mirrors)
            {
                try
                {
                    return DownloadString(mirror);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to download {mirror} file.");
                }
            }

            throw new Exception("Failed to download string from all mirrors.");
        }

        public string DownloadString(string url)
        {
            return DownloadString(url, Encoding.UTF8);
        }

        public string DownloadString(string url, Encoding encoding)
        {
            var webClient = new WebClient { Encoding = encoding };
            return webClient.DownloadString(url);
        }

        public string DownloadString(string url, List<Cookie> cookies)
        {
            return DownloadString(url, cookies, Encoding.UTF8);
        }

        public string DownloadString(string url, List<Cookie> cookies, Encoding encoding)
        {
            var webClient = new WebClient { Encoding = encoding };
            if (cookies?.Any() == true)
            {
                var cookieString = string.Join(";", cookies.Select(a => $"{a.Name}={a.Value}"));
                webClient.Headers.Add(HttpRequestHeader.Cookie, cookieString);
            }

            return webClient.DownloadString(url);
        }

        public void DownloadString(string url, string path)
        {
            DownloadString(url, path, Encoding.UTF8);
        }

        public void DownloadString(string url, string path, Encoding encoding)
        {
            var webClient = new WebClient { Encoding = Encoding.UTF8 };
            var data = webClient.DownloadString(url);
            File.WriteAllText(path, data);
        }

        public byte[] DownloadData(string url)
        {
            var webClient = new WebClient();
            return webClient.DownloadData(url);
        }

        public void DownloadFile(string url, string path)
        {
            FileSystem.CreateDirectory(Path.GetDirectoryName(path));
            var webClient = new WebClient();
            webClient.DownloadFile(url, path);
        }

        public string GetCachedWebFile(string url)
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
