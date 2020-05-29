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
            string OriginLang = OriginLibrary._OriginLang;
            string OriginLangCountry = OriginLang.Substring((OriginLang.Length - 2));
            var url = string.Format(@"https://api2.origin.com/ecommerce2/public/supercat/{0}/{1}?country={2}", 
                gameId, OriginLang, OriginLangCountry);
            var stringData = Encoding.UTF8.GetString(HttpDownloader.DownloadData(url));
            return JsonConvert.DeserializeObject<GameStoreDataResponse>(stringData);
        }

        public static StorePageMetadata GetStoreMetadata(string offerPath)
        {
            // Remove edition from offer path, offer path is: /<franchise>/<game>/<edition>
            var match = Regex.Match(offerPath, @"(\/(.+?)\/(.+?))\/");
            var offer = match.Groups[1].Value.ToString();
            string OriginLang = OriginLibrary._OriginLang.ToLower().Replace("_", "-");
            var url = string.Format(@"https://data3.origin.com/ocd{0}.{1}.irl.ocd", offer, OriginLang);
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
                string OriginLang = OriginLibrary._OriginLang;
                var url = string.Format(@"https://api1.origin.com/ecommerce2/public/{0}/{1}", gameId, OriginLang);
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
