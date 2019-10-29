using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using AngleSharp.Dom;
using AngleSharp.Parser.Html;
using Newtonsoft.Json;
using Playnite.Common.Web;
using Playnite.SDK;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

namespace Playnite.Metadata.Providers
{

    public class WikipediaSearchItem : GenericItemOption
    {
        public string Title { get; set; }
    }

    public class OnDemandWikipediaMetadata : OnDemandMetadataProvider
    {
        private readonly WikipediaMetadataPlugin plugin;
        private readonly MetadataRequestOptions options;
        private GameMetadata gameData;

        private List<MetadataField> availableFields;
        public override List<MetadataField> AvailableFields
        {
            get
            {
                if (availableFields == null)
                {
                    availableFields = GetAvailableFields();
                }

                return availableFields;
            }
        }

        public OnDemandWikipediaMetadata(WikipediaMetadataPlugin plugin, MetadataRequestOptions options)
        {
            this.options = options;
            this.plugin = plugin;
        }

        public override DateTime? GetReleaseDate()
        {
            if (AvailableFields.Contains(MetadataField.ReleaseDate))
            {
                return gameData.GameInfo.ReleaseDate;
            }

            return base.GetReleaseDate();
        }

        public override List<string> GetDevelopers()
        {
            if (AvailableFields.Contains(MetadataField.Developers))
            {
                return gameData.GameInfo.Developers;
            }

            return base.GetDevelopers();
        }

        public override List<string> GetPublishers()
        {
            if (AvailableFields.Contains(MetadataField.Publishers))
            {
                return gameData.GameInfo.Publishers;
            }

            return base.GetPublishers();
        }

        public override MetadataFile GetCoverImage()
        {
            if (AvailableFields.Contains(MetadataField.ReleaseDate))
            {
                return gameData.CoverImage;
            }

            return base.GetCoverImage();
        }

        public override string GetName()
        {
            if (AvailableFields.Contains(MetadataField.Name))
            {
                return gameData.GameInfo.Name;
            }

            return base.GetName();
        }

        private List<MetadataField> GetAvailableFields()
        {
            var fields = new List<MetadataField>();
            if (gameData == null)
            {
                GetData();
            }

            if (!gameData.IsEmpty)
            {
                fields.Add(MetadataField.Name);
                if (gameData.GameInfo.Publishers.HasItems())
                {
                    fields.Add(MetadataField.Publishers);
                }

                if (gameData.GameInfo.Developers.HasItems())
                {
                    fields.Add(MetadataField.Developers);
                }

                if (gameData.GameInfo.ReleaseDate != null)
                {
                    fields.Add(MetadataField.ReleaseDate);
                }

                if (gameData.CoverImage != null)
                {
                    fields.Add(MetadataField.CoverImage);
                }
            }

            return fields;
        }


        private void GetData()
        {
            if (options.IsBackgroundDownload)
            {
                throw new NotImplementedException();
            }
            else
            {
                var item = plugin.PlayniteApi.Dialogs.ChooseItemWithSearch(null, (a) => plugin.SearchMetadata(a), options.GameData.Name);
                if (item == null)
                {
                    gameData = GameMetadata.GetEmptyData();
                }
                else
                {
                    var searchItem = item as WikipediaSearchItem;
                    gameData = plugin.ParseGamePage(plugin.GetPage(searchItem.Title));
                }
            }
        }
    }

    public class WikipediaMetadataPlugin : MetadataPlugin
    {
        private static ILogger logger = LogManager.GetLogger();

        public override string Name => "Wikipedia";

        public override List<MetadataField> SupportedFields { get; } = new List<MetadataField>
        {
            MetadataField.ReleaseDate,
            MetadataField.Developers,
            MetadataField.Publishers,
            MetadataField.CoverImage,
            MetadataField.Name
        };

        public override Guid Id { get; } = new Guid("88A920B3-B35C-4C30-A8E7-88BFCC2320EE");

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

        public WikipediaMetadataPlugin(IPlayniteAPI playniteAPI) : base(playniteAPI)
        {
        }

        public List<GenericItemOption> SearchMetadata(string searchTerm)
        {
            if (searchTerm.IsNullOrEmpty())
            {
                return new List<GenericItemOption>();
            }

            var url = string.Format(@"https://en.wikipedia.org/w/api.php?action=query&srsearch={0}&format=json&list=search&srlimit=50", HttpUtility.UrlEncode(searchTerm));
            var stringResult = HttpDownloader.DownloadString(url);
            var result = JsonConvert.DeserializeObject<SearchResponse>(stringResult);

            if (result.error != null)
            {
                throw new Exception(result.error.info);
            }
            else
            {
                var ret = new List<GenericItemOption>();
                var parser = new HtmlParser();
                foreach (var searchResult in result.query.search)
                {
                    ret.Add(new WikipediaSearchItem
                    {
                        Title = searchResult.title,
                        Name = searchResult.title,
                        Description = parser.Parse(searchResult.snippet).DocumentElement.TextContent
                    });
                }

                return ret;
            }
        }

        public WikiPage GetPage(string pageTitle)
        {
            var url = string.Format(@"https://en.wikipedia.org/w/api.php?action=parse&page={0}&format=json", HttpUtility.UrlEncode(pageTitle));
            var stringResult = HttpDownloader.DownloadString(url);
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

        public GameMetadata ParseGamePage(WikiPage page, string gameName = "")
        {
            logger.Info("Parsing wiki page " + page.title);            
            var gameInfo = new GameInfo();
            var metadata = new GameMetadata() { GameInfo = gameInfo };
            var parser = new HtmlParser();
            var document = parser.Parse(@"<html><head></head><body>" + page.text["*"] + @"</body></html>?");
            var tables = document.QuerySelectorAll("table.infobox.hproduct");

            if (tables.Length == 0)
            {
                return metadata;
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
                gameInfo.Name = rows[0].QuerySelector("th").TextContent.Replace('\n', ' ');
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

            if (!image.IsNullOrEmpty())
            {
                metadata.CoverImage = new MetadataFile(image);
            }

            // Other fields
            var gameProperties = new Dictionary<string, string>();
            int startIndex = (string.IsNullOrEmpty(image) || string.IsNullOrEmpty(gameInfo.Name)) ? 1 : 2;
            for (int i = startIndex; i < rows.Length; i++)
            {
                var row = rows[i];
                var rowName = row.QuerySelector("th").TextContent;
                var rowValue = row.QuerySelector("td").TextContent;
                gameProperties.Add(rowName, rowValue);

                if (rowName.IndexOf("developer", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    gameInfo.Developers = rowValue.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(a => Regex.Replace(a, @"\[\d+\]", "").Trim()).ToList();
                    
                    continue;
                }

                if (rowName.IndexOf("publisher", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    gameInfo.Publishers = rowValue.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(a => Regex.Replace(a, @"\[\d+\]", "").Trim()).ToList();
                    continue;
                }

                if (rowName.IndexOf("genre", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    gameInfo.Genres = rowValue.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(a => Regex.Replace(a, @"\[\d+\]", "").Trim()).ToList();
                    continue;
                }

                if (rowName.IndexOf("release", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    rowValue = Regex.Replace(rowValue, "[A-Z]+:", "\n");
                    var dates = rowValue.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();

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
                            gameInfo.ReleaseDate = dateTime;
                            break;
                        }
                    }

                    if (gameInfo.ReleaseDate != null)
                    {
                        continue;
                    }
                }
            }

            return metadata;
        }

        public override OnDemandMetadataProvider GetMetadataProvider(MetadataRequestOptions options)
        {
            return new OnDemandWikipediaMetadata(this, options);
        }
    }
}
