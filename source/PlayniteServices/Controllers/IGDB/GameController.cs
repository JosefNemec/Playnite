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

                if (game == null)
                {
                    logger.Error("Failed IGDB content serialization.");
                    return Ok();
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
        public async Task<ServicesResponse<ExpandedGame>> Get(ulong gameId)
        {
            var game = (await new GameController().Get(gameId)).Data;
            if (game.id == 0)
            {
                new ServicesResponse<ExpandedGame>(new ExpandedGame());
            }

            var parsedGame = new ExpandedGame()
            {
                id = game.id,
                name = game.name,
                slug = game.slug,
                url = game.url,
                summary = game.summary,
                storyline = game.storyline,
                popularity = game.popularity,
                version_title = game.version_title,
                category = game.category,
                first_release_date = game.first_release_date,
                rating = game.rating,
                aggregated_rating = game.aggregated_rating,
                total_rating = game.total_rating
            };

            if (game.alternative_names?.Any() == true)
            {
                parsedGame.alternative_names = new List<AlternativeName>();
                foreach (var nameId in game.alternative_names)
                {
                    parsedGame.alternative_names.Add((await AlternativeNameController.GetItem(nameId)).Data);
                }
            }

            if (game.involved_companies?.Any() == true)
            {
                parsedGame.involved_companies = new List<ExpandedInvolvedCompany>();
                foreach (var companyId in game.involved_companies)
                {
                    parsedGame.involved_companies.Add((await InvolvedCompanyController.GetItem(companyId)).Data);
                }
            }

            if (game.genres?.Any() == true)
            {
                parsedGame.genres_v3 = new List<Genre>();
                foreach (var genreId in game.genres)
                {
                    parsedGame.genres_v3.Add((await GenreController.GetItem(genreId)).Data);
                }
            }

            if (game.websites?.Any() == true)
            {
                parsedGame.websites = new List<Website>();
                foreach (var websiteId in game.websites)
                {
                    parsedGame.websites.Add((await WebsiteController.GetItem(websiteId)).Data);
                }
            }

            if (game.game_modes?.Any() == true)
            {
                parsedGame.game_modes_v3 = new List<GameMode>();
                foreach (var modeId in game.game_modes)
                {
                    parsedGame.game_modes_v3.Add((await GameModeController.GetItem(modeId)).Data);
                }
            }

            if (game.cover > 0)
            {
                parsedGame.cover_v3 = (await CoverController.GetItem(game.cover)).Data;
            }

            // fallback properties for 4.x
            parsedGame.cover = parsedGame.cover_v3?.url;
            parsedGame.publishers = parsedGame.involved_companies?.Where(a => a.publisher == true).Select(a => a.company.name).ToList();
            parsedGame.developers = parsedGame.involved_companies?.Where(a => a.developer == true).Select(a => a.company.name).ToList();
            parsedGame.genres = parsedGame.genres_v3?.Select(a => a.name).ToList();
            parsedGame.game_modes = parsedGame.game_modes_v3?.Select(a => a.name).ToList();
            parsedGame.first_release_date = parsedGame.first_release_date * 1000;

            return new ServicesResponse<ExpandedGame>(parsedGame);
        }
    }
}
