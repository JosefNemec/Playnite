using Newtonsoft.Json;
using OriginLibrary.Models;
using Playnite.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginLibrary.Services
{
    public class OriginApiClient
    {
        public static GameStoreDataResponse GetGameStoreData(string gameId)
        {
            var url = string.Format(@"https://api2.origin.com/ecommerce2/public/supercat/{0}/en_IE?country=IE", gameId);
            var stringData = Encoding.UTF8.GetString(HttpDownloader.DownloadData(url));
            return JsonConvert.DeserializeObject<GameStoreDataResponse>(stringData);
        }

        public static GameLocalDataResponse GetGameLocalData(string gameId)
        {
            var url = string.Format(@"https://api1.origin.com/ecommerce2/public/{0}/en_US", gameId);
            var stringData = Encoding.UTF8.GetString(HttpDownloader.DownloadData(url));
            return JsonConvert.DeserializeObject<GameLocalDataResponse>(stringData);
        }
    }
}
