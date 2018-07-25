using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlayniteServices.Models.IGDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.IGDB
{
    [Route("api/igdb/game")]
    public class GameController : Controller
    {
        [HttpGet("{gameId}")]
        public async Task<ServicesResponse<Game>> Get(ulong gameId, [FromQuery]string apiKey)
        {
            var url = string.Format(@"games/{0}?fields=*&limit=40&offset=0&search={0}", gameId);
            var libraryStringResult = await IGDB.SendStringRequest(url, apiKey);
            var game = JsonConvert.DeserializeObject<List<Game>>(libraryStringResult);
            return new ServicesResponse<Game>(game[0], string.Empty);
        }
    }

    [Route("api/igdb/game_parsed")]
    public class GameParsedController : Controller
    {
        [HttpGet("{gameId}")]
        public async Task<ServicesResponse<ParsedGame>> Get(ulong gameId, [FromQuery]string apiKey)
        {
            var cacheCollection = Program.DatabaseCache.GetCollection<ParsedGame>("IGBDParsedGameCache");
            var cache = cacheCollection.FindById(gameId);
            if (cache != null)
            {
                var dateDiff = DateTime.Now - cache.creation_time;
                if (dateDiff.TotalHours <= IGDB.CacheTimeout)
                {
                    return new ServicesResponse<ParsedGame>(cache, string.Empty);
                }
            }

            var url = string.Format(@"games/{0}?fields=*&limit=40&offset=0&search={0}", gameId);
            var libraryStringResult = await IGDB.SendStringRequest(url, apiKey);
            var game = JsonConvert.DeserializeObject<List<Game>>(libraryStringResult)[0];
            var parsedGame = new ParsedGame()
            {
                id = game.id,
                name = game.name,
                first_release_date = game.first_release_date,
                cover = game.cover?.url,
                websites = game.websites,
                summary = game.summary,
                creation_time = DateTime.Now,
                rating = game.rating,
                aggregated_rating = game.aggregated_rating,
                total_rating = game.total_rating,
                alternative_names = game.alternative_names,
                external = game.external,
                screenshots = game.screenshots,
                videos = game.videos,
                artworks = game.artworks,
                release_dates = game.release_dates                
            };
        
            if (game.developers?.Any() == true)
            {
                parsedGame.developers = new List<string>();
                foreach (var dev in game.developers)
                {
                    var dbDev = (await (new CompanyController()).Get(dev, apiKey)).Data;
                    parsedGame.developers.Add(dbDev.name);
                }
            }

            if (game.game_modes?.Any() == true)
            {
                parsedGame.game_modes = new List<string>();
                foreach (var mode in game.game_modes)
                {
                    var dbMode = (await (new GameModeController()).Get(mode, apiKey)).Data;
                    parsedGame.game_modes.Add(dbMode.name);
                }
            }

            if (game.genres?.Any() == true)
            {
                parsedGame.genres = new List<string>();
                foreach (var genre in game.genres)
                {
                    var dbGenre = (await (new GenreController()).Get(genre, apiKey)).Data;
                    parsedGame.genres.Add(dbGenre.name);
                }
            }

            if (game.publishers?.Any() == true)
            {
                parsedGame.publishers = new List<string>();
                foreach (var pub in game.publishers)
                {
                    var dbDev = (await (new CompanyController()).Get(pub, apiKey)).Data;
                    parsedGame.publishers.Add(dbDev.name);
                }
            }

            if (game.themes?.Any() == true)
            {
                parsedGame.themes = new List<string>();
                foreach (var theme in game.themes)
                {
                    var dbTheme = (await (new ThemeController()).Get(theme, apiKey)).Data;
                    parsedGame.themes.Add(dbTheme.name);
                }
            }

            if (game.platforms?.Any() == true)
            {
                parsedGame.platforms = new List<string>();
                foreach (var platform in game.platforms)
                {
                    var dbPlatform = (await (new PlatformController()).Get(platform, apiKey)).Data;
                    parsedGame.platforms.Add(dbPlatform.name);
                }
            }

            cacheCollection.Upsert(parsedGame);
            return new ServicesResponse<ParsedGame>(parsedGame, string.Empty);
        }
    }
}
