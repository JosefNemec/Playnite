using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Playnite;
using Playnite.Common;
using Playnite.SDK;
using PlayniteServices.Controllers.IGDB.DataGetter;
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
    public class GameController : Controller
    {
        private static ILogger logger = LogManager.GetLogger();
        private static readonly object CacheLock = new object();
        private const string endpointPath = "games";

        private UpdatableAppSettings settings;
        private IgdbApi igdbApi;

        public GameController(UpdatableAppSettings settings, IgdbApi igdbApi)
        {
            this.settings = settings;
            this.igdbApi = igdbApi;
        }

        [ServiceFilter(typeof(PlayniteVersionFilter))]
        [HttpGet("{gameId}")]
        public async Task<ServicesResponse<Game>> Get(ulong gameId)
        {
            return await GetItem(gameId);
        }

        public async Task<ServicesResponse<Game>> GetItem(ulong gameId)
        {
            return new ServicesResponse<Game>(await igdbApi.GetItem<Game>(gameId, endpointPath, CacheLock));
        }

        // Only use for IGDB webhook.
        [HttpPost]
        public async Task<ActionResult> Post()
        {
            if (Request.Headers.TryGetValue("X-Secret", out var secret))
            {
                if (secret != settings.Settings.IGDB.WebHookSecret)
                {
                    logger.Error($"X-Secret doesn't match: {secret}");
                    return BadRequest();
                }

                try
                {
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
                    var cachePath = Path.Combine(settings.Settings.IGDB.CacheDirectory, endpointPath, game.id + ".json");
                    lock (CacheLock)
                    {
                        FileSystem.PrepareSaveFile(cachePath);
                        System.IO.File.WriteAllText(cachePath, jsonString, Encoding.UTF8);
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to process IGDB webhook.");
                }

                return Ok();
            }
            else
            {
                logger.Error("Missing X-Secret from IGDB webhook.");
                return BadRequest();
            }
        }
    }

    [ServiceFilter(typeof(PlayniteVersionFilter))]
    [Route("igdb/game_parsed")]
    public class GameParsedController : Controller
    {
        private UpdatableAppSettings settings;
        private IgdbApi igdbApi;

        public GameParsedController(UpdatableAppSettings settings, IgdbApi igdbApi)
        {
            this.settings = settings;
            this.igdbApi = igdbApi;
        }

        [HttpGet("{gameId}")]
        public async Task<ServicesResponse<ExpandedGameLegacy>> Get(ulong gameId)
        {
            return new ServicesResponse<ExpandedGameLegacy>(await GetExpandedGame(gameId));
        }

        public async Task<ExpandedGameLegacy> GetExpandedGame(ulong gameId)
        {
            var game = await igdbApi.Games.Get(gameId);
            if (game.id == 0)
            {
                new ExpandedGameLegacy();
            }

            var parsedGame = new ExpandedGameLegacy()
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
                    parsedGame.alternative_names.Add(await igdbApi.AlternativeNames.Get(nameId));
                }
            }

            if (game.involved_companies?.Any() == true)
            {
                parsedGame.involved_companies = new List<ExpandedInvolvedCompany>();
                foreach (var companyId in game.involved_companies)
                {
                    parsedGame.involved_companies.Add(await igdbApi.InvolvedCompanies.Get(companyId));
                }
            }

            if (game.genres?.Any() == true)
            {
                parsedGame.genres_v3 = new List<Genre>();
                foreach (var genreId in game.genres)
                {
                    parsedGame.genres_v3.Add(await igdbApi.Genres.Get(genreId));
                }
            }

            if (game.websites?.Any() == true)
            {
                parsedGame.websites = new List<Website>();
                foreach (var websiteId in game.websites)
                {
                    parsedGame.websites.Add(await igdbApi.Websites.Get(websiteId));
                }
            }

            if (game.game_modes?.Any() == true)
            {
                parsedGame.game_modes_v3 = new List<GameMode>();
                foreach (var modeId in game.game_modes)
                {
                    parsedGame.game_modes_v3.Add(await igdbApi.GameModes.Get(modeId));
                }
            }

            if (game.player_perspectives?.Any() == true)
            {
                parsedGame.player_perspectives = new List<PlayerPerspective>();
                foreach (var persId in game.player_perspectives)
                {
                    parsedGame.player_perspectives.Add(await igdbApi.PlayerPerspectives.Get(persId));
                }
            }

            if (game.cover > 0)
            {
                parsedGame.cover_v3 = await igdbApi.Covers.Get(game.cover);
            }

            if (game.artworks?.Any() == true)
            {
                parsedGame.artworks = new List<GameImage>();
                foreach (var artworkId in game.artworks)
                {
                    parsedGame.artworks.Add(await igdbApi.Artworks.Get(artworkId));
                }
            }

            if (game.screenshots?.Any() == true)
            {
                parsedGame.screenshots = new List<GameImage>();
                foreach (var screenshotId in game.screenshots)
                {
                    parsedGame.screenshots.Add(await igdbApi.Screenshots.Get(screenshotId));
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
