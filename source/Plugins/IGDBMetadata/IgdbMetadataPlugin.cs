using IGDBMetadata.Models;
using IGDBMetadata.Services;
using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace IGDBMetadata
{
    public class IgdbMetadataPlugin : MetadataPlugin
    {
        public class IgdbImageOption : ImageFileOption
        {
            public PlayniteServices.Models.IGDB.GameImage Image { get; set; }
        }

        private IgdbServiceClient client;

        public override string Name { get; } = "IGDB";
        public override Guid Id { get; } = Guid.Parse("000001DB-DBD1-46C6-B5D0-B1BA559D10E4");
        internal readonly IgdbMetadataSettings Settings;
        public override List<GameField> SupportedFields { get; } = new List<GameField>
        {
            GameField.Description,
            GameField.CoverImage,
            GameField.BackgroundImage,
            GameField.ReleaseDate,
            GameField.Developers,
            GameField.Publishers,
            GameField.Genres,
            GameField.Links,
            GameField.Tags,
            GameField.CriticScore,
            GameField.CommunityScore
        };

        public IgdbMetadataPlugin(IPlayniteAPI playniteAPI) : base(playniteAPI)
        {
            client = new IgdbServiceClient(playniteAPI.ApplicationInfo.ApplicationVersion);
            Settings = new IgdbMetadataSettings(this);
        }

        public override GameMetadata GetMetadata(MetadataRequestOptions options)
        {
            if (!options.IsBackgroundDownload)
            {
                var item = PlayniteApi.Dialogs.ChooseItemWithSearch(null, (a) =>
                {
                    var res = SearchMetadata(a);
                    return res.Select(b => b as GenericItemOption).ToList();
                }, options.GameData.Name);

                if (item == null)
                {
                    return null;
                }
                else
                {
                    var searchItem = item as SearchResult;
                    return GetMetadata(searchItem.Id);
                }
            }
            else
            {
                var game = options.GameData;
                if (BuiltinExtensions.GetExtensionFromId(game.PluginId) == BuiltinExtension.SteamLibrary)
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
                var results = SearchMetadata(GetIgdbSearchString(name)).ToList();
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

                // Try removing all ":" and "-"
                testName = Regex.Replace(name, @"\s*(:|-)\s*", " ");
                resCopy = results.GetClone();
                resCopy.ForEach(a => a.Name = Regex.Replace(a.Name, @"\s*(:|-)\s*", " "));
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
        }

        internal static string GetImageUrl(PlayniteServices.Models.IGDB.GameImage image, string imageSize)
        {
            var url = image.url;
            if (!url.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
            {
                url = "https:" + url;
            }

            url = Regex.Replace(url, @"\/t_[^\/]+", "/t_" + imageSize);
            return url;
        }

        private GameInfo GetParsedGame(ulong id)
        {
            var dbGame = client.GetIGDBGameParsed(id);
            var game = new GameInfo()
            {
                Name = dbGame.name,
                Description = dbGame.summary?.Replace("\n", "\n<br>")
            };

            // TODO use pngs in original size
            if (dbGame.cover != null)
            {
                game.CoverImage = GetImageUrl(dbGame.cover_v3, ImageSizes.cover_big);
            }

            List<PlayniteServices.Models.IGDB.GameImage> possibleBackgrounds = null;
            if (dbGame.artworks.HasItems())
            {
                possibleBackgrounds = dbGame.artworks;
            }
            else if (Settings.UseScreenshotsIfNecessary && dbGame.screenshots.HasItems())
            {
                possibleBackgrounds = dbGame.screenshots;
            }

            // TODO use pngs once IGBD resize issue is fixed
            if (possibleBackgrounds.HasItems())
            {
                PlayniteServices.Models.IGDB.GameImage selected = null;
                if (possibleBackgrounds.Count == 1)
                {
                    selected = possibleBackgrounds[0];
                }
                else
                {
                    if (Settings.ImageSelectionPriority == MultiImagePriority.First)
                    {
                        selected = possibleBackgrounds[0];
                    }
                    else if (Settings.ImageSelectionPriority == MultiImagePriority.Random ||
                        (Settings.ImageSelectionPriority == MultiImagePriority.Select && PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Fullscreen))
                    {
                        var index = GlobalRandom.Next(0, possibleBackgrounds.Count - 1);
                        selected = possibleBackgrounds[index];
                    }
                    else if (Settings.ImageSelectionPriority == MultiImagePriority.Select && PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
                    {
                        var selection = new List<ImageFileOption>();
                        foreach (var artwork in possibleBackgrounds)
                        {
                            selection.Add(new IgdbImageOption
                            {
                                Path = GetImageUrl(artwork, ImageSizes.screenshot_med),
                                Image = artwork
                            });
                        }

                        var sel = PlayniteApi.Dialogs.ChooseImageFile(
                            selection,
                            string.Format(PlayniteApi.Resources.GetString("LOCIgdbSelectBackgroundTitle"), dbGame.name)) as IgdbImageOption;
                        selected = sel?.Image;
                    }
                }

                if (selected != null)
                {
                    if (selected.height > 1080)
                    {
                        game.BackgroundImage = GetImageUrl(selected, ImageSizes.p1080);
                    }
                    else
                    {
                        game.BackgroundImage = GetImageUrl(selected, ImageSizes.original);
                    }
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
                game.Links = dbGame.websites.Where(a => !a.url.IsNullOrEmpty()).Select(a => new Link(a.category.ToString(), a.url)).ToList();
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

        public List<SearchResult> SearchMetadata(string gameName)
        {
            return client.GetIGDBGames(gameName)?.Select(a => new SearchResult(
                a.id.ToString(),
                a.name.RemoveTrademarks(),
                a.first_release_date == 0 ? (DateTime?)null : DateTimeOffset.FromUnixTimeMilliseconds(a.first_release_date).DateTime,
                a.alternative_names?.Any() == true ? a.alternative_names.Select(name => name.name.RemoveTrademarks()).ToList() : null,
                null)).ToList();
        }

        public GameMetadata GetMetadata(string id)
        {
            var game = GetParsedGame(ulong.Parse(id));
            var metadata = new GameMetadata() { GameInfo = game };
            if (!game.CoverImage.IsNullOrEmpty())
            {
                metadata.CoverImage = new MetadataFile(game.CoverImage);
                game.CoverImage = null;
            }

            if (!game.BackgroundImage.IsNullOrEmpty())
            {
                metadata.BackgroundImage = new MetadataFile(game.BackgroundImage);
                game.BackgroundImage = null;
            }

            return metadata;
        }

        private string GetIgdbSearchString(string gameName)
        {
            var temp = gameName.Replace(":", " ").Replace("-", " ");
            return Regex.Replace(temp, @"\s+", " ");
        }

        private GameMetadata matchFun(Game game, string matchName, IEnumerable<SearchResult> list)
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

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        public override UserControl GetSettingsView(bool firstRunView)
        {
            return new IgdbMetadataSettingsView();
        }
    }
}
