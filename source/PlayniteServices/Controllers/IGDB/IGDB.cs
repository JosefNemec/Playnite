using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.IGDB
{
    public class IGDB
    {
        private static ILogger logger = LogManager.GetLogger();

        public static string UrlBase
        {
            get
            {
                return Startup.Configuration.GetSection("IGDB")["ApiEndpoint"];
            }
        }

        public static string ApiKey
        {
            get
            {
                return Startup.Configuration.GetSection("IGDB")["ApiKey"];
            }
        }

        public static string CacheDirectory
        {
            get
            {
                var path = Startup.Configuration.GetSection("IGDB")["CacheDirectory"];
                if (Path.IsPathRooted(path))
                {
                    return path;
                }
                else
                {
                    return Path.Combine(Paths.ExecutingDirectory, path);
                }
            }
        }

        public static string WebHookSecret
        {
            get
            {
                return Startup.Configuration.GetSection("IGDB")["WebHookSecret"];
            }
        }

        public static int SearchCacheTimeout
        {
            get
            {
                return int.Parse(Startup.Configuration.GetSection("IGDB")["SearchCacheTimeout"]);
            }
        }

        public static HttpClient HttpClient
        {
            get;
        } = new HttpClient();

        private static HttpRequestMessage CreateRequest(string url, string query, string apiKey)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(UrlBase + url),
                Method = HttpMethod.Get,
                Content = new StringContent(query)
            };

            request.Headers.Add("user-key", apiKey);
            request.Headers.Add("Accept", "application/json");
            return request;
        }

        public static async Task<string> SendStringRequest(string url, string query)
        {
            logger.Debug($"IGDB Live: {url}, {query}");
            var sharedRequest = CreateRequest(url, query, ApiKey);
            var sharedResponse = await HttpClient.SendAsync(sharedRequest);
            return await sharedResponse.Content.ReadAsStringAsync();
        }

        public static async Task<string> SendDirectRequest(string url)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get
            };

            var response = await HttpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
