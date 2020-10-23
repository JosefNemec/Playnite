using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Playnite.Common;
using Playnite.SDK;
using PlayniteServices.Databases;
using PlayniteServices.Filters;
using PlayniteServices.Models.IGDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SdkModels = Playnite.SDK.Models;

namespace PlayniteServices.Controllers.IGDB
{
    [ServiceFilter(typeof(PlayniteVersionFilter))]
    [Route("igdb")]
    public class MetadataController : Controller
    {
        private readonly static ILogger logger = LogManager.GetLogger();
        private UpdatableAppSettings settings;
        private IgdbApi igdbApi;
        private static readonly Regex separatorRegex = new Regex(@"\s*(:|-)\s*", RegexOptions.Compiled);
        private static readonly Regex noIntroArticleRegEx = new Regex(@",\s*(the|a|an|der|das|die)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly char[] bracketsMatchList = new char[] { '[', ']', '(', ')', '{', '}' };
        private static readonly char[] whereQueryBlacklist = new char[2] { ':', '-' };
        private GamesController gamesController;
        private ExpandedGameController expandedController;
        private GameParsedController parsedController;

        public MetadataController(UpdatableAppSettings settings, IgdbApi igdbApi)
        {
            this.settings = settings;
            this.igdbApi = igdbApi;
            gamesController = new GamesController(settings, igdbApi);
            expandedController = new ExpandedGameController(settings, igdbApi);
            parsedController = new GameParsedController(settings, igdbApi);
        }

        [HttpPost("metadata_v2")]
        public async Task<ServicesResponse<ExpandedGame>> PostMetadataV2([FromBody]SdkModels.Game game)
        {
            return await GetMetadata(game, expandedController.GetExpandedGame);
        }

        [HttpPost("metadata")]
        public async Task<ServicesResponse<ExpandedGameLegacy>> PostMetadata([FromBody]SdkModels.Game game)
        {
            return await GetMetadata(game, parsedController.GetExpandedGame);
        }

        private async Task<ServicesResponse<T>> GetMetadata<T>(SdkModels.Game game, Func<ulong, Task<T>> expandFunc) where T : new()
        {
            var isKnownPlugin = game.PluginId != Guid.Empty;
            var isSteamPlugin = BuiltinExtensions.GetIdFromExtension(BuiltinExtension.SteamLibrary) == game.PluginId;
            ulong igdbId = 0;
            var matchId = $"{game.GameId}{game.PluginId}".MD5();
            var searchId = $"{game.Name}{game.ReleaseDate?.Year}".MD5();

            // Check if match was previously found
            if (isKnownPlugin)
            {
                if (isSteamPlugin)
                {
                    igdbId = await igdbApi.GetSteamIgdbMatch(ulong.Parse(game.GameId));
                }
                else
                {
                    var match = Database.IGBDGameIdMatches.FindById(matchId);
                    if (match != null)
                    {
                        igdbId = match.IgdbId;
                    }
                }
            }

            if (igdbId == 0)
            {
                var match = Database.IGDBSearchIdMatches.FindById(searchId);
                if (match != null)
                {
                    igdbId = match.IgdbId;
                }
            }

            var foundMetadata = new T();
            if (igdbId != 0)
            {
                return new ServicesResponse<T>(await expandFunc(igdbId));
            }
            else
            {
                igdbId = await TryMatchGame(game, false);
                var useAlt = settings.Settings.IGDB.AlternativeSearch && !game.Name.ContainsAny(whereQueryBlacklist);
                if (useAlt && igdbId == 0)
                {
                    igdbId = await TryMatchGame(game, true);
                }

                if (igdbId != 0)
                {
                    foundMetadata = await expandFunc(igdbId);
                }
            }

            // Update match database if match was found
            if (igdbId != 0)
            {
                if (isKnownPlugin && !isSteamPlugin)
                {
                    Database.IGBDGameIdMatches.Upsert(new GameIdMatch
                    {
                        GameId = game.GameId,
                        Id = matchId,
                        IgdbId = igdbId,
                        Library = game.PluginId
                    });
                }

                Database.IGDBSearchIdMatches.Upsert(new SearchIdMatch
                {
                    Term = game.Name,
                    Id = searchId,
                    IgdbId = igdbId
                });
            }

            return new ServicesResponse<T>(foundMetadata);
        }

        private string FixNointroNaming(string name)
        {
            var match = noIntroArticleRegEx.Match(name);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim() + " " + noIntroArticleRegEx.Replace(name, "");
            }

            return name;
        }

