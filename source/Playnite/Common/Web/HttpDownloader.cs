using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using Playnite.SDK;
using System.Threading;

namespace Playnite.Common.Web
{
    public class HttpDownloader
    {
        private static ILogger logger = LogManager.GetLogger();
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly Downloader downloader = new Downloader();

        public static string DownloadString(IEnumerable<string> mirrors)
        {
            return downloader.DownloadString(mirrors);
        }

        public static string DownloadString(string url)
        {
            return downloader.DownloadString(url);
        }

        public static string DownloadString(string url, Encoding encoding)
        {
            return downloader.DownloadString(url, encoding);
        }

        public static string DownloadString(string url, List<Cookie> cookies)
        {
            return downloader.DownloadString(url, cookies);
        }

        public static string DownloadString(string url, List<Cookie> cookies, Encoding encoding)
        {
            return downloader.DownloadString(url, cookies, encoding);
        }

        public static void DownloadString(string url, string path)
        {
            downloader.DownloadString(url, path);
        }

        public static void DownloadString(string url, string path, Encoding encoding)
        {
            downloader.DownloadString(url, path, encoding);
        }

        public static byte[] DownloadData(string url)
        {
            return downloader.DownloadData(url);
        }

        public static void DownloadFile(string url, string path)
        {
            downloader.DownloadFile(url, path);
        }

        public static void DownloadFile(string url, string path, CancellationTokenSource cancelToken)
        {
            downloader.DownloadFile(url, path, cancelToken);
        }

        public static HttpStatusCode GetResponseCode(string url)
        {
            try
            {
                var response = httpClient.GetAsync(url).GetAwaiter().GetResult();
                return response.StatusCode;
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to get HTTP response for {url}.");
                return HttpStatusCode.ServiceUnavailable;
            }
        }
    }
}
