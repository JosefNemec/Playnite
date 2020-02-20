using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using XboxLibrary.Models;

namespace XboxLibrary.Services
{
    public class XboxAccountClient
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private XboxLibrary library;
        private const string loginUrl = @"https://login.live.com/oauth20_authorize.srf?display=touch&scope=service%3A%3Auser.auth.xboxlive.com%3A%3AMBI_SSL&redirect_uri=https%3A%2F%2Flogin.live.com%2Foauth20_desktop.srf&locale=en&response_type=token&client_id=0000000048093EE3";

        private readonly string liveTokensPath;
        private readonly string xstsLoginTokesPath;

        public XboxAccountClient(XboxLibrary library)
        {
            this.library = library;
            liveTokensPath = Path.Combine(library.GetPluginUserDataPath(), "login.json");
            xstsLoginTokesPath = Path.Combine(library.GetPluginUserDataPath(), "xsts.json");
        }

        public async Task Login()
        {
            var callbackUrl = string.Empty;
            using (var webView = library.PlayniteApi.WebViews.CreateView(490, 560))
            {
                webView.NavigationChanged += (s, e) =>
                {
                    var url = webView.GetCurrentAddress();
                    if (url.Contains("access_token="))
                    {
                        callbackUrl = url;
                        webView.Close();
                    }
                };

                if (File.Exists(liveTokensPath))
                {
                    File.Delete(liveTokensPath);
                }

                if (File.Exists(xstsLoginTokesPath))
                {
                    File.Delete(xstsLoginTokesPath);
                }

                webView.DeleteDomainCookies(".live.com");
                webView.DeleteDomainCookies(".xboxlive.com");
                webView.DeleteDomainCookies(".xbox.com");
                webView.DeleteDomainCookies(".microsoft.com");
                webView.DeleteDomainCookies(".login.live.com");
                webView.Navigate(loginUrl);
                webView.OpenDialog();
            }

            if (!callbackUrl.IsNullOrEmpty())
            {
                var rediUri = new Uri(callbackUrl);
                var fragments = HttpUtility.ParseQueryString(rediUri.Fragment);
                var liveLoginData = new AuthenticationData
                {
                    AccessToken = fragments["#access_token"],
                    RefreshToken = fragments["refresh_token"],
                    ExpiresIn = fragments["expires_in"],
                    CreationDate = DateTime.Now,
                    TokenType = fragments["token_type"],
                    UserId = fragments["user_id"]
                };

                FileSystem.WriteStringToFile(liveTokensPath, Serialization.ToJson(liveLoginData));
                await Authenticate(liveLoginData.AccessToken);
            }
        }

        public async Task<bool> GetIsUserLoggedIn()
        {
            try
            {
                if (!File.Exists(xstsLoginTokesPath))
                {
                    return false;
                }

                var tokens = Serialization.FromJsonFile<AuthorizationData>(xstsLoginTokesPath);
                using (var client = new HttpClient())
                {
                    SetAuthenticationHeaders(client.DefaultRequestHeaders, tokens);
                    var requestData = new ProfileRequest()
                    {
                        settings = new List<string> { "GameDisplayName" },
                        userIds = new List<ulong> { ulong.Parse(tokens.DisplayClaims.xui[0].xid) }
                    };

                    var response = await client.PostAsync(
                        @"https://profile.xboxlive.com/users/batch/profile/settings",
                        new StringContent(Serialization.ToJson(requestData), Encoding.UTF8, "application/json"));
                    return response.StatusCode == System.Net.HttpStatusCode.OK;
                }
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                logger.Error(e, "Failed to check Xbox user loging status.");
                return false;
            }
        }

        private async Task Authenticate(string accessToken)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("x-xbl-contract-version", "1");

                //  Authenticate
                var authRequestData = new AthenticationRequest();
                authRequestData.Properties.RpsTicket = accessToken;
                var authPostContent = Serialization.ToJson(authRequestData, true);

                var authResponse = await client.PostAsync(
                    @"https://user.auth.xboxlive.com/user/authenticate",
                    new StringContent(authPostContent, Encoding.UTF8, "application/json"));
                var authResponseContent = await authResponse.Content.ReadAsStringAsync();
                var authTokens = Serialization.FromJson<AuthorizationData>(authResponseContent);