        private async Task<ulong> TryMatchGame(SdkModels.Game game, bool alternativeSearch)
        {
            if (game.Name.IsNullOrEmpty())
            {
                return 0;
            }

            if (game.Name.ContainsAny(bracketsMatchList))
            {
                return 0;
            }

            ulong matchedGame = 0;
            var copyGame = game.GetClone();
            copyGame.Name = StringExtensions.NormalizeGameName(game.Name);
            copyGame.Name = FixNointroNaming(copyGame.Name);
            var name = copyGame.Name;
            name = Regex.Replace(name, @"\s+RHCP$", "", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, @"\s+RU$", "", RegexOptions.IgnoreCase);

            var results = await gamesController.GetSearchResults(name, alternativeSearch);
            results.ForEach(a => a.name = StringExtensions.NormalizeGameName(a.name));
            string testName = string.Empty;

            // Direct comparison
            matchedGame = MatchFun(game, name, results);
            if (matchedGame > 0)
            {
                return matchedGame;
            }

            // Try replacing roman numerals: 3 => III
            testName = Regex.Replace(name, @"\d+", ReplaceNumsForRomans);
            matchedGame = MatchFun(game, testName, results);
            if (matchedGame > 0)
            {
                return matchedGame;
            }

            // Try adding The
            testName = "The " + name;
            matchedGame = MatchFun(game, testName, results);
            if (matchedGame > 0)
            {
                return matchedGame;
            }

            // Try chaning & / and
            testName = Regex.Replace(name, @"\s+and\s+", " & ", RegexOptions.IgnoreCase);
            matchedGame = MatchFun(game, testName, results);
            if (matchedGame > 0)
            {
                return matchedGame;
            }

            // Try removing apostrophes
            var resCopy = results.GetClone();
            resCopy.ForEach(a => a.name = a.name.Replace("'", ""));
            matchedGame = MatchFun(game, name, resCopy);
            if (matchedGame > 0)
            {
                return matchedGame;
            }

            // Try removing all ":" and "-"
            testName = separatorRegex.Replace(name, " ");
            resCopy = results.GetClone();
            foreach (var res in resCopy)
            {
                res.name = separatorRegex.Replace(res.name, " ");
                if (res.alternative_names.HasItems())
                {
                    res.alternative_names = res.alternative_names.Where(a => !a.name.IsNullOrEmpty()).ToList();
                    res.alternative_names.ForEach(a => a.name = separatorRegex.Replace(a.name, " "));
                }
            }

            matchedGame = MatchFun(game, testName, resCopy);
            if (matchedGame > 0)
            {
                return matchedGame;
            }

            // Try without subtitle
            if (!alternativeSearch)
            {
                var testResult = results.OrderBy(a => a.first_release_date).FirstOrDefault(a =>
                {
                    if (a.first_release_date == 0)
                    {
                        return false;
                    }

                    if (!string.IsNullOrEmpty(a.name) && a.name.Contains(":"))
                    {
                        return string.Equals(name, a.name.Split(':')[0], StringComparison.InvariantCultureIgnoreCase);
                    }

                    return false;
                });

                if (testResult != null)
                {
                    return testResult.id;
                }
            }

            return 0;
        }

        private ulong MatchFun(SdkModels.Game game, string matchName, List<ExpandedGameLegacy> list)
        {
            var res = list.Where(a => string.Equals(matchName, a.name, StringComparison.InvariantCultureIgnoreCase));
            if (!res.Any())
            {
                res = list.Where(a =>
                a.alternative_names.HasItems() &&
                a.alternative_names.Select(b => b.name).ContainsString(matchName) == true);
            }

            if (res.Any())
            {
                if (res.Count() == 1)
                {
                    return res.First().id;
                }
                else
                {
                    if (game.ReleaseDate != null)
                    {
                        var igdbGame = res.FirstOrDefault(a => a.first_release_date.ToDateFromUnixMs().Year == game.ReleaseDate.Value.Year);
                        if (igdbGame != null)
                        {
                            return igdbGame.id;
                        }
                    }
                    else
                    {
                        // If multiple matches are found and we don't have release date then prioritize older game
                        if (res.All(a => a.first_release_date == 0))
                        {
                            return res.First().id;
                        }
                        else
                        {
                            var igdbGame = res.OrderBy(a => a.first_release_date).First(a => a.first_release_date > 0);
                            return igdbGame.id;
                        }
                    }
                }
            }

            return 0;
        }

        private string ReplaceNumsForRomans(Match m)
        {
            return Roman.To(int.Parse(m.Value));
        }
    }
}
