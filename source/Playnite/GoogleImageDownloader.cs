using AngleSharp.Parser.Html;
using Flurl;
using Newtonsoft.Json;
using Playnite.Common;
using Playnite.SDK;
using Playnite.WebView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite
{
    public class GoogleImage
    {
        [JsonProperty("ow")]
        public uint Width { get; set; }

        [JsonProperty("oh")]
        public uint Height { get; set; }

        [JsonProperty("ou")]
        public string ImageUrl { get; set; }

        [JsonProperty("tu")]
        public string ThumbUrl { get; set; }

        public string Size => $"{Width}x{Height}";
    }

    public class GoogleImageDownloader : IDisposable
    {
        private static ILogger logger = LogManager.GetLogger();

        private readonly OffscreenWebView webView;
        public GoogleImageDownloader()
        {
            webView = new OffscreenWebView();
        }

        public void Dispose()
        {
            webView.Dispose();
        }

        public async Task<List<GoogleImage>> GetImages(string searchTerm, SafeSearchSettings safeSearch, bool transparent = false)
        {
            var images = new List<GoogleImage>();
            var parser = new HtmlParser();
            var url = new Url(@"https://www.google.com/search");
            url.SetQueryParam("tbm", "isch");
            url.SetQueryParam("client", "firefox-b-d");
            url.SetQueryParam("source", "lnt");
            url.SetQueryParam("q", searchTerm);

            if (safeSearch == SafeSearchSettings.On)
            {
                url.SetQueryParam("safe", "on");
            }
            else if (safeSearch == SafeSearchSettings.Off)
            {
                url.SetQueryParam("safe", "off");
            }

            if (transparent)
            {
                url.SetQueryParam("tbs", "ic:trans");
            }

            webView.NavigateAndWait(url.ToString());
            if (webView.GetCurrentAddress().StartsWith(@"https://consent.google.com", StringComparison.OrdinalIgnoreCase))
            {
                // This rejects Google's consent form for cookies
                await webView.EvaluateScriptAsync(@"document.getElementsByTagName('form')[0].submit();");
                await Task.Delay(3000);
                webView.NavigateAndWait(url.ToString());
            }

            var googleContent = await webView.GetPageSourceAsync();
            if (googleContent.Contains(".rg_meta", StringComparison.Ordinal))
            {
                var document = parser.Parse(googleContent);
                foreach (var imageElem in document.QuerySelectorAll(".rg_meta"))
                {
                    images.Add(Serialization.FromJson<GoogleImage>(imageElem.InnerHtml));
                }
            }
            else
            {
                var formatted = Regex.Replace(googleContent, @"\r\n?|\n", string.Empty);
                var matches = Regex.Matches(formatted, @"\[""(https:\/\/encrypted-[^,]+?)"",\d+,\d+\],\[""(http.+?)"",(\d+),(\d+)\]");
                foreach (Match match in matches)
                {
                    var data = Serialization.FromJson<List<List<object>>>($"[{match.Value}]");
                    images.Add(new GoogleImage
                    {
                        ThumbUrl = data[0][0].ToString(),
                        ImageUrl = data[1][0].ToString(),
                        Height = uint.Parse(data[1][1].ToString()),
                        Width = uint.Parse(data[1][2].ToString())
                    });
                }
            }

            if (!images.HasItems())
            {
                logger.Error("Failed to parse any Google image results.");
                logger.Debug(googleContent);
            }

            return images;
        }
    }
}
