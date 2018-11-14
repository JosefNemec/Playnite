using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TwitchLibrary.Models;

namespace TwitchLibrary.Services
{
    public class AmazonEntitlementClient
    {
        public static List<GoodsItem> GetAccountEntitlements(string accountId, string authToken)
        {
            // This looks super bad and we should idealy use HttpClient instead.
            // Problem is that Amazon server is super picky about how the request is formatted
            // and this is only working version for now.

            var content = "{\"customer\":{\"id\":\"amzn1.twitch.account_id\"},\"client\":{\"clientId\":\"Fuel\"},\"startSync\":{\"syncToken\":null,\"syncPoint\":null}}";
            content = content.Replace("account_id", accountId);
            var request = (HttpWebRequest)WebRequest.Create(@"https://adg-entitlement.amazon.com/twitch/syncGoods/");
            request.Headers.Add("TwitchOAuth", authToken);
            request.Headers.Add("X-ADG-Oauth-Headers", "TwitchOAuth");
            request.Headers.Add("Accept-Encoding", "gzip");
            request.Headers.Add("X-Amz-Target", "com.amazon.adg.entitlement.model.ADGEntitlementService.syncGoods");
            request.Headers.Add("Content-Encoding", "amz-1.0");
            request.ContentType = "application/json; charset=utf-8";
            request.UserAgent = "FuelSDK/release-1.0.0.0";
            request.Method = "POST";            
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(content);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var result = string.Empty;
            var httpResponse = (HttpWebResponse)request.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            var goods = JsonConvert.DeserializeObject<SyncGoodsResponse>(result);
            return goods.goods;                
        }
    }
}
