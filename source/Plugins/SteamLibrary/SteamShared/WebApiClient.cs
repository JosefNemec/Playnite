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
        public static StoreAppDetailsResult ParseStoreData(uint appId, string data)
        {
            var parsedData = JsonConvert.DeserializeObject<Dictionary<string, StoreAppDetailsResult>>(data);
            return parsedData[appId.ToString()];
        }

        public static string GetRawStoreAppDetail(uint appId)
        {
            var url = $"https://store.steampowered.com/api/appdetails?appids={appId}&l=english";
            return HttpDownloader.DownloadString(url);
        }

        public static StoreAppDetailsResult.AppDetails GetStoreAppDetail(uint appId)
        {
            var data = GetRawStoreAppDetail(appId);
            var response = ParseStoreData(appId, data);

            // No store data for this appid
            if (response.success != true)
            {
                return null;
            }

            return response.data;
        }
    }
}
