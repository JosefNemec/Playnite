using Newtonsoft.Json;
using OriginLibrary.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OriginLibrary.Services
{
    public class OriginAccountClient
    {
        private const string loginUrl = @"https://accounts.ea.com/connect/auth?response_type=code&client_id=ORIGIN_SPA_ID&display=originXWeb/login&locale=en_US&redirect_uri=https://www.origin.com/views/login.html";
        private const string logoutUrl = @"https://accounts.ea.com/connect/logout?client_id=ORIGIN_SPA_ID&redirect_uri=https://www.origin.com/views/social.html";
        private const string tokenUrl = @"https://accounts.ea.com/connect/auth?client_id=ORIGIN_JS_SDK&response_type=token&redirect_uri=nucleus:rest&prompt=none";
        private ILogger logger = LogManager.GetLogger();
        private IWebView webView;

        public OriginAccountClient(IWebView webView)
        {
            this.webView = webView;
        }

        public List<AccountEntitlementsResponse.Entitlement> GetOwnedGames(long userId, AuthTokenResponse token)
        {
            var client = new WebClient();
            client.Headers.Add("authtoken", token.access_token);
            client.Headers.Add("accept", "application/vnd.origin.v3+json; x-cache/force-write");

            var stringData = client.DownloadString(string.Format(@"https://api1.origin.com/ecommerce2/consolidatedentitlements/{0}?machine_hash=1", userId));
            var data = JsonConvert.DeserializeObject<AccountEntitlementsResponse>(stringData);
            return data.entitlements;
        }

        public AccountInfoResponse GetAccountInfo(AuthTokenResponse token)
        {
            var client = new WebClient();
            client.Headers.Add("Authorization", token.token_type + " " + token.access_token);
            var stringData = client.DownloadString(@"https://gateway.ea.com/proxy/identity/pids/me");
            return JsonConvert.DeserializeObject<AccountInfoResponse>(stringData);
        }

        public UsageResponse GetUsage(long userId, string gameId, AuthTokenResponse token)
        {
            var gameStoreData = OriginApiClient.GetGameStoreData(gameId);
            string multiplayerId = gameStoreData.platforms.First(a => a.platform == "PCWIN").multiplayerId;
            string masterTitleId = gameStoreData.masterTitleId;

            var client = new WebClient();
            client.Headers.Add("authtoken", token.access_token);
            client.Headers.Add("X-Origin-Platform", "PCWIN");
            if (!string.IsNullOrEmpty(multiplayerId))
            {
                client.Headers.Add("MultiplayerId", multiplayerId);
            }

            var stringData = client.DownloadString(string.Format(@"https://api1.origin.com/atom/users/{0}/games/{1}/usage", userId, masterTitleId));
            return new UsageResponse(stringData);
        }

        public AuthTokenResponse GetAccessToken()
        {
            webView.NavigateAndWait(tokenUrl);
            var stringInfo = webView.GetPageText();
            var tokenData = JsonConvert.DeserializeObject<AuthTokenResponse>(stringInfo);
            return tokenData;
        }

        public void Login()
        {
            webView.LoadingChanged += (s, e) =>
            {
                if (webView.GetCurrentAddress().StartsWith(@"https://www.origin.com/views/login"))
                {
                    webView.Close();
                }

                if (webView.GetCurrentAddress().StartsWith(@"https://www.origin.com/views/social"))
                {
                    webView.Navigate(loginUrl);
                }
            };

            webView.Navigate(logoutUrl);
            webView.OpenDialog();
        }

        public bool GetIsUserLoggedIn()
        {
            var token = GetAccessToken();
            return string.IsNullOrEmpty(token?.error);
        }
    }
}
