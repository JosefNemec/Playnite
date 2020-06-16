using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.Patreon
{
    public class Patreon
    {
        public static string Endpoint
        {
            get
            {
                return GetConfig("ApiEndpoint");
            }
        }

        public static string ClientSecret
        {
            get => GetConfig("Secret");
        }

        public static string ClientId
        {
            get => GetConfig("Id");
        }

        private static string refreshToken;
        public static string RefreshToken
        {
            get
            {
                if (string.IsNullOrEmpty(refreshToken))
                {
                    refreshToken = GetConfig("RefreshToken");
                }

                return refreshToken;
            }

            private set
            {
                refreshToken = value;
            }
        }

        private static string accessToken;
        public static string AccessToken
        {
            get
            {
                if (string.IsNullOrEmpty(accessToken))
                {
                    accessToken = GetConfig("AccessToken");
                }

                return accessToken;
            }

            private set
            {
                accessToken = value;
            }
        }

        public static HttpClient HttpClient
        {
            get;
        } =  new HttpClient();

        private static void SaveTokens()
        {
            var path = Path.Combine(Paths.ExecutingDirectory, "patreonTokens.json");
            var config = new Dictionary<string, object>()
            {
                { "Patreon", new Dictionary<string, string>()
                    {
                        { "AccessToken", AccessToken },
                        { "RefreshToken", RefreshToken }
                    }
                }
            };

            File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));
        }

        private static string GetConfig(string key)
        {
            var set = Startup.Configuration.GetSection("Patreon");
            return set[key];
        }

        private static HttpRequestMessage CreateGetRequest(string url)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = url.StartsWith("https") ? new Uri(url) : new Uri(Endpoint + url),
                Method = HttpMethod.Get
            };

            request.Headers.Add("Authorization", $"Bearer {AccessToken}");
            return request;
        }

        private static async Task UpdateTokens()
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(Endpoint + $"token?grant_type=refresh_token&refresh_token={RefreshToken}&client_id={ClientId}&client_secret={ClientSecret}"),
                Method = HttpMethod.Post
            };

            var response = await HttpClient.SendAsync(request);
            var stringData = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(stringData);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                AccessToken = data["access_token"];
                RefreshToken = data["refresh_token"];
                SaveTokens();
            }
        }

        public static async Task<string> SendStringRequest(string url)
        {
            var request = CreateGetRequest(url);
            var response = await HttpClient.SendAsync(request);

            // Access token probably expired (once every month)
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await UpdateTokens();
                request = CreateGetRequest(url);
                response = await HttpClient.SendAsync(request);
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
