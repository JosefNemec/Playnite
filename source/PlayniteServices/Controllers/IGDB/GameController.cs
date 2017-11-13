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
        public async Task<ServicesResponse<Game>> Get(UInt64 gameId)
        {            
            var url = string.Format(IGDB.UrlBase + @"games/{0}?fields=name%2Csummary%2Cdevelopers%2Cpublishers%2Cgenres%2Cfirst_release_date%2Ccover%2Cthemes%2Cgame_modes%2Cwebsites&limit=40&offset=0&search={0}", gameId);
            var libraryStringResult = await IGDB.HttpClient.GetStringAsync(url);
            var game = JsonConvert.DeserializeObject<List<Game>>(libraryStringResult);
            return new ServicesResponse<Game>(game[0], string.Empty);
        }
    }

    [Route("api/igdb/game_parsed")]
    public class GameParsedController : Controller
    {
        [HttpGet("{gameId}")]
        public async Task<ServicesResponse<ParsedGame>> Get(UInt64 gameId)
        {
            var url = string.Format(IGDB.UrlBase + @"games/{0}?fields=name%2Csummary%2Cdevelopers%2Cpublishers%2Cgenres%2Cfirst_release_date%2Ccover%2Cthemes%2Cgame_modes%2Cwebsites&limit=40&offset=0&search={0}", gameId);
            var libraryStringResult = await IGDB.HttpClient.GetStringAsync(url);
            var game = JsonConvert.DeserializeObject<List<Game>>(libraryStringResult)[0];
            var parsedGame = new ParsedGame()
            {
                id = game.id,
                name = game.name,
                first_release_date = game.first_release_date,
                cover = game.cover?.url,
                websites = game.websites,
                summary = game.summary
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
                foreach (var genre in game.themes)
                {
                    var dbTheme = (await (new ThemeController()).Get(genre)).Data;
                    parsedGame.themes.Add(dbTheme.name);
                }
            }

            return new ServicesResponse<ParsedGame>(parsedGame, string.Empty);
        }
    }
}
