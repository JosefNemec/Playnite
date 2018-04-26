using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.IGDB
{
    public class IGDB
    {
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

        public static int CacheTimeout
        {
            get
            {
                return int.Parse(Startup.Configuration.GetSection("IGDB")["CacheTimeout"]);
            }
        }

        private static HttpClient httpClient;
        public static HttpClient HttpClient
        {
            get
            {
                if (httpClient == null)
                {
                    httpClient = new HttpClient();
                    
                }

                return httpClient;
            }
        }

        private static HttpRequestMessage CreateRequest(string url, string apiKey)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(UrlBase + url),
                Method = HttpMethod.Get
            };

            request.Headers.Add("user-key", apiKey);
            request.Headers.Add("Accept", "application/json");
            return request;
        }

        public static async Task<string> SendStringRequest(string url, string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var request = CreateRequest(url, key);
                var response = await HttpClient.SendAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }

            var sharedRequest = CreateRequest(url, ApiKey);
            var sharedResponse = await HttpClient.SendAsync(sharedRequest);
            return await sharedResponse.Content.ReadAsStringAsync();
        }
    }
}
