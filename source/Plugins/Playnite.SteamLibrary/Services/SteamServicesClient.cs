using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.Services;
using Playnite.SteamLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SteamLibrary.Services
{
    public class SteamServicesClient : BaseServicesClient
    {
        private readonly ILogger logger;

        private static HttpClient httpClient = new HttpClient()
        {
            Timeout = new TimeSpan(0, 0, 30)
        };

        public SteamServicesClient(string endpoint, ILogger logger) : base(endpoint)
        {
            this.logger = logger;
        }

        public void PostSteamAppInfoData(int appId, string data)
        {
            var content = new StringContent(data, Encoding.UTF8, "text/plain");
            httpClient.PostAsync(Endpoint + $"/api/steam/appinfo/{appId}", content).Wait();
        }

        public string GetSteamAppInfoData(int appId)
        {
            return ExecuteGetRequest<string>($"/api/steam/appinfo/{appId}");
        }

        public void PostSteamStoreData(int appId, string data)
        {
            var content = new StringContent(data, Encoding.UTF8, "text/plain");
            httpClient.PostAsync(Endpoint + $"/api/steam/store/{appId}", content).Wait();
        }

        public string GetSteamStoreData(int appId)
        {
            return ExecuteGetRequest<string>($"/api/steam/store/{appId}");
        }

        public List<GetOwnedGamesResult.Game> GetSteamLibrary(string userName)
        {
            return ExecuteGetRequest<List<GetOwnedGamesResult.Game>>("/api/steam/library/" + userName);
        }
    }
}
