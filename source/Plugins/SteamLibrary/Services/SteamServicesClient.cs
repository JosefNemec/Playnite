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

        public SteamServicesClient(string endpoint) : base(endpoint)
        {
        }

        public void PostSteamAppInfoData(uint appId, string data)
        {
            var content = new StringContent(data, Encoding.UTF8, "text/plain");
            HttpClient.PostAsync(Endpoint + $"/steam/appinfo/{appId}", content).Wait();
        }

        public string GetSteamAppInfoData(uint appId)
        {
            return ExecuteGetRequest<string>($"/steam/appinfo/{appId}");
        }

        public void PostSteamStoreData(uint appId, string data)
        {
            var content = new StringContent(data, Encoding.UTF8, "text/plain");
            HttpClient.PostAsync(Endpoint + $"/steam/store/{appId}", content).Wait();
        }

        public string GetSteamStoreData(uint appId)
        {
            return ExecuteGetRequest<string>($"/steam/store/{appId}");
        }

        public List<GetOwnedGamesResult.Game> GetSteamLibrary(string userName)
        {
            return ExecuteGetRequest<List<GetOwnedGamesResult.Game>>("/steam/library/" + userName);
        }
    }
}
