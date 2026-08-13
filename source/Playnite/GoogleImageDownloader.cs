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

    public class DDGImageSearchResult
    {
        public class Results
        {
            public uint height { get; set; }
            public uint width { get; set; }
            public string thumbnail { get; set; }
            public string image { get; set; }
        }

        public Results[] results { get; set; }
    }

    public class GoogleImageDownloader : IDisposable
    {
        private static ILogger logger = LogManager.GetLogger();

        private readonly OffscreenWebView webView;
        private TaskCompletionSource<DDGImageSearchResult> ddgResult = null;

        public GoogleImageDownloader()
        {
            webView = new OffscreenWebView(new WebViewSettings
            {
                PassResourceContentStreamToCallback = true,
                ShouldPassResourceContentFunc = (a) => UrlMatchesDdgImageSearch(a.Request.Url),
                ResourceLoadedCallback = ResourceLoadedCallback
            });
        }

        public void Dispose()
        {
            webView.Dispose();
        }

        private bool UrlMatchesDdgImageSearch(string url)
        {
            return url?.Contains("duckduckgo.com/i.js", StringComparison.OrdinalIgnoreCase) == true;
        }

        private void ResourceLoadedCallback(WebViewResourceLoadedCallback args)
        {
            if (!UrlMatchesDdgImageSearch(args.Request.Url))
                return;

            args.ResponseContent.Seek(0, SeekOrigin.Begin);
            if (Serialization.TryFromJsonStream<DDGImageSearchResult>(args.ResponseContent, out var searchResult))
                ddgResult.SetResult(searchResult);
        }

        public List<GoogleImage> GetDdgImages(string searchTerm, bool transparent = false)
        {
            ddgResult = new TaskCompletionSource<DDGImageSearchResult>();
            var url = new Url("https://duckduckgo.com");
            url.SetQueryParam("ia", "images");
            url.SetQueryParam("iax", "images");
            url.SetQueryParam("q", searchTerm);

            if (transparent)
                url.SetQueryParam("iaf", "type:transparent");

            webView.NavigateAndWait(url.ToString());
            if (!ddgResult.Task.Wait(TimeSpan.FromSeconds(10)))
                return new List<GoogleImage>();

            var results = ddgResult.Task.Result;
            if (results?.results.HasItems() == true)
            {
                return results.results.Select(a => new GoogleImage
                {
                    Height = a.height,
                    Width = a.width,
                    ThumbUrl = a.thumbnail,
                    ImageUrl = a.image,
                }).ToList();
            }

            return new List<GoogleImage>();
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
                    var imageUrl = data[1][0].ToString();
                    if (images.Any(a => a.ImageUrl.Equals(imageUrl, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    images.Add(new GoogleImage
                    {
                        ThumbUrl = data[0][0].ToString(),
                        ImageUrl = imageUrl,
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
