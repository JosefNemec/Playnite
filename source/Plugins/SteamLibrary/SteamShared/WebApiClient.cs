using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Playnite.Common.Web;
using Steam.Models;

namespace Steam
{
    public class WebApiClient
    {
        public static AppReviewsResult GetUserRating(uint appId)
        {
            var url = $"https://store.steampowered.com/appreviews/{appId}?json=1&purchase_type=all&language=all";
            return JsonConvert.DeserializeObject<AppReviewsResult>(HttpDownloader.DownloadString(url));
        }

        public static StoreAppDetailsResult.AppDetails GetStoreAppDetail(uint appId)
        {
            var url = $"https://store.steampowered.com/api/appdetails?appids={appId}&l=english";
            var parsedData = JsonConvert.DeserializeObject<Dictionary<string, StoreAppDetailsResult>>(HttpDownloader.DownloadString(url));
            var response = parsedData[appId.ToString()];

            // No store data for this appid
            if (response.success != true)
            {
                return null;
            }

            return response.data;
        }
    }
}
