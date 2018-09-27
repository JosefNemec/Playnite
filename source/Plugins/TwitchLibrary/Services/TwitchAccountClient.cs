using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwitchLibrary.Models;

namespace TwitchLibrary.Services
{
    // TODO: Rework to use HttpClient, the same way as AmazonEntitlementClient
    public class TwitchAccountClient
    {        
        private IWebView webView;
        private string tokensPath;
        private const string userAgent = @"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) twitch-electron/0.1.0 Chrome/59.0.3071.115 Twitch/1.8.4 Safari/537.36";

        public TwitchAccountClient(IWebView webView, string tokensPath)
        {
            this.webView = webView;
            this.tokensPath = tokensPath;
        }

        public void RenewTokens(string authToken, string accountId)
        {
            var renewWebRequest = (HttpWebRequest)WebRequest.Create(@"https://logins-v1.curseapp.net/login/renew");
            renewWebRequest.Headers.Add("AuthenticationToken", authToken);
            renewWebRequest.Accept = "application/json";
            renewWebRequest.UserAgent = userAgent;
            renewWebRequest.Method = "POST";

            var renewResult = string.Empty;
            var renewWebReponse = (HttpWebResponse)renewWebRequest.GetResponse();
            using (var streamReader = new StreamReader(renewWebReponse.GetResponseStream()))
            {
                renewResult = streamReader.ReadToEnd();
            }

            var renewResponse = JsonConvert.DeserializeObject<TwitchOauthSession>(renewResult);
            var authResult = Authenticate(renewResponse.Token);
            SaveTokens(
                authResult.AccessToken,
                authResult.RefreshToken,
                accountId,
                renewResponse.Token,
                renewResponse.RenewAfter,
                renewResponse.Expires);
        }

        internal FuelLoginResponse Authenticate(string authToken)
        {
            var fuelWebRequest = (HttpWebRequest)WebRequest.Create(@"https://logins-v1.curseapp.net/login/fuel");
            fuelWebRequest.Headers.Add("AuthenticationToken", authToken);
            fuelWebRequest.Accept = "application/json";
            fuelWebRequest.Method = "POST";

            var fuelResult = string.Empty;
            var fuelWebResponse = (HttpWebResponse)fuelWebRequest.GetResponse();
            using (var streamReader = new StreamReader(fuelWebResponse.GetResponseStream()))
            {
                fuelResult = streamReader.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<FuelLoginResponse>(fuelResult);
        }

        internal void SaveTokens(
            string accessToken,
            string refreshToken,
            string accountId,
            string authToken,
            long renewAfter,
            long expires)
        {
            var loginData = new TwitchLoginData()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccountId = accountId,
                AuthenticationToken = authToken,
                RenewAfter = DateTimeOffset.FromUnixTimeMilliseconds(renewAfter).DateTime,
                Expires = DateTimeOffset.FromUnixTimeMilliseconds(expires).DateTime
            };

            File.WriteAllText(tokensPath, JsonConvert.SerializeObject(loginData, Formatting.Indented));
        }

        public void Login()
        {
            var twitchAuthToken = string.Empty;
            webView.NavigationChanged += (s, e) =>
            {
                var address = webView.GetCurrentAddress();
                var match = Regex.Match(address, @"\?code\=(\w+?)\&");
                if (match.Success)
                {
                    twitchAuthToken = match.Groups[1].Value;
                    webView.Close();
                }

            };

            webView.Navigate(@"https://api.twitch.tv/kraken/oauth2/authorize?client_id=jf3xu125ejjjt5cl4osdjci6oz6p93r&curse_embed=false&embed=false&force_login=true&lang=en&login_type=login&redirect_uri=https%3A%2F%2Fweb.curseapp.net%2Flaguna%2Fpassport-callback.html&response_type=code&scope=chat_login%20user_read%20user_subscriptions&state=f51bfcf2845dc42db1dd6d1dfc3c3bf2");
            webView.OpenDialog();
            if (string.IsNullOrEmpty(twitchAuthToken))
            {
                return;
            }

            var oauthRequest = new TwitchOauthRequest()
            {
                ClientID = "jf3xu125ejjjt5cl4osdjci6oz6p93r",
                Code = twitchAuthToken,
                RedirectUri = "https://web.curseapp.net/laguna/passport-callback.html",
                State = "f51bfcf2845dc42db1dd6d1dfc3c3bf2"
            };

            var oauthWebRequuest = (HttpWebRequest)WebRequest.Create(@"https://logins-v1.curseapp.net/login/twitch-oauth");
            oauthWebRequuest.Headers.Add("Accept-Encoding", "gzip, deflate");
            oauthWebRequuest.ContentType = "application/json; charset=UTF-8";
            oauthWebRequuest.UserAgent = userAgent;
            oauthWebRequuest.Method = "POST";
            oauthWebRequuest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            using (var streamWriter = new StreamWriter(oauthWebRequuest.GetRequestStream()))
            {
                streamWriter.Write(JsonConvert.SerializeObject(oauthRequest));
                streamWriter.Flush();
                streamWriter.Close();
            }

            var oauthResult = string.Empty;
            var oauthWebResponse = (HttpWebResponse)oauthWebRequuest.GetResponse();
            using (var streamReader = new StreamReader(oauthWebResponse.GetResponseStream()))
            {
                oauthResult = streamReader.ReadToEnd();
            }

            var oauthResponse = JsonConvert.DeserializeObject<TwitchOauthResponse>(oauthResult);
            var fuelResponse = Authenticate(oauthResponse.Session.Token);
            SaveTokens(
                fuelResponse.AccessToken,
                fuelResponse.RefreshToken,
                oauthResponse.TwitchUserID,
                oauthResponse.Session.Token,
                oauthResponse.Session.RenewAfter,
                oauthResponse.Session.Expires);
        }
    }
}
