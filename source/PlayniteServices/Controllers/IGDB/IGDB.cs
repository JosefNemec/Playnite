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
                return @"https://igdbcom-internet-game-database-v1.p.mashape.com/";
            }
        }

        private static string apiKey;
        public static string ApiKey
        {
            get
            {
                if (!string.IsNullOrEmpty(apiKey))
                {
                    return apiKey;
                }

                var key = Startup.Configuration.GetSection("IGBDKey");
                if (key != null)
                {
                    apiKey = key.Value;
                }

                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new Exception("Missing IGDB API Key.");
                }

                return apiKey;
            }

            set
            {
                apiKey = value;
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
                    httpClient.DefaultRequestHeaders.Add("X-Mashape-Key", ApiKey);
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                }

                return httpClient;
            }
        }
    }
}