                // Authorize
                var atrzRequrestData = new AuhtorizationRequest();
                atrzRequrestData.Properties.UserTokens = new List<string> { authTokens.Token };
                var atrzPostContent = Serialization.ToJson(atrzRequrestData, true);

                var atrzResponse = await client.PostAsync(
                    @"https://xsts.auth.xboxlive.com/xsts/authorize",
                    new StringContent(atrzPostContent, Encoding.UTF8, "application/json"));
                var atrzResponseContent = await atrzResponse.Content.ReadAsStringAsync();
                var atrzTokens = Serialization.FromJson<AuthorizationData>(atrzResponseContent);

                FileSystem.WriteStringToFile(xstsLoginTokesPath, atrzResponseContent);
            }
        }

        internal async Task RefreshTokens()
        {
            logger.Debug("Refreshing xbox tokens.");
            var tokens = Serialization.FromJsonFile<AuthenticationData>(liveTokensPath);

            var query = HttpUtility.ParseQueryString(string.Empty);
            query.Add("grant_type", "refresh_token");
            query.Add("client_id", "0000000048093EE3");
            query.Add("scope", "service::user.auth.xboxlive.com::MBI_SSL");
            query.Add("refresh_token", tokens.RefreshToken);

            var refreshUrl = @"https://login.live.com/oauth20_token.srf?" + query.ToString();
            using (var client = new HttpClient())
            {
                var refreshResponse = await client.GetAsync(refreshUrl);
                if (refreshResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseContent = await refreshResponse.Content.ReadAsStringAsync();
                    var response = Serialization.FromJson<RefreshTokenResponse>(responseContent);
                    tokens.AccessToken = response.access_token;
                    tokens.RefreshToken = response.refresh_token;
                    FileSystem.WriteStringToFile(liveTokensPath, Serialization.ToJson(tokens));
                    await Authenticate(tokens.AccessToken);
                }
            }
        }

        public async Task<List<TitleHistoryResponse.Title>> GetLibraryTitles()
        {
            if (!File.Exists(xstsLoginTokesPath))
            {
                throw new Exception("User is not authenticated.");
            }
            else
            {
                var loggedIn = await GetIsUserLoggedIn();
                if (!loggedIn && File.Exists(liveTokensPath))
                {
                    await RefreshTokens();
                }

                if (!await GetIsUserLoggedIn())
                {
                    throw new Exception("User is not authenticated.");
                }
            }

            var tokens = Serialization.FromJsonFile<AuthorizationData>(xstsLoginTokesPath);
            using (var client = new HttpClient())
            {
                SetAuthenticationHeaders(client.DefaultRequestHeaders, tokens);
                var response = await client.GetAsync(
                    string.Format(@"https://titlehub.xboxlive.com/users/xuid({0})/titles/titlehistory/decoration/{1}",
                    tokens.DisplayClaims.xui[0].xid, "detail"));
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception("User is not authenticated.");
                }

                var cont = await response.Content.ReadAsStringAsync();
                var titleHistory = Serialization.FromJson<TitleHistoryResponse>(cont);
                return titleHistory.titles;
            }
        }

        public async Task<TitleHistoryResponse.Title> GetTitleInfo(string pfn)
        {
            var tokens = Serialization.FromJsonFile<AuthorizationData>(xstsLoginTokesPath);
            using (var client = new HttpClient())
            {
                SetAuthenticationHeaders(client.DefaultRequestHeaders, tokens);
                var requestData = new Dictionary<string, List<string>>
                {
                    { "pfns", new List<string> { pfn } },
                    { "windowsPhoneProductIds", new List<string>() },
                };

                var response = await client.PostAsync(
                           @"https://titlehub.xboxlive.com/titles/batch/decoration/detail",
                           new StringContent(Serialization.ToJson(requestData), Encoding.UTF8, "application/json"));

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception("User is not authenticated.");
                }

                var cont = await response.Content.ReadAsStringAsync();
                var titleHistory = Serialization.FromJson<TitleHistoryResponse>(cont);
                return titleHistory.titles.First();
            }
        }

        private void SetAuthenticationHeaders(
            System.Net.Http.Headers.HttpRequestHeaders headers,
            AuthorizationData auth)
        {
            headers.Add("x-xbl-contract-version", "2");
            headers.Add("Authorization", $"XBL3.0 x={auth.DisplayClaims.xui[0].uhs};{auth.Token}");
            headers.Add("Accept-Language", "en-US");
        }
    }
}
