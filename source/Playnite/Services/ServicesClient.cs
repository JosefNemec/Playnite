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
            var url = Endpoint + subUrl;
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

        public PlayniteServices.Models.IGDB.Company GetIGDBCompany(UInt64 id)
        {
            return ExecuteRequest<PlayniteServices.Models.IGDB.Company>("/api/igdb/company/" + id);
        }

        public PlayniteServices.Models.IGDB.Genre GetIGDBGenre(UInt64 id)
        {
            return ExecuteRequest<PlayniteServices.Models.IGDB.Genre>("/api/igdb/genre/" + id);
        }
    }
}
