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

        public List<Playnite.Providers.Steam.GetOwnedGamesResult.Game> GetSteamLibrary(string userName)
        {
            var url = Endpoint + "/api/steam/library/" + userName;
            var httpClient = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 30)
            };
            var strResult = httpClient.GetStringAsync(url).GetAwaiter().GetResult();
            var result = JsonConvert.DeserializeObject<ServicesResponse<List<Providers.Steam.GetOwnedGamesResult.Game>>>(strResult);

            if (!string.IsNullOrEmpty(result.Error))
            {
                throw new Exception(result.Error);
            }

            return result.Data;
        }
    }
}
