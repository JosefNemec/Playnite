using AngleSharp.Parser.Html;
using Flurl;
using Newtonsoft.Json;
using Playnite.Common;
using Playnite.WebView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
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

        [JsonProperty("ity")]
        public string Extension { get; set; }

        public string Size => $"{Width}x{Height}";
    }

    public class GoogleImageDownloader : IDisposable
    {
        private readonly OffscreenWebView webView;
        public GoogleImageDownloader()
        {
            webView = new OffscreenWebView();
        }

        public void Dispose()
        {
            webView.Dispose();
        }

        public async Task<List<GoogleImage>> GetImages(string searchTerm, bool transparent = false)
        {
            var images = new List<GoogleImage>();
            var parser = new HtmlParser();
            var url = new Url(@"https://www.google.com/search");
            url.SetQueryParam("tbm", "isch");
            url.SetQueryParam("client", "firefox-b-d");
            url.SetQueryParam("source", "lnt");
            url.SetQueryParam("q", searchTerm);
            if (transparent)
            {
                url.SetQueryParam("tbs", "ic:trans");
            }

            webView.NavigateAndWait(url.ToString());
            var googleContent = await webView.GetPageSourceAsync();
            var document = parser.Parse(googleContent);
            foreach (var imageElem in document.QuerySelectorAll(".rg_meta"))
            {
                images.Add(Serialization.FromJson<GoogleImage>(imageElem.InnerHtml));
            }

            return images;
        }
    }
}
