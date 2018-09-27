using GogLibrary.Models;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GogLibrary.Services
{
    public class GogApiClient
    {
        private ILogger logger = LogManager.GetLogger();

        public GogApiClient()
        {
        }

        public StorePageResult.ProductDetails GetGameStoreData(string gameUrl)
        {
            string[] data;

            try
            {
                data = HttpDownloader.DownloadString(gameUrl, new List<System.Net.Cookie>() { new System.Net.Cookie("gog_lc", Gog.EnStoreLocaleString) }).Split('\n');
            }
            catch (WebException e)
            {
                var response = (HttpWebResponse)e.Response;
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }

            foreach (var line in data)
            {
                var trimmed = line.TrimStart();
                if (line.TrimStart().StartsWith("var gogData"))
                {
                    var stringData = trimmed.Substring(14).TrimEnd(';');
                    var desData = JsonConvert.DeserializeObject<StorePageResult>(stringData);

                    if (desData.gameProductData == null)
                    {
                        return null;
                    }

                    return desData.gameProductData;
                }
            }

            throw new Exception("Failed to get store data from page, no data found. " + gameUrl);
        }

        public ProductApiDetail GetGameDetails(string id)
        {
            var baseUrl = @"http://api.gog.com/products/{0}?expand=description";

            try
            {
                var stringData = HttpDownloader.DownloadString(string.Format(baseUrl, id), new List<System.Net.Cookie>() { new System.Net.Cookie("gog_lc", Gog.EnStoreLocaleString) });
                return JsonConvert.DeserializeObject<ProductApiDetail>(stringData);
            }
            catch (WebException exc)
            {
                logger.Warn(exc, "Failed to download GOG game details for " + id);
                return null;
            }
        }
    }
}
