using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlayniteServices.Models.IGDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Playnite;
using System.Web;
using Playnite.SDK;
using Microsoft.Extensions.Options;
using PlayniteServices.Filters;
using System.Text.RegularExpressions;

namespace PlayniteServices.Controllers.IGDB
{
    [ServiceFilter(typeof(PlayniteVersionFilter))]
    [Route("igdb/games")]
    public class GamesController : Controller
    {
        private static readonly JsonSerializer jsonSerializer = new JsonSerializer();
        private static readonly object CacheLock = new object();
        private const string cacheDir = "game_search";
        private static ILogger logger = LogManager.GetLogger();
        private static readonly char[] bracketsMatchList = new char[] { '[', ']', '(', ')', '{', '}' };

        private AppSettings appSettings;

        public GamesController(IOptions<AppSettings> settings)
        {
            appSettings = settings.Value;
        }

        [HttpGet("{gameName}")]
        public async Task<ServicesResponse<List<ExpandedGameLegacy>>> Get(string gameName)
        {
            var result = await GetSearchResults(gameName, appSettings.IGDB.AlternativeSearch);
            if (result.Count == 0 && appSettings.IGDB.AlternativeSearch)
            {
                result = await GetSearchResults(gameName, false);
            }

            return new ServicesResponse<List<ExpandedGameLegacy>>(result);
        }

        public async static Task<List<ExpandedGameLegacy>> GetSearchResults(string searchString, bool alternativeSearch)
        {
            List<Game> searchResult = null;
            var modifiedSearchString = ModelsUtils.GetIgdbSearchString(searchString);
            var cachePath = Path.Combine(
                IGDB.CacheDirectory,
                cacheDir,
                (alternativeSearch ? "alt_" : "srch_") + Playnite.Common.Paths.GetSafeFilename(modifiedSearchString) + ".json");
            lock (CacheLock)
            {
                if (System.IO.File.Exists(cachePath))
                {
                    var fileInfo = new FileInfo(cachePath);
                    fileInfo.Refresh();
                    if ((DateTime.Now - fileInfo.LastWriteTime).TotalHours <= IGDB.SearchCacheTimeout)
                    {
                        using (var fs = new FileStream(cachePath, FileMode.Open, FileAccess.Read))
                        using (var sr = new StreamReader(fs))
                        using (var reader = new JsonTextReader(sr))
                        {
                            searchResult = jsonSerializer.Deserialize<List<Game>>(reader);
                        }
                    }
                }
            }

            if (searchResult == null)
            {
                var matchString = HttpUtility.UrlDecode(modifiedSearchString);
                if (matchString.ContainsAny(bracketsMatchList))
                {
                    return new List<ExpandedGameLegacy>();
                }

                var whereQuery = $"where (name ~ *\"{matchString}\"*) | (alternative_names.name ~ *\"{matchString}\"*); fields id; limit 50;";
                var searchQuery = $"search \"{matchString}\"; fields id; limit 50;";
                var query = alternativeSearch ? whereQuery : searchQuery;
                var searchStringResult = await IGDB.SendStringRequest("games", query);
                searchResult = JsonConvert.DeserializeObject<List<Game>>(searchStringResult);

                lock (CacheLock)
                {
                    Playnite.Common.FileSystem.PrepareSaveFile(cachePath);
                    System.IO.File.WriteAllText(cachePath, searchStringResult);
                }
            }

            var finalResult = new List<ExpandedGameLegacy>();
            for (int i = 0; i < searchResult.Count; i++)
            {
                Game result = null;
                try
                {
                    result = (await GameController.GetItem(searchResult[i].id)).Data;
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to get game {searchResult[i].id}");
                    continue;
                }

                if (result.id == 0)
                {
                    continue;
                }

                var xpanded = new ExpandedGameLegacy()
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

            return finalResult;
        }
    }
}
