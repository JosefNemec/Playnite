using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Playnite;
using Playnite.Common;
using Playnite.SDK;
using PlayniteServices.Models.IGDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.IGDB
{
    [Route("igdb/game")]
    public class GameController : IgdbItemController
    {
        private static ILogger logger = LogManager.GetLogger();
        private static readonly object CacheLock = new object();
        private const string endpointPath = "games";

        [HttpGet("{gameId}")]
        public async Task<ServicesResponse<Game>> Get(ulong gameId)
        {
            return new ServicesResponse<Game>(await GetItem<Game>(gameId, endpointPath, CacheLock));
        }
        
        // Only use for IGDB webhook.
        [HttpPost]
        public ActionResult Post([FromBody]Game game)
        {
            if (Request.Headers.TryGetValue("X-Secret", out var secret))
            {
                if (secret != IGDB.WebHookSecret)
                {
                    return BadRequest();
                }

                logger.Info($"Received game webhook from IGDB: {game.id}");
                var cachePath = Path.Combine(IGDB.CacheDirectory, endpointPath, game.id + ".json");
                lock (CacheLock)
                {
                    FileSystem.PrepareSaveFile(cachePath);                    
                    System.IO.File.WriteAllText(cachePath, Serialization.ToJson(game));
                }

                return Ok();
            }

            return BadRequest();
        }
    }

    [Route("igdb/game_parsed")]
    public class GameParsedController : Controller
    {
        [HttpGet("{gameId}")]
        public async Task<ServicesResponse<ParsedGame>> Get(ulong gameId)
        {
            var game = (await new GameController().Get(gameId)).Data;
            var parsedGame = new ParsedGame()
            {
                id = game.id,
                name = game.name,
                first_release_date = game.first_release_date,
                cover = game.cover?.url,
                websites = game.websites,
                summary = game.summary,
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
                    var dbDev = (await (new CompanyController()).Get(dev)).Data;
                    parsedGame.developers.Add(dbDev.name);
                }
            }

            if (game.game_modes?.Any() == true)
            {
                parsedGame.game_modes = new List<string>();
                foreach (var mode in game.game_modes)
                {
                    var dbMode = (await (new GameModeController()).Get(mode)).Data;
                    parsedGame.game_modes.Add(dbMode.name);
                }
            }

            if (game.genres?.Any() == true)
            {
                parsedGame.genres = new List<string>();
                foreach (var genre in game.genres)
                {
                    var dbGenre = (await (new GenreController()).Get(genre)).Data;
                    parsedGame.genres.Add(dbGenre.name);
                }
            }

            if (game.publishers?.Any() == true)
            {
                parsedGame.publishers = new List<string>();
                foreach (var pub in game.publishers)
                {
                    var dbDev = (await (new CompanyController()).Get(pub)).Data;
                    parsedGame.publishers.Add(dbDev.name);
                }
            }

            if (game.themes?.Any() == true)
            {
                parsedGame.themes = new List<string>();
                foreach (var theme in game.themes)
                {
                    var dbTheme = (await (new ThemeController()).Get(theme)).Data;
                    parsedGame.themes.Add(dbTheme.name);
                }
            }

            if (game.platforms?.Any() == true)
            {
                parsedGame.platforms = new List<string>();
                foreach (var platform in game.platforms)
                {
                    var dbPlatform = (await (new PlatformController()).Get(platform)).Data;
                    parsedGame.platforms.Add(dbPlatform.name);
                }
            }

            return new ServicesResponse<ParsedGame>(parsedGame);
        }
    }
}
