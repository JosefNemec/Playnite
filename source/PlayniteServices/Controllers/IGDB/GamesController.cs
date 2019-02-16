using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlayniteServices.Models.IGDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LiteDB;
using System.IO;
using Playnite;

namespace PlayniteServices.Controllers.IGDB
{
    [Route("igdb/games")]
    public class GamesController : Controller
    {
        private static readonly object CacheLock = new object();

        private const string cacheDir = "game_search";

        [HttpGet("{gameName}")]
        public async Task<ServicesResponse<List<Game>>> Get(string gameName)
        {
            List<Game> searchResult = null;
            gameName = gameName.ToLower();
            var cachePath = Path.Combine(IGDB.CacheDirectory, cacheDir, Playnite.Common.System.Paths.GetSafeFilename(gameName) + ".json");
            lock (CacheLock)
            {
                if (System.IO.File.Exists(cachePath))
                {
                    var fileInfo = new FileInfo(cachePath);
                    if ((fileInfo.LastWriteTime - DateTime.Now).TotalHours <= IGDB.SearchCacheTimeout)
                    {
                        searchResult = JsonConvert.DeserializeObject<List<Game>>(System.IO.File.ReadAllText(cachePath));                        
                    }
                }
            }

            if (searchResult == null)
            {
                var url = string.Format(@"games/?fields=id&limit=40&offset=0&search={0}", gameName);
                var libraryStringResult = await IGDB.SendStringRequest(url);
                searchResult = JsonConvert.DeserializeObject<List<Game>>(libraryStringResult);
                lock (CacheLock)
                {
                    FileSystem.PrepareSaveFile(cachePath);
                    System.IO.File.WriteAllText(cachePath, libraryStringResult);
                }
            }

            using (var gameController = new GameController())
            {
                for (int i = 0; i < searchResult.Count; i++)
                {
                    searchResult[i] = (await gameController.Get(searchResult[i].id)).Data;
                }
            }

            return new ServicesResponse<List<Game>>(searchResult);
        }
    }
}
