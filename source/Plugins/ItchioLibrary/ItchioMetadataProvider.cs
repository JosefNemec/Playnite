using AngleSharp.Dom;
using AngleSharp.Parser.Html;
using Playnite.SDK;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using Playnite.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ItchioLibrary
{
    public class ItchioMetadataProvider : ILibraryMetadataProvider
    {
        private Butler butler;

        public ItchioMetadataProvider()
        {
            butler = new Butler();
        }

        public void Dispose()
        {
            butler.Dispose();
        }

        #region IMetadataProvider

        public GameMetadata GetMetadata(Game game)
        {
            var gameData = new Game()
            {
                Links = new ObservableCollection<Link>(),
                Tags = new ComparableList<string>(),
                Genres = new ComparableList<string>()
            };

            var metadata = new GameMetadata
            {
                GameData = gameData
            };

            var itchGame = butler.GetGame(Convert.ToInt32(game.GameId));

            // Cover image
            if (!string.IsNullOrEmpty(itchGame.coverUrl))
            {
                var cover = HttpDownloader.DownloadData(itchGame.coverUrl);
                var coverFile = Path.GetFileName(itchGame.coverUrl);
                metadata.Image = new MetadataFile(coverFile, cover);
            }

            if (!string.IsNullOrEmpty(itchGame.url))
            {
                gameData.Links.Add(new Link("Store", itchGame.url));
                var gamePageSrc = HttpDownloader.DownloadString(itchGame.url);
                var parser = new HtmlParser();
                var gamePage = parser.Parse(gamePageSrc);
                
                // Description
                gameData.Description = gamePage.QuerySelector(".formatted_description").InnerHtml;

                // Background
                var gameTheme = gamePage.QuerySelector("#game_theme").InnerHtml;
                var bckMatch = Regex.Match(gameTheme, @"background-image:\surl\((.+?)\)");
                if (bckMatch.Success)
                {
                    metadata.BackgroundImage = bckMatch.Groups[1].Value;
                    gameData.BackgroundImage = bckMatch.Groups[1].Value;
                }                

                // Other info
                var infoPanel = gamePage.QuerySelector(".game_info_panel_widget");
                var fields = infoPanel.QuerySelectorAll("tr");
                foreach (var field in fields)
                {
                    var name = field.QuerySelectorAll("td")[0].TextContent;
                    if (name == "Genre")
                    {
                        foreach (var item in field.QuerySelectorAll("a"))
                        {                            
                            gameData.Genres.Add(item.TextContent);
                        }

                        continue;
                    }

                    if (name == "Tags")
                    {
                        foreach (var item in field.QuerySelectorAll("a"))
                        {
                            gameData.Tags.Add(item.TextContent);
                        }

                        continue;
                    }

                    if (name == "Links")
                    {
                        foreach (var item in field.QuerySelectorAll("a"))
                        {
                            gameData.Links.Add(new Link(item.TextContent, item.Attributes["href"].Value));
                        }

                        continue;
                    }

                    if (name == "Author")
                    {
                        gameData.Developers = new ComparableList<string> { field.ChildNodes[1].TextContent };
                    }

                    if (name == "Release date")
                    {
                        var strDate = field.QuerySelector("abbr").Attributes["title"].Value.Split('@')[0].Trim();
                        if (DateTime.TryParseExact(strDate, "d MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
                        {
                            gameData.ReleaseDate = dateTime;
                        }                        
                    }
                }
            }

            return metadata;
        }

        #endregion IMetadataProvider
    }
}
