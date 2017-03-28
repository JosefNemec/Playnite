using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CefSharp;
using CefSharp.Wpf;
using System.Threading;
using System.Text.RegularExpressions;
using System.Windows;
using Playnite.Controls;

namespace Playnite.Providers.Origin
{
    public class WebApiClient
    {
        private CefSharp.OffScreen.ChromiumWebBrowser browser;

        public WebApiClient()
        {
            browser = new CefSharp.OffScreen.ChromiumWebBrowser(automaticallyCreateBrowser: false);
            browser.BrowserInitialized += Browser_BrowserInitialized;
            browser.CreateBrowser(IntPtr.Zero);
        }

        private AutoResetEvent browserInitializedEvent = new AutoResetEvent(false);
        private void Browser_BrowserInitialized(object sender, EventArgs e)
        {
            browserInitializedEvent.Set();
        }

        public static GameStoreDataResponse GetGameStoreData(string gameId)
        {
            var url = string.Format(@"https://api2.origin.com/ecommerce2/public/supercat/{0}/en_IE?country=IE", gameId);
            var stringData = Encoding.UTF8.GetString(Web.DownloadData(url));
            return JsonConvert.DeserializeObject<GameStoreDataResponse>(stringData);
        }

        public static GameLocalDataResponse GetGameLocalData(string gameId)
        {
            var url = string.Format(@"https://api1.origin.com/ecommerce2/public/{0}/en_US", gameId);
            var stringData = Encoding.UTF8.GetString(Web.DownloadData(url));
            return JsonConvert.DeserializeObject<GameLocalDataResponse>(stringData);
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

        #region GetLoginRequired
        private async void getLoginRequired_StateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading == false)
            {
                var b = (CefSharp.OffScreen.ChromiumWebBrowser)sender;
                var stringData = await b.GetTextAsync();
                var data = JsonConvert.DeserializeObject<AuthTokenResponse>(stringData);
                loginRequired = !string.IsNullOrEmpty(data.error);
                loginRequiredEvent.Set();
            }
        }

        private bool loginRequired = true;
        private AutoResetEvent loginRequiredEvent = new AutoResetEvent(false);
        public bool GetLoginRequired()
        {
            if (!browser.IsBrowserInitialized)
            {
                browserInitializedEvent.WaitOne(5000);
            }

            try
            {
                browser.LoadingStateChanged += getLoginRequired_StateChanged;
                browser.Load(@"https://accounts.ea.com/connect/auth?client_id=ORIGIN_JS_SDK&response_type=token&redirect_uri=nucleus:rest&prompt=none");
                loginRequiredEvent.WaitOne(10000);
                return loginRequired;
            }
            finally
            {
                browser.LoadingStateChanged -= getLoginRequired_StateChanged;
            }
        }

        #endregion GetLoginRequired

        #region GetAccessToken
        private async void getAccessToken_StateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading == false)
            {
                var b = (CefSharp.OffScreen.ChromiumWebBrowser)sender;
                accessToken = await b.GetTextAsync();
                tokenGotEvent.Set();
            }
        }

        private string accessToken = string.Empty;
        private AutoResetEvent tokenGotEvent = new AutoResetEvent(false);
        public AuthTokenResponse GetAccessToken()
        {
            if (!browser.IsBrowserInitialized)
            {
                browserInitializedEvent.WaitOne(5000);
            }

            try
            {
                accessToken = string.Empty;
                browser.LoadingStateChanged += getAccessToken_StateChanged;
                browser.Load(@"https://accounts.ea.com/connect/auth?client_id=ORIGIN_JS_SDK&response_type=token&redirect_uri=nucleus:rest&prompt=none");
                tokenGotEvent.WaitOne(10000);

                var tokenData = JsonConvert.DeserializeObject<AuthTokenResponse>(accessToken);
                return tokenData;
            }
            finally
            {
                browser.LoadingStateChanged -= getAccessToken_StateChanged;
            }
        }
        #endregion GetAccessToken

        #region Login
        private void login_StateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading == false)
            {
                var b = (ChromiumWebBrowser)sender;

                b.Dispatcher.Invoke(() =>
                {
                    if (b.Address.StartsWith(@"https://www.origin.com/views/social"))
                    {
                        loginWindow.Dispatcher.Invoke(() =>
                        {
                            loginWindow.Browser.Address = loginUrl;
                        });
                    }

                    if (b.Address.StartsWith(@"https://www.origin.com/views/login"))
                    {
                        loginWindow.Dispatcher.Invoke(() =>
                        {
                            loginSuccess = true;
                            loginWindow.Close();
                        });
                    }
                    else
                    {
                        loginSuccess = false;
                    }
                });
            }
        }

        private bool loginSuccess = false;
        private string loginUrl = @"https://accounts.ea.com/connect/auth?response_type=code&client_id=ORIGIN_SPA_ID&display=originXWeb/login&locale=en_US&redirect_uri=https://www.origin.com/views/login.html";
        private string logoutUrl = @"https://accounts.ea.com/connect/logout?client_id=ORIGIN_SPA_ID&redirect_uri=https://www.origin.com/views/social.html";

        LoginWindow loginWindow;
        public bool Login(Window parent = null)
        {
            loginSuccess = false;
            loginWindow = new LoginWindow()
            {
                Height = 670,
                Width = 490
            };
            loginWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            loginWindow.Browser.LoadingStateChanged += login_StateChanged;
            loginWindow.Owner = parent;
            loginWindow.Browser.Address = logoutUrl;
            loginWindow.ShowDialog();
            return loginSuccess;
        }
        #endregion Login
    }
}
