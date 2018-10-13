using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.Services;
using SteamLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SteamLibrary.Services
{
    public class SteamServicesClient : BaseServicesClient
    {
        private readonly ILogger logger = LogManager.GetLogger();

        private static HttpClient httpClient = new HttpClient()
        {
            Timeout = new TimeSpan(0, 0, 30)
        };

        public SteamServicesClient(string endpoint) : base(endpoint)
        {
        }

        public void PostSteamAppInfoData(uint appId, string data)
        {
            var content = new StringContent(data, Encoding.UTF8, "text/plain");
            httpClient.PostAsync(Endpoint + $"/api/steam/appinfo/{appId}", content).Wait();
        }

        public string GetSteamAppInfoData(uint appId)
        {
            return ExecuteGetRequest<string>($"/api/steam/appinfo/{appId}");
        }

        public void PostSteamStoreData(uint appId, string data)
        {
            var content = new StringContent(data, Encoding.UTF8, "text/plain");
            httpClient.PostAsync(Endpoint + $"/api/steam/store/{appId}", content).Wait();
        }

        public string GetSteamStoreData(uint appId)
        {
            return ExecuteGetRequest<string>($"/api/steam/store/{appId}");
        }

        public List<GetOwnedGamesResult.Game> GetSteamLibrary(string userName)
        {
            return ExecuteGetRequest<List<GetOwnedGamesResult.Game>>("/api/steam/library/" + userName);
        }
    }
}
