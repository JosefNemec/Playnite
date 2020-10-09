using Playnite.Common;
using Playnite.SDK;
using PSNLibrary.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace PSNLibrary.Services
{
    public class PSNAccountClient
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly PSNLibrary library;
        private readonly string tokenPath;
        private const int pageRequestLimit = 100;
        private const string loginUrl = @"https://my.account.sony.com/central/signin/?response_type=token&scope=capone%3Areport_submission%2Ckamaji%3Agame_list%2Ckamaji%3Aget_account_hash%2Cuser%3Aaccount.get%2Cuser%3Aaccount.profile.get%2Ckamaji%3Asocial_get_graph%2Ckamaji%3Augc%3Adistributor%2Cuser%3Aaccount.identityMapper%2Ckamaji%3Amusic_views%2Ckamaji%3Aactivity_feed_get_feed_privacy%2Ckamaji%3Aactivity_feed_get_news_feed%2Ckamaji%3Aactivity_feed_submit_feed_story%2Ckamaji%3Aactivity_feed_internal_feed_submit_story%2Ckamaji%3Aaccount_link_token_web%2Ckamaji%3Augc%3Adistributor_web%2Ckamaji%3Aurl_preview&client_id=656ace0b-d627-47e6-915c-13b259cd06b2&redirect_uri=https%3A%2F%2Fmy.playstation.com%2Fauth%2Fresponse.html%3FrequestID%3Dexternal_request_a90959ab-afa3-4594-824b-ad00b6617f57%26baseUrl%3D%2F%26returnRoute%3D%2F%26targetOrigin%3Dhttps%3A%2F%2Fmy.playstation.com%26excludeQueryParams%3Dtrue&tp_console=true&ui=pr&cid=b7895274-2de2-45da-add9-3afd775eb65f&error=login_required&error_code=4165&no_captcha=true#/signin/ca?entry=ca";
        private const string loginTokenUrl = @"https://ca.account.sony.com/api/v1/oauth/authorize?response_type=token&scope=capone:report_submission,kamaji:game_list,kamaji:get_account_hash,user:account.get,user:account.profile.get,kamaji:social_get_graph,kamaji:ugc:distributor,user:account.identityMapper,kamaji:music_views,kamaji:activity_feed_get_feed_privacy,kamaji:activity_feed_get_news_feed,kamaji:activity_feed_submit_feed_story,kamaji:activity_feed_internal_feed_submit_story,kamaji:account_link_token_web,kamaji:ugc:distributor_web,kamaji:url_preview&client_id=656ace0b-d627-47e6-915c-13b259cd06b2&redirect_uri=https://my.playstation.com/auth/response.html?requestID=iframe_request_ecd7cd01-27ad-4851-9c0d-0798c1a65e53&baseUrl=/&targetOrigin=https://my.playstation.com&prompt=none";
        private const string tokenUrl = @"https://ca.account.sony.com/api/v1/oauth/authorize?response_type=token&scope=capone:report_submission,kamaji:game_list,kamaji:get_account_hash,user:account.get,user:account.profile.get,kamaji:social_get_graph,kamaji:ugc:distributor,user:account.identityMapper,kamaji:music_views,kamaji:activity_feed_get_feed_privacy,kamaji:activity_feed_get_news_feed,kamaji:activity_feed_submit_feed_story,kamaji:activity_feed_internal_feed_submit_story,kamaji:account_link_token_web,kamaji:ugc:distributor_web,kamaji:url_preview&client_id=656ace0b-d627-47e6-915c-13b259cd06b2&redirect_uri=https://my.playstation.com/auth/response.html?requestID=iframe_request_b0f09e04-8206-49be-8be6-b2cfe05249e2&baseUrl=/&targetOrigin=https://my.playstation.com&prompt=none";
        private const string gameListUrl = @"https://gamelist.api.playstation.com/v1/users/me/titles?type=owned,played&app=richProfile&sort=-lastPlayedDate&iw=240&ih=240&fields=@default&limit={0}&offset={1}&npLanguage=en";
        private const string trophiesUrl = @"https://us-tpy.np.community.playstation.net/trophy/v1/trophyTitles?fields=@default,trophyTitleSmallIconUrl&platform=PS3,PS4,PSVITA&limit={0}&offset={1}&npLanguage=en";
        private const string profileUrl = @"https://us-prof.np.community.playstation.net/userProfile/v1/users/me/profile2";
        private const string downloadListUrl = @"https://store.playstation.com/en/download/list";
        private const string profileLandingUrl = @"https://my.playstation.com/whatsnew";

        public PSNAccountClient(PSNLibrary library)
        {
            this.library = library;
            tokenPath = Path.Combine(library.GetPluginUserDataPath(), "token.json");
        }

        public void Login()
        {
            using (var webView = library.PlayniteApi.WebViews.CreateView(540, 670))
            {
                var callbackUrl = string.Empty;
                webView.LoadingChanged += (_, __) =>
                {
                    var address = webView.GetCurrentAddress();
                    if (address == profileLandingUrl)
                    {
                        webView.Navigate(tokenUrl);
                    }
                    else if (address.Contains("access_token="))
                    {
                        callbackUrl = address;
                        webView.Close();
                    }
                    else if (address.EndsWith("/friends"))
                    {
                        webView.Navigate(loginTokenUrl);
                    }
                };

                if (File.Exists(tokenPath))
                {
                    File.Delete(tokenPath);
                }

                webView.DeleteDomainCookies(".playstation.com");
                webView.DeleteDomainCookies(".sonyentertainmentnetwork.com");
                webView.DeleteDomainCookies(".sony.com");
                webView.Navigate(loginUrl);
                webView.OpenDialog();

                if (!callbackUrl.IsNullOrEmpty())
                {
                    var rediUri = new Uri(callbackUrl);
                    var fragments = HttpUtility.ParseQueryString(rediUri.Fragment);
                    var token = fragments["#access_token"];
                    FileSystem.WriteStringToFile(tokenPath, token);
                }
            }
        }

        private async Task CheckAuthentication()
        {
            if (!File.Exists(tokenPath))
            {
                throw new Exception("User is not authenticated.");
            }
            else
            {
                if (!await GetIsUserLoggedIn())
                {
                    throw new Exception("User is not authenticated.");
                }
            }
        }

        public async Task<List<DownloadListEntitlement>> GetDownloadList()
        {
            await CheckAuthentication();

            using (var webView = library.PlayniteApi.WebViews.CreateOffscreenView())
            {
                var loadComplete = new AutoResetEvent(false);
                var items = new List<DownloadListEntitlement>();
                var processingDownload = false;

                webView.LoadingChanged += async (_, e) =>
                {
                    var address = webView.GetCurrentAddress();
                    if (address?.EndsWith("download/list") == true && !e.IsLoading)
                    {
                        if (processingDownload)
                        {
                            return;
                        }

                        processingDownload = true;
                        var numberOfTries = 0;
                        while (numberOfTries < 6)
                        {
                            // Don't know how to reliable tell if the data are ready because they are laoded post page load
                            await Task.Delay(10000);
                            if (!webView.CanExecuteJavascriptInMainFrame)
                            {
                                logger.Warn("PSN JS execution not ready yet.");
                                continue;
                            }

                            // Need to use this hack since the data we need are stored in browser's local storage
                            // Based on https://github.com/RePod/psdle/blob/master/psdle.js
                            var res = await webView.EvaluateScriptAsync(@"JSON.stringify(Ember.Application.NAMESPACES_BY_ID['valkyrie-storefront'].__container__.lookup('service:macross-brain').macrossBrainInstance.getEntitlementStore().getAllEntitlements()._result)");
                            var strRes = (string)res.Result;
                            if (strRes.IsNullOrEmpty())
                            {
                                numberOfTries++;
                                continue;
                            }

                            try
                            {
                                items = Serialization.FromJson<List<DownloadListEntitlement>>(strRes);
                            }
                            catch (Exception exc)
                            {
                                logger.Error(exc, "Failed to deserialize PSN's download list.");
                                logger.Debug(strRes);
                            }

                            loadComplete.Set();
                            break;
                        }
                    }
                };

                webView.Navigate(downloadListUrl);
                loadComplete.WaitOne(60000);
                return items;
            }
        }

        private async Task<T> SendPageRequest<T>(HttpClient client, string url, int offset) where T : class
        {
            var strResponse = await client.GetStringAsync(url.Format(pageRequestLimit, offset));
            return Serialization.FromJson<T>(strResponse);
        }

        public async Task<List<TrophyTitles.TrophyTitle>> GetThropyTitles()
        {
            await CheckAuthentication();
            var token = GetStoredToken();
            var titles = new List<TrophyTitles.TrophyTitle>();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                var itemCount = 0;
                var offset = -pageRequestLimit;

                do
                {
                    var response = await SendPageRequest<TrophyTitles>(client, trophiesUrl,  offset + pageRequestLimit);
                    itemCount = response.totalResults;
                    offset = response.offset;
                    titles.AddRange(response.trophyTitles);
                }
                while (offset + pageRequestLimit < itemCount);
            }

            return titles;
        }

        public async Task<List<AccountTitles.Title>> GetAccountTitles()
        {
            await CheckAuthentication();
            var token = GetStoredToken();
            var titles = new List<AccountTitles.Title>();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                var itemCount = 0;
                var offset = -pageRequestLimit;

                do
                {
                    var response = await SendPageRequest<AccountTitles>(client, gameListUrl, offset + pageRequestLimit);
                    itemCount = response.totalResults;
                    offset = response.start;
                    titles.AddRange(response.titles);
                }
                while (offset + pageRequestLimit < itemCount);
            }

            return titles;
        }

        private string GetStoredToken()
        {
            var token = string.Empty;
            if (File.Exists(tokenPath))
            {
                token = File.ReadAllText(tokenPath);
            }

            return token;
        }

        private string RefreshToken()
        {
            logger.Debug("Trying to refresh PSN token.");
            if (File.Exists(tokenPath))
            {
                File.Delete(tokenPath);
            }

            var callbackUrl = string.Empty;
            using (var webView = library.PlayniteApi.WebViews.CreateOffscreenView())
            {
                webView.LoadingChanged += (_, __) =>
                {
                    var address = webView.GetCurrentAddress();
                    if (address.Contains("access_token="))
                    {
                        callbackUrl = address;
                    }
                };

                webView.NavigateAndWait(profileLandingUrl);
                webView.NavigateAndWait(tokenUrl);

                if (!callbackUrl.IsNullOrEmpty())
                {
                    var rediUri = new Uri(callbackUrl);
                    var fragments = HttpUtility.ParseQueryString(rediUri.Fragment);
                    var token = fragments["#access_token"];
                    FileSystem.WriteStringToFile(tokenPath, token);
                    return token;
                }
            }

            return string.Empty;
        }

        public async Task<bool> GetIsUserLoggedIn()
        {
            if (!File.Exists(tokenPath))
            {
                return false;
            }

            try
            {
                var token = GetStoredToken();
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                    var response = await client.GetAsync(profileUrl);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return true;
                    }
                    else
                    {
                        token = RefreshToken();
                        if (token.IsNullOrEmpty())
                        {
                            return false;
                        }

                        client.DefaultRequestHeaders.Remove("Authorization");
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                        response = await client.GetAsync(profileUrl);
                        return response.StatusCode == System.Net.HttpStatusCode.OK;
                    }
                }
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                logger.Error(e, "Failed to check if user is authenticated into PSN.");
                return false;
            }
        }
    }
}
