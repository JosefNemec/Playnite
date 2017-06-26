using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading;
using PlayniteServices.Models.Steam;

namespace PlayniteServices.Controllers.Steam
{
    [Route("api/steam/library")]
    public class LibraryController : Controller
    {
        private static string apiKey;
        private static HttpClient httpClient = new HttpClient();
        private static double requestDelay = 1500;
        private static DateTime lastRequest = DateTime.Now.AddMilliseconds(-requestDelay);
        private static object dateLock = new object();
        private static object userIdLock = new object();

        private void VerifyApiKey()
        {
            if (!string.IsNullOrEmpty(apiKey))
            {
                return;
            }

            var key = Startup.Configuration.GetSection("SteamKey");
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
        private void WaitRequest()
        {
            lock (dateLock)
            {
                var timeDiff = DateTime.Now - lastRequest;
                
                if (timeDiff.TotalMilliseconds > requestDelay)
                {
                    lastRequest = DateTime.Now;
                    return;
                }
                Console.WriteLine("waiting for -----" + ((int)requestDelay - (DateTime.Now - lastRequest).Milliseconds).ToString());
                Thread.Sleep((int)requestDelay - (DateTime.Now - lastRequest).Milliseconds);
                lastRequest = DateTime.Now;
            }            
        }

        private string GetUserId(string userName)
        {
            lock (userIdLock)
            {
                var cacheCollection = Program.DatabaseCache.GetCollection<SteamNameCache>("SteamUserNamesCache");
                var cache = cacheCollection.FindById(userName);
                if (cache != null)
                {
                    return cache.Id;
                }

                WaitRequest();
                
                var idUrl = string.Format(
                    @"http://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key={0}&vanityurl={1}",
                    apiKey, userName);
                var idStringResult = httpClient.GetStringAsync(idUrl).GetAwaiter().GetResult();
                var idResult = JsonConvert.DeserializeObject<ResolveVanityResult>(idStringResult);

                if (idResult.response.success == 42)
                {
                    throw new Exception("Failed to resolve Steam user id: " + idResult.response.message);
                }

                cacheCollection.Insert(new SteamNameCache()
                {
                    Id = idResult.response.steamid,
                    Name = userName,
                    UpdateDate = DateTime.Now
                });

                return idResult.response.steamid;
            }
        }
                
        [HttpGet("{userName}")]
        public async Task<ServicesResponse<List<GetOwnedGamesResult.Game>>> Get(string userName)
        {            
            VerifyApiKey();

            var libraryUrl = string.Format(
                @"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={0}&include_appinfo=1&format=json&steamid={1}",
                apiKey, GetUserId(userName));
            WaitRequest();

            var libraryStringResult = await httpClient.GetStringAsync(libraryUrl);
            var libraryResult = JsonConvert.DeserializeObject<GetOwnedGamesResult>(libraryStringResult);

            return new ServicesResponse<List<GetOwnedGamesResult.Game>>(libraryResult.response.games, string.Empty);
        }
    }
}
