using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Playnite;
using Playnite.Common;
using Playnite.SDK;
using PlayniteServices.Filters;
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

        private AppSettings appSettings;

        public GameController(IOptions<AppSettings> settings)
        {
            appSettings = settings.Value;
        }

        [ServiceFilter(typeof(PlayniteVersionFilter))]
        [HttpGet("{gameId}")]
        public async Task<ServicesResponse<Game>> Get(ulong gameId)
        {
            return await GetItem(gameId);
        }

        public static async Task<ServicesResponse<Game>> GetItem(ulong gameId)
        {
            return new ServicesResponse<Game>(await GetItem<Game>(gameId, endpointPath, CacheLock));
        }

        // Only use for IGDB webhook.
        [HttpPost]
        public async Task<ActionResult> Post()
        {
            if (Request.Headers.TryGetValue("X-Secret", out var secret))
            {
                if (secret != IGDB.WebHookSecret)
                {
                    return BadRequest();
                }

                Game game = null;
                string jsonString = null;
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    jsonString = await reader.ReadToEndAsync();
                    if (!string.IsNullOrEmpty(jsonString))
                    {
                        game = Serialization.FromJson<Game>(jsonString);
                    }
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
                    System.IO.File.WriteAllText(cachePath, jsonString, Encoding.UTF8);
                }

                return Ok();
            }

            return BadRequest();
        }
    }

    [ServiceFilter(typeof(PlayniteVersionFilter))]
    [Route("igdb/game_parsed")]
    public class GameParsedController : Controller
    {
        private IOptions<AppSettings> appSettings;

        public GameParsedController(IOptions<AppSettings> settings)
        {
            appSettings = settings;
        }

        [HttpGet("{gameId}")]
        public async Task<ServicesResponse<ExpandedGame>> Get(ulong gameId)
        {
            return new ServicesResponse<ExpandedGame>(await GetExpandedGame(gameId));
        }

        public async static Task<ExpandedGame> GetExpandedGame(ulong gameId)
        {
            var game = (await GameController.GetItem(gameId)).Data;
            if (game.id == 0)
            {
                new ExpandedGame();
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
                first_release_date = game.first_release_date * 1000,
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

            if (game.player_perspectives?.Any() == true)
            {
                parsedGame.player_perspectives = new List<PlayerPerspective>();
                foreach (var persId in game.player_perspectives)
                {
                    parsedGame.player_perspectives.Add((await PlayerPerspectiveController.GetItem(persId)).Data);
                }
            }

            if (game.cover > 0)
            {
                parsedGame.cover_v3 = (await CoverController.GetItem(game.cover)).Data;
            }

            if (game.artworks?.Any() == true)
            {
                parsedGame.artworks = new List<GameImage>();
                foreach (var artworkId in game.artworks)
                {
                    parsedGame.artworks.Add((await ArtworkController.GetItem(artworkId)).Data);
                }
            }

            if (game.screenshots?.Any() == true)
            {
                parsedGame.screenshots = new List<GameImage>();
                foreach (var screenshotId in game.screenshots)
                {
                    parsedGame.screenshots.Add((await ScreenshotController.GetItem(screenshotId)).Data);
                }
            }

            // fallback properties for 4.x
            parsedGame.cover = parsedGame.cover_v3?.url;
            parsedGame.publishers = parsedGame.involved_companies?.Where(a => a.publisher == true).Select(a => a.company.name).ToList();
            parsedGame.developers = parsedGame.involved_companies?.Where(a => a.developer == true).Select(a => a.company.name).ToList();
            parsedGame.genres = parsedGame.genres_v3?.Select(a => a.name).ToList();
            parsedGame.game_modes = parsedGame.game_modes_v3?.Select(a => a.name).ToList();
            return parsedGame;
        }
    }
}
