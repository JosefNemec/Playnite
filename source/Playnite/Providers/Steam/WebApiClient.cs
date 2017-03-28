using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;

namespace Playnite.Providers.Steam
{
    public class WebApiClient
    {
        public static StoreAppDetailsResult.AppDetails GetStoreAppDetail(int appId)
        {
            var url = @"http://store.steampowered.com/api/appdetails?appids={0}";
            url = string.Format(url, appId);
            var data = Web.DownloadString(url);
            var parsedData = JsonConvert.DeserializeObject<Dictionary<string, StoreAppDetailsResult>>(data);
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
