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
using System.Web;
using Playnite.SDK;

namespace PlayniteServices.Controllers.IGDB
{
    [Route("igdb/games")]
    public class GamesController : Controller
    {
        private static readonly object CacheLock = new object();
        private const string cacheDir = "game_search";
        private static ILogger logger = LogManager.GetLogger();

        [HttpGet("{gameName}")]
        public async Task<ServicesResponse<List<ExpandedGame>>> Get(string gameName)
        {
            List<Game> searchResult = null;
            gameName = gameName.ToLower();
            var cachePath = Path.Combine(IGDB.CacheDirectory, cacheDir, Playnite.Common.Paths.GetSafeFilename(gameName) + ".json");
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
                var libraryStringResult = await IGDB.SendStringRequest("games", $"search \"{HttpUtility.UrlDecode(gameName)}\"; fields id; limit 40;");
                searchResult = JsonConvert.DeserializeObject<List<Game>>(libraryStringResult);
                lock (CacheLock)
                {
                    Playnite.Common.FileSystem.PrepareSaveFile(cachePath);
                    System.IO.File.WriteAllText(cachePath, libraryStringResult);
                }
            }

            var finalResult = new List<ExpandedGame>();
            using (var gameController = new GameController())
            {
                for (int i = 0; i < searchResult.Count; i++)
                {
                    Game result = null;
                    try
                    {
                        result = (await gameController.Get(searchResult[i].id)).Data;
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, $"Failed to get game {searchResult[i].id}");
                        continue;
                    }

                    var xpanded = new ExpandedGame()
                    {
                        id = result.id,
                        name = result.name,
                        first_release_date = result.first_release_date * 1000
                    };

                    if (result.alternative_names?.Any() == true)
                    {
                        xpanded.alternative_names = new List<AlternativeName>();
                        foreach (var nameId in result.alternative_names)
                        {
                            xpanded.alternative_names.Add((await AlternativeNameController.GetItem(nameId)).Data);
                        }
                    }

                    finalResult.Add(xpanded);
                }
            }

            return new ServicesResponse<List<ExpandedGame>>(finalResult);
        }
    }
}
