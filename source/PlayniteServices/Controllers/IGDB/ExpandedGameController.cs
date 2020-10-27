using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PlayniteServices.Controllers.IGDB.DataGetter;
using PlayniteServices.Filters;
using PlayniteServices.Models.IGDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.IGDB
{
    [ServiceFilter(typeof(PlayniteVersionFilter))]
    [Route("igdb/game_parsed_v2")]
    public class ExpandedGameController : Controller
    {
        private UpdatableAppSettings settings;
        private IgdbApi igdbApi;

        public ExpandedGameController(UpdatableAppSettings settings, IgdbApi igdbApi)
        {
            this.settings = settings;
            this.igdbApi = igdbApi;
        }

        [HttpGet("{gameId}")]
        public async Task<ServicesResponse<ExpandedGame>> Get(ulong gameId)
        {
            return new ServicesResponse<ExpandedGame>(await GetExpandedGame(gameId));
        }

        public async Task<ExpandedGame> GetExpandedGame(ulong gameId)
        {
            var game = await igdbApi.Games.Get(gameId);
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
                    parsedGame.alternative_names.Add(await igdbApi.AlternativeNames.Get(nameId));
                }
            }

            if (game.involved_companies?.Any() == true)
            {
                parsedGame.involved_companies = new List<ExpandedInvolvedCompany>();
                foreach (var companyId in game.involved_companies)
                {
                    parsedGame.involved_companies.Add(await igdbApi.InvolvedCompanies.GetItem(companyId));
                }
            }

            if (game.genres?.Any() == true)
            {
                parsedGame.genres = new List<Genre>();
                foreach (var genreId in game.genres)
                {
                    parsedGame.genres.Add(await igdbApi.Genres.Get(genreId));
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
                parsedGame.game_modes = new List<GameMode>();
                foreach (var modeId in game.game_modes)
                {
                    parsedGame.game_modes.Add(await igdbApi.GameModes.Get(modeId));
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
                parsedGame.cover = await igdbApi.Covers.Get(game.cover);
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

            if (game.age_ratings?.Any() == true)
            {
                parsedGame.age_ratings = new List<AgeRating>();
                foreach (var ageId in game.age_ratings)
                {
                    parsedGame.age_ratings.Add(await igdbApi.AgeRatings.Get(ageId));
                }
            }

            if (game.collection > 0)
            {
                parsedGame.collection = await igdbApi.Collections.Get(game.collection);
            }

            return parsedGame;
        }
    }
}
