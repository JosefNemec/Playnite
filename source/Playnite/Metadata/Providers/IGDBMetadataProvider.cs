using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Services;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Playnite.SDK;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using Playnite.Common;

namespace Playnite.Metadata.Providers
{
    public class IGDBMetadataProvider : LibraryMetadataProvider
    {
        private ServicesClient client;

        public IGDBMetadataProvider() : this(new ServicesClient())
        {
        }

        public IGDBMetadataProvider(ServicesClient client)
        {
            this.client = client;
        }

        private GameInfo GetParsedGame(ulong id)
        {
            var dbGame = client.GetIGDBGameParsed(id);
            var game = new GameInfo()
            {
                Name = dbGame.name,
                Description = dbGame.summary?.Replace("\n", "\n<br>")
            };

            if (dbGame.cover != null)
            {
                game.CoverImage = dbGame.cover.Replace("t_thumb", "t_cover_big");
                if (!game.CoverImage.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
                {
                    game.CoverImage = "https:" + game.CoverImage;
                }
            }

            if (dbGame.first_release_date != 0)
            {
                game.ReleaseDate = DateTimeOffset.FromUnixTimeMilliseconds(dbGame.first_release_date).DateTime;
            }

            if (dbGame.developers?.Any() == true)
            {
                game.Developers = dbGame.developers;
            }

            if (dbGame.publishers?.Any() == true)
            {
                game.Publishers = dbGame.publishers;
            }

            if (dbGame.genres?.Any() == true)
            {
                game.Genres = dbGame.genres;
            }

            if (dbGame.websites?.Any() == true)
            {
                game.Links = dbGame.websites.Select(a => new Link(a.category.ToString(), a.url)).ToList();
            }

            if (dbGame.game_modes?.Any() == true)
            {
                var cultInfo = new CultureInfo("en-US", false).TextInfo;
                game.Tags = dbGame.game_modes.Select(a => cultInfo.ToTitleCase(a)).ToList();
            }

            if (dbGame.aggregated_rating != 0)
            {
                game.CriticScore = Convert.ToInt32(dbGame.aggregated_rating);
            }

            if (dbGame.rating != 0)
            {
                game.CommunityScore = Convert.ToInt32(dbGame.rating);
            }

            return game;
        }

        private string ReplaceNumsForRomans(Match m)
        {
            return Roman.To(int.Parse(m.Value));
        }

        public ICollection<MetadataSearchResult> SearchMetadata(Game game)
        {
            return client.GetIGDBGames(game.Name)?.Select(a => new MetadataSearchResult()
            {
                Id = a.id.ToString(),
                Name = a.name,
                ReleaseDate = a.first_release_date == 0 ? (DateTime?)null : DateTimeOffset.FromUnixTimeMilliseconds(a.first_release_date).DateTime,
                AlternativeNames = a.alternative_names?.Any() == true ? a.alternative_names.Select(name => name.name).ToList() : null
            }).ToList();
        }

        public GameMetadata GetMetadata(string gameId)
        {
            var game = GetParsedGame(ulong.Parse(gameId));
            MetadataFile image = null;
            if (!string.IsNullOrEmpty(game.CoverImage))
            {
                image = new MetadataFile(game.CoverImage);
                game.CoverImage = null;
            }

            return new GameMetadata(game, null, image, null);
        }

        public override GameMetadata GetMetadata(Game game)
        {
            if (game.PluginId == Guid.Parse("CB91DFC9-B977-43BF-8E70-55F46E410FAB"))
            {
                var igdbId = client.GetIGDBGameBySteamId(game.GameId);
                if (igdbId != 0)
                {
                    return GetMetadata(igdbId.ToString());
                }
            }

            if (string.IsNullOrEmpty(game.Name))
            {
                return GameMetadata.GetEmptyData();
            }

            var copyGame = game.GetClone();
            copyGame.Name = StringExtensions.NormalizeGameName(game.Name);
            var name = copyGame.Name;
            var results = SearchMetadata(copyGame).ToList();
            results.ForEach(a => a.Name = StringExtensions.NormalizeGameName(a.Name));            

            GameMetadata data = null;
            string testName = string.Empty;

            // Direct comparison
            data = matchFun(game, name, results);
            if (data != null)
            {
                return data;
            }

            // Try replacing roman numerals: 3 => III
            testName = Regex.Replace(name, @"\d+", ReplaceNumsForRomans);
            data = matchFun(game, testName, results);
            if (data != null)
            {
                return data;
            }

            // Try adding The
            testName = "The " + name;
            data = matchFun(game, testName, results);
            if (data != null)
            {
                return data;
            }

            // Try chaning & / and
            testName = Regex.Replace(name, @"\s+and\s+", " & ", RegexOptions.IgnoreCase);
            data = matchFun(game, testName, results);
            if (data != null)
            {
                return data;
            }

            // Try removing apostrophes
            var resCopy = results.GetClone();
            resCopy.ForEach(a => a.Name = a.Name.Replace("'", ""));
            data = matchFun(game, name, resCopy);
            if (data != null)
            {
                return data;
            }

            // Try removing all ":"
            testName = Regex.Replace(testName, @"\s*:\s*", " ");
            resCopy = results.GetClone();
            resCopy.ForEach(a => a.Name = Regex.Replace(a.Name, @"\s*:\s*", " "));
            data = matchFun(game, testName, resCopy);
            if (data != null)
            {
                return data;
            }

            // Try without subtitle
            var testResult = results.OrderBy(a => a.ReleaseDate).FirstOrDefault(a =>
            {
                if (a.ReleaseDate == null)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(a.Name) && a.Name.Contains(":"))
                {
                    return string.Equals(name, a.Name.Split(':')[0], StringComparison.InvariantCultureIgnoreCase);
                }

                return false;
            });

            if (testResult != null)
            {
                return GetMetadata(testResult.Id);
            }

            if (data != null)
            {
                return data;
            }
            else
            {
                return GameMetadata.GetEmptyData();
            }            
        }

        private GameMetadata matchFun(Game game, string matchName, IEnumerable<MetadataSearchResult> list)
        {
            var res = list.Where(a => string.Equals(matchName, a.Name, StringComparison.InvariantCultureIgnoreCase));
            if (!res.Any())
            {
                res = list.Where(a => a.AlternativeNames.ContainsString(matchName) == true);
            }

            if (res.Any())
            {
                if (res.Count() == 1)
                {
                    return GetMetadata(res.First().Id);
                }
                else
                {
                    if (game.ReleaseDate != null)
                    {
                        var igdbGame = res.FirstOrDefault(a => a.ReleaseDate?.Year == game.ReleaseDate.Value.Year);
                        if (igdbGame != null)
                        {
                            return GetMetadata(igdbGame.Id);
                        }
                    }
                    else
                    {
                        // If multiple matches are found and we don't have release date then prioritize older game
                        if (res.All(a => a.ReleaseDate == null))
                        {
                            return GetMetadata(res.First().Id);
                        }
                        else
                        {
                            var igdbGame = res.OrderBy(a => a.ReleaseDate?.Year).First(a => a.ReleaseDate != null);
                            return GetMetadata(igdbGame.Id);
                        }                        
                    }
                }
            }

            return null;
        }
    }
}
