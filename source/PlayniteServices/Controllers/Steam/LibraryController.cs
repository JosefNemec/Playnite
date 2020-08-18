using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading;
using PlayniteServices.Models.Steam;
using LiteDB;
using PlayniteServices.Filters;
using PlayniteServices.Databases;

namespace PlayniteServices.Controllers.Steam
{
    [ServiceFilter(typeof(PlayniteVersionFilter))]
    [Route("steam/library")]
    public class LibraryController : Controller
    {
        private static HttpClient httpClient = new HttpClient();
        private static double requestDelay = 1500;
        private static DateTime lastRequest = DateTime.Now.AddMilliseconds(-requestDelay);
        private static object dateLock = new object();
        private static object userIdLock = new object();

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

                Thread.Sleep((int)requestDelay - (DateTime.Now - lastRequest).Milliseconds);
                lastRequest = DateTime.Now;
            }
        }

        private string GetUserId(string userName)
        {
            lock (userIdLock)
            {
                var cache = Database.SteamUserNamesCache.FindById(userName);
                if (cache != null)
                {
                    return cache.Id;
                }

                WaitRequest();

                var idUrl = string.Format(
                    @"http://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key={0}&vanityurl={1}",
                    Steam.ApiKey, userName);
                var idStringResult = httpClient.GetStringAsync(idUrl).GetAwaiter().GetResult();
                var idResult = JsonConvert.DeserializeObject<ResolveVanityResult>(idStringResult);

                if (idResult.response.success == 42)
                {
                    throw new Exception("Failed to resolve Steam user id: " + idResult.response.message);
                }

                Database.SteamUserNamesCache.Insert(new SteamNameCache()
                {
                    Id = idResult.response.steamid,
                    Name = userName,
                    UpdateDate = DateTime.Now
                });

                return idResult.response.steamid;
            }
        }

        [HttpGet("{userName}")]
        public async Task<ServicesResponse<List<GetOwnedGamesResult.Game>>> Get(string userName, [FromQuery]bool freeSub)
        {
            // ID can be passed directly
            var steamId = ulong.TryParse(userName, out var directId) ? directId.ToString() : GetUserId(userName);
            var libraryUrl = string.Format(
                @"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={0}&include_appinfo=1&format=json&steamid={1}&include_played_free_games=1",
                Steam.ApiKey, steamId);
            if (freeSub)
            {
                libraryUrl += "&include_free_sub=1";
            }

            WaitRequest();

            var libraryStringResult = await httpClient.GetStringAsync(libraryUrl);
            var libraryResult = JsonConvert.DeserializeObject<GetOwnedGamesResult>(libraryStringResult);

            return new ServicesResponse<List<GetOwnedGamesResult.Game>>(libraryResult.response.games);
        }
    }
}
