using Microsoft.Win32;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Services
{
    public class ServicesClient
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private HttpClient httpClient = new HttpClient()
        {
            Timeout = new TimeSpan(0, 0, 30)
        };

        public string Endpoint
        {
            get;
        }

        public ServicesClient()
        {
            Endpoint = ConfigurationManager.AppSettings["ServicesUrl"].TrimEnd('/');
        }

        public ServicesClient(string endpoint)
        {
            Endpoint = endpoint.TrimEnd('/');
        }

        private T ExecuteGetRequest<T>(string subUrl)
        {
            var url = Uri.EscapeUriString(Endpoint + subUrl);
            var strResult = httpClient.GetStringAsync(url).GetAwaiter().GetResult();
            var result = JsonConvert.DeserializeObject<ServicesResponse<T>>(strResult);

            if (!string.IsNullOrEmpty(result.Error))
            {
                logger.Error("Service request error by proxy: " + result.Error);
                throw new Exception(result.Error);
            }

            return result.Data;
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

        public List<Playnite.Providers.Steam.GetOwnedGamesResult.Game> GetSteamLibrary(string userName)
        {
            return ExecuteGetRequest<List<Playnite.Providers.Steam.GetOwnedGamesResult.Game>>("/api/steam/library/" + userName);
        }

        public List<PlayniteServices.Models.IGDB.Game> GetIGDBGames(string searchName, string apiKey = null)
        {
            var encoded = Uri.EscapeDataString(searchName);
            var url = string.IsNullOrEmpty(apiKey) ? $"/api/igdb/games/{encoded}" : $"/api/igdb/games/{encoded}?apikey={apiKey}";
            return ExecuteGetRequest<List<PlayniteServices.Models.IGDB.Game>>(url);
        }

        public PlayniteServices.Models.IGDB.Game GetIGDBGame(UInt64 id, string apiKey = null)
        {
            var url = string.IsNullOrEmpty(apiKey) ? $"/api/igdb/game/{id}" : $"/api/igdb/game/{id}?apikey={apiKey}";
            return ExecuteGetRequest<PlayniteServices.Models.IGDB.Game>(url);
        }

        public PlayniteServices.Models.IGDB.ParsedGame GetIGDBGameParsed(UInt64 id, string apiKey = null)
        {
            var url = string.IsNullOrEmpty(apiKey) ? $"/api/igdb/game_parsed/{id}" : $"/api/igdb/game_parsed/{id}?apikey={apiKey}";
            return ExecuteGetRequest<PlayniteServices.Models.IGDB.ParsedGame>(url);
        }

        public PlayniteServices.Models.IGDB.Company GetIGDBCompany(UInt64 id, string apiKey = null)
        {
            var url = string.IsNullOrEmpty(apiKey) ? $"/api/igdb/company/{id}" : $"/api/igdb/company/{id}?apikey={apiKey}";
            return ExecuteGetRequest<PlayniteServices.Models.IGDB.Company>(url);
        }

        public PlayniteServices.Models.IGDB.Genre GetIGDBGenre(UInt64 id, string apiKey = null)
        {
            var url = string.IsNullOrEmpty(apiKey) ? $"/api/igdb/genre/{id}" : $"/api/igdb/genre/{id}?apikey={apiKey}";
            return ExecuteGetRequest<PlayniteServices.Models.IGDB.Genre>(url);
        }

        public List<string> GetPatrons()
        {
            return ExecuteGetRequest<List<string>>("/api/patreon/patrons");
        }

        public void PostUserUsage(string instId)
        {
            var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            var winId = root.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", false).GetValue("ProductId").ToString().GetSHA256Hash();
            var user = new PlayniteServices.Models.Playnite.User()
            {
                Id = winId,
                WinVersion = Environment.OSVersion.VersionString,
                PlayniteVersion = Update.GetCurrentVersion().ToString()
            };

            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            httpClient.PostAsync(Endpoint + "/api/playnite/users", content).Wait();
        }
    }
}
