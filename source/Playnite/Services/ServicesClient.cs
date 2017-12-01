using Microsoft.Win32;
using Newtonsoft.Json;
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

        private T ExecuteRequest<T>(string subUrl)
        {
            var url = Uri.EscapeUriString(Endpoint + subUrl);
            var strResult = httpClient.GetStringAsync(url).GetAwaiter().GetResult();
            var result = JsonConvert.DeserializeObject<ServicesResponse<T>>(strResult);

            if (!string.IsNullOrEmpty(result.Error))
            {
                throw new Exception(result.Error);
            }

            return result.Data;
        }

        public List<Playnite.Providers.Steam.GetOwnedGamesResult.Game> GetSteamLibrary(string userName)
        {
            return ExecuteRequest<List<Playnite.Providers.Steam.GetOwnedGamesResult.Game>>("/api/steam/library/" + userName);
        }

        public List<PlayniteServices.Models.IGDB.Game> GetIGDBGames(string searchName)
        {
            return ExecuteRequest<List<PlayniteServices.Models.IGDB.Game>>("/api/igdb/games/" + searchName);
        }

        public PlayniteServices.Models.IGDB.Game GetIGDBGame(UInt64 id)
        {
            return ExecuteRequest<PlayniteServices.Models.IGDB.Game>("/api/igdb/game/" + id);
        }

        public PlayniteServices.Models.IGDB.ParsedGame GetIGDBGameParsed(UInt64 id)
        {
            return ExecuteRequest<PlayniteServices.Models.IGDB.ParsedGame>("/api/igdb/game_parsed/" + id);
        }

        public PlayniteServices.Models.IGDB.Company GetIGDBCompany(UInt64 id)
        {
            return ExecuteRequest<PlayniteServices.Models.IGDB.Company>("/api/igdb/company/" + id);
        }

        public PlayniteServices.Models.IGDB.Genre GetIGDBGenre(UInt64 id)
        {
            return ExecuteRequest<PlayniteServices.Models.IGDB.Genre>("/api/igdb/genre/" + id);
        }

        public void PostUserUsage()
        {
            var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            var winId = root.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", false).GetValue("ProductId").ToString();
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
