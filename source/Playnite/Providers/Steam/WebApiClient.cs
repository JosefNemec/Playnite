using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using Playnite.Web;

namespace Playnite.Providers.Steam
{
    public class WebApiClient
    {
        public static StoreAppDetailsResult ParseStoreData(int appId, string data)
        {
            var parsedData = JsonConvert.DeserializeObject<Dictionary<string, StoreAppDetailsResult>>(data);
            return parsedData[appId.ToString()];
        }

        public static string GetRawStoreAppDetail(int appId)
        {
            var url = @"http://store.steampowered.com/api/appdetails?appids={0}";
            url = string.Format(url, appId);
            return HttpDownloader.DownloadString(url);
        }

        public static StoreAppDetailsResult.AppDetails GetStoreAppDetail(int appId)
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
