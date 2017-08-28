using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Parser.Html;
using Newtonsoft.Json;
using Playnite.Models;
using Playnite.Providers;
using NLog;

namespace Playnite.MetaProviders
{
    public class Wikipedia
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public class Error
        {
            public string code
            {
                get; set;
            }
            public string info
            {
                get; set;
            }
        }

        public class SearchResponse
        {
            public Error error
            {
                get; set;
            }
            public SearchQueryResult query
            {
                get; set;
            }
        }

        public class SearchQueryResult
        {
            public List<SearchResult> search
            {
                get; set;
            }
        }

        public class SearchResult
        {
            public string title
            {
                get;set;
            }
            public string snippet
            {
                get; set;
            }
        }

        public class ParseResponse
        {
            public Error error
            {
                get; set;
            }
            public WikiPage parse
            {
                get; set;
            }
        }

        public class WikiPage
        {
            public string title
            {
                get; set;
            }
            public Dictionary<string, string> text
            {
                get; set;
            }
        }

        public Wikipedia()
        {
        }

        public List<SearchResult> Search(string searchTerm)
        {
            var url = string.Format(@"https://en.wikipedia.org/w/api.php?action=query&srsearch={0}&format=json&list=search&srlimit=50", HttpUtility.UrlEncode(searchTerm));
            var stringResult = Web.DownloadString(url);
            var result = JsonConvert.DeserializeObject<SearchResponse>(stringResult);

            if (result.error != null)
            {
                throw new Exception(result.error.info);
            }
            else
            {
                var parser = new HtmlParser();
                foreach (var searchResult in result.query.search)
                {
                    searchResult.snippet = parser.Parse(searchResult.snippet).DocumentElement.TextContent;
                }

                return result.query.search;
            }
        }

        public WikiPage GetPage(string pageTitle)
        {
            var url = string.Format(@"https://en.wikipedia.org/w/api.php?action=parse&page={0}&format=json", HttpUtility.UrlEncode(pageTitle));
            var stringResult = Web.DownloadString(url);
            var result = JsonConvert.DeserializeObject<ParseResponse>(stringResult);

            if (result.error != null)
            {
                throw new Exception(result.error.info);
            }
            else
            {
                return result.parse;
            }
        }

        public Game ParseGamePage(WikiPage page, string gameName = "")
        {
            logger.Info("Parsing wiki page " + page.title);
            var game = new Game();
            var parser = new HtmlParser();
            var document = parser.Parse(@"<html><head></head><body>" + page.text["*"] + @"</body></html>?");
            var tables = document.QuerySelectorAll("table.infobox.hproduct");

            if (tables.Length == 0)
            {
                return game;
            }

            IElement infoTable = null;
            if (tables.Length == 1)
            {
                infoTable = tables[0];
            }
            else
            {
                // Get correct property table for cases where there are more games on single page
                var compareName = string.IsNullOrEmpty(gameName) ? page.title : gameName;

                foreach (var info in tables)
                {
                    var name = info.QuerySelector("tr").QuerySelector("th").TextContent;
                    if (string.Compare(compareName.Trim(), name.Trim(), true) == 0)
                    {
                        infoTable = info;
                        break;
                    }
                }

                if (infoTable == null)
                {
                    throw new Exception(string.Format("Couldn't find info about game {0} on page {1}", compareName, page.title));
                }
            }

            var rows = infoTable.QuerySelectorAll("tr");
            var imageRowIndex = 0;

            // Name
            var nameField = rows[0].QuerySelector("th");
            if (nameField != null)
            {
                game.Name = rows[0].QuerySelector("th").TextContent.Replace('\n', ' ');
                imageRowIndex = 1;
            }

            // Box art
            var boxartElem = rows[imageRowIndex].GetElementsByTagName("img");
            string image =  string.Empty;
            if (boxartElem.Length != 0)
            {
                if (boxartElem[0].HasAttribute("srcset"))
                {
                    image = boxartElem[0].Attributes["srcset"].Value;
                    if (image.EndsWith("x"))
                    {
                        var imageStrings = image.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        var images = new List<string>();
                        foreach (var imageString in imageStrings)
                        {
                            var match = Regex.Match(imageString, @"(.+)\s.+x");
                            if (match.Success)
                            {
                                images.Add(match.Groups[1].Value);
                            }
                        }

                        image = images.Last().Trim();
                    }
                }
                else
                {
                    image = boxartElem[0].Attributes["src"].Value;
                }

                image = "http:" + image;
            }

            game.Image = image;

            // Other fields
            var gameProperties = new Dictionary<string, string>();
            int startIndex = (string.IsNullOrEmpty(image) || string.IsNullOrEmpty(game.Name)) ? 1 : 2;
            for (int i = startIndex; i < rows.Length; i++)
            {
                var row = rows[i];
                var rowName = row.QuerySelector("th").TextContent;
                var rowValue = row.QuerySelector("td").TextContent;
                gameProperties.Add(rowName, rowValue);

                if (rowName.IndexOf("developer", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    game.Developers = new ComparableList<string>(rowValue.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(a => Regex.Replace(a, @"\[\d+\]", "").Trim()));
                    
                    continue;
                }

                if (rowName.IndexOf("publisher", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    game.Publishers = new ComparableList<string>(rowValue.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(a => Regex.Replace(a, @"\[\d+\]", "").Trim()));
                    continue;
                }

                if (rowName.IndexOf("genre", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    game.Genres = new ComparableList<string>(rowValue.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(a => Regex.Replace(a, @"\[\d+\]", "").Trim()));
                    continue;
                }

                if (rowName.IndexOf("release", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    List<string> dates= rowValue.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    // Take first valid date we find
                    foreach (var stringDate in dates)
                    {
                        var strDate = stringDate.Trim();

                        // Remove region
                        if (strDate.Contains(':') && !strDate.EndsWith(":"))
                        {
                            strDate = strDate.Substring(strDate.IndexOf(':') + 2);
                        }

                        // Remove region (differen format)
                        if (strDate.Contains('('))
                        {
                            strDate = strDate.Remove(strDate.IndexOf('('));
                        }

                        // Remove annotation
                        if (strDate.Contains('['))
                        {
                            strDate = strDate.Remove(strDate.IndexOf('['));
                        }

                        strDate = strDate.Trim();

                        bool validDate = false;
                        if (DateTime.TryParseExact(strDate, "MMMM dd, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
                        {
                            validDate = true;
                        }
                        else if (DateTime.TryParseExact(strDate, "MMMM d, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                        {
                            validDate = true;
                        }
                        else if (DateTime.TryParseExact(strDate, "dd MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                        {
                            validDate = true;
                        }
                        else if (DateTime.TryParseExact(strDate, "d MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                        {
                            validDate = true;
                        }
                        else if (DateTime.TryParseExact(strDate, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                        {
                            validDate = true;
                        }
                        else if (DateTime.TryParseExact(strDate, "MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                        {
                            validDate = true;
                        }

                        if (validDate)
                        {
                            game.ReleaseDate = dateTime;
                            break;
                        }
                    }

                    if (game.ReleaseDate != null)
                    {
                        continue;
                    }
                }
            }

            return game;
        }
    }
}
