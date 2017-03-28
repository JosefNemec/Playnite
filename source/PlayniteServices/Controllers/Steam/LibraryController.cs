using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading;

namespace PlayniteServices.Controllers.Steam
{
    [Route("api/steam/library")]
    public class LibraryController : Controller
    {
        public class ResolveVanityResult
        {
            public class Response
            {
                public int success;
                public string steamid;
                public string message;
            }

            public Response response;
        }

        public class GetOwnedGamesResult
        {
            public class Game
            {
                public int appid;
                public string name;
                public int playtime_forever;
                public string img_icon_url;
                public string img_logo_url;
                public bool has_community_visible_stats;
            }

            public class Response
            {
                public int game_count;
                public List<Game> games;
            }

            public Response response;
        }

        private static string apiKey;
        private static HttpClient httpClient = new HttpClient();
        private static Dictionary<string, string> userIdCache = new Dictionary<string, string>();
        private static double requestDelay = 1500;
        private static DateTime lastRequest = DateTime.Now.AddMilliseconds(-requestDelay);
        private static object dateLock = new object();
        private static object userIdLock = new object();

        private void verifyApiKey()
        {
            if (!string.IsNullOrEmpty(apiKey))
            {
                return;
            }

            var key = Startup.Configuration.GetSection("Steam");
            if (key != null)
            {
                apiKey = key.Value;
            }
            
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("Missing Steam API Key on server.");
            }
        }

        // Steam API has limit one request per second, so we need to slow requests down
        // TODO: change this to something more sophisticated like proper queue
        private void waitRequest()
        {
            lock (dateLock)
            {
                var timeDiff = DateTime.Now - lastRequest;
                
                if (timeDiff.TotalMilliseconds > requestDelay)
                {
                    lastRequest = DateTime.Now;
                    return;
                }

                Thread.Sleep((int)requestDelay - (DateTime.Now - lastRequest).Milliseconds);
                lastRequest = DateTime.Now;
            }            
        }

        private string getUserId(string userName)
        {

            lock (userIdLock)
            {
                if (userIdCache.ContainsKey(userName))
                {
                    return userIdCache[userName];
                }

                waitRequest();
                
                var idUrl = string.Format(
                    @"http://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key={0}&vanityurl={1}",
                    apiKey, userName);
                var idStringResult = httpClient.GetStringAsync(idUrl).GetAwaiter().GetResult();
                var idResult = JsonConvert.DeserializeObject<ResolveVanityResult>(idStringResult);

                if (idResult.response.success == 42)
                {
                    throw new Exception("Failed to resolve Steam user id: " + idResult.response.message);
                }

                userIdCache.Add(userName, idResult.response.steamid);
                return idResult.response.steamid;
            }
        }
                
        [HttpGet("{userName}")]
        public async Task<ServicesResponse<List<GetOwnedGamesResult.Game>>> Get(string userName)
        {
            return await Task.Run(() =>
            {
                verifyApiKey();

                var libraryUrl = string.Format(
                    @"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={0}&include_appinfo=1&format=json&steamid={1}",
                    apiKey, getUserId(userName));
                waitRequest();

                var libraryStringResult = httpClient.GetStringAsync(libraryUrl).GetAwaiter().GetResult();
                var libraryResult = JsonConvert.DeserializeObject<GetOwnedGamesResult>(libraryStringResult);

                return new ServicesResponse<List<GetOwnedGamesResult.Game>>(libraryResult.response.games, string.Empty);
            });
        }
    }
}
