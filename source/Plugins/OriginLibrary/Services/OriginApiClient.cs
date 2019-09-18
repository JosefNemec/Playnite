using Newtonsoft.Json;
using OriginLibrary.Models;
using Playnite.Common.Web;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OriginLibrary.Services
{
    public class OriginApiClient
    {
        public static ILogger logger = LogManager.GetLogger();

        public static GameStoreDataResponse GetGameStoreData(string gameId)
        {
            var url = string.Format(@"https://api2.origin.com/ecommerce2/public/supercat/{0}/en_IE?country=IE", gameId);
            var stringData = Encoding.UTF8.GetString(HttpDownloader.DownloadData(url));
            return JsonConvert.DeserializeObject<GameStoreDataResponse>(stringData);
        }

        public static StorePageMetadata GetStoreMetadata(string offerPath)
        {
            // Remove edition from offer path, offer path is: /<franchise>/<game>/<edition>
            var match = Regex.Match(offerPath, @"(\/(.+?)\/(.+?))\/");
            var offer = match.Groups[1].Value.ToString();
            var url = string.Format(@"https://data3.origin.com/ocd{0}.en-us.irl.ocd", offer);
            if (HttpDownloader.GetResponseCode(url) == System.Net.HttpStatusCode.OK)
            {
                var stringData = Encoding.UTF8.GetString(HttpDownloader.DownloadData(url));
                return JsonConvert.DeserializeObject<StorePageMetadata>(stringData);
            }
            else
            {
                return null;
            }
        }

        public static GameLocalDataResponse GetGameLocalData(string gameId)
        {
            try
            {
                var url = string.Format(@"https://api1.origin.com/ecommerce2/public/{0}/en_US", gameId);
                var stringData = Encoding.UTF8.GetString(HttpDownloader.DownloadData(url));
                return JsonConvert.DeserializeObject<GameLocalDataResponse>(stringData);
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to get local game Origin manifest.");
                return null;
            }
        }
    }
}
