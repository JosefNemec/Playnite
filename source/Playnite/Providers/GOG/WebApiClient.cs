using CefSharp;
using CefSharp.Wpf;
using Newtonsoft.Json;
using NLog;
using Playnite.Controls;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;

namespace Playnite.Providers.GOG
{
    public class WebApiClient : IDisposable
    {
        private static string localeString = "US_USD_en-US";
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private CefSharp.OffScreen.ChromiumWebBrowser browser;

        public WebApiClient()
        {
            browser = new CefSharp.OffScreen.ChromiumWebBrowser(automaticallyCreateBrowser:false);
            browser.BrowserInitialized += Browser_BrowserInitialized;
            browser.CreateBrowser(IntPtr.Zero);
        }

        public void Dispose()
        {
            browser?.Dispose();
        }

        private AutoResetEvent browserInitializedEvent = new AutoResetEvent(false);
        private void Browser_BrowserInitialized(object sender, EventArgs e)
        {
            browserInitializedEvent.Set();
        }

        public static StorePageResult.ProductDetails GetGameStoreData(string gameUrl)
        {
            string[] data;

            try
            {
                data = Web.DownloadString(gameUrl, new List<System.Net.Cookie>() { new System.Net.Cookie("gog_lc", localeString) }).Split('\n');
            }
            catch (WebException e)
            {
                var response = (HttpWebResponse)e.Response;
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }

            foreach (var line in data)
            {
                var trimmed = line.TrimStart();
                if (line.TrimStart().StartsWith("var gogData"))
                {
                    var stringData = trimmed.Substring(14).TrimEnd(';');
                    var desData = JsonConvert.DeserializeObject<StorePageResult>(stringData);

                    if (desData.gameProductData == null)
                    {
                        return null;
                    }

                    return desData.gameProductData;
                }
            }

            throw new Exception("Failed to get store data from page, no data found. " + gameUrl);
        }

        #region GetGameDetails

        public static ProductApiDetail GetGameDetails(string id)
        {
            var baseUrl = @"http://api.gog.com/products/{0}?expand=description";

            try
            {
                var stringData = Web.DownloadString(string.Format(baseUrl, id), new List<System.Net.Cookie>() { new System.Net.Cookie("gog_lc", localeString) });
                return JsonConvert.DeserializeObject<ProductApiDetail>(stringData);
            }
            catch (WebException exc)
            {
                logger.Warn(exc, "Failed to download GOG game details for " + id);
                return null;
            }
        }

        #endregion GetGameDetails

        #region ForceLanguage
        private void forceLanguage_StateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading == false)
            {
                forceLanguageEvent.Set();
            }
        }

        private AutoResetEvent forceLanguageEvent = new AutoResetEvent(false);
        public void ForceLanguage(string languageCode)
        {
            if (!browser.IsBrowserInitialized)
            {
                browserInitializedEvent.WaitOne(5000);
            }

            try
            {
                browser.LoadingStateChanged += forceLanguage_StateChanged;
                browser.Load(@"https://www.gog.com/user/changeLanguage/" + languageCode);
                forceLanguageEvent.WaitOne(10000);
            }
            finally
            {
                browser.LoadingStateChanged -= forceLanguage_StateChanged;
            }
        }
        #endregion ForceLanguage

        #region GetLoginRequired
        private void getLoginRequired_StateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading == false)
            {
                var b = (CefSharp.OffScreen.ChromiumWebBrowser)sender;

                if (b.Address.Contains("getFilteredProducts"))
                {
                    loginRequired = false;
                }
                else
                {
                    loginRequired = true;
                }

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
                browser.Load(@"https://www.gog.com/account/getFilteredProducts?hiddenFlag=0&mediaType=1&page=1&sortBy=title");
                loginRequiredEvent.WaitOne(10000);
                return loginRequired;
            }
            finally
            {
                browser.LoadingStateChanged -= getLoginRequired_StateChanged;
            }
        }

        #endregion GetLoginRequired

        #region GetOwnedGames
        private async void getOwnedGames_StateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading == false)
            {
                var getGames = false;
                var b = (CefSharp.OffScreen.ChromiumWebBrowser)sender;

                    if (b.Address.Contains("getFilteredProducts"))
                    {
                        getGames = true;
                    }  

                if (getGames == true)
                {
                    gamesList = await b.GetTextAsync();
                    gamesGotEvent.Set();
                }
            }
        }

        private string gamesList = string.Empty;
        private AutoResetEvent gamesGotEvent = new AutoResetEvent(false);
        public List<GetOwnedGamesResult.Product> GetOwnedGames()
        {
            if (!browser.IsBrowserInitialized)
            {
                browserInitializedEvent.WaitOne(5000);
            }

            ForceLanguage("en");
            var baseUrl = @"https://www.gog.com/account/getFilteredProducts?hiddenFlag=0&mediaType=1&page={0}&sortBy=title";

            try
            {
                gamesList = string.Empty;
                browser.LoadingStateChanged += getOwnedGames_StateChanged;
                browser.Load(string.Format(baseUrl, 1));
                gamesGotEvent.WaitOne(10000);

                var libraryData = JsonConvert.DeserializeObject<GetOwnedGamesResult>(gamesList);
                if (libraryData == null)
                {
                    logger.Error("GOG library content is empty.");
                    return null;
                }

                if (libraryData.totalPages > 1)
                {
                    for (int i = 2; i <= libraryData.totalPages; i++)
                    {
                        gamesList = string.Empty;
                        browser.Load(string.Format(baseUrl, i));
                        gamesGotEvent.WaitOne(10000);
                        var pageData = JsonConvert.DeserializeObject<GetOwnedGamesResult>(gamesList);
                        libraryData.products.AddRange(pageData.products);
                    }
                }

                return libraryData.products;
            }
            finally
            {
                browser.LoadingStateChanged -= getOwnedGames_StateChanged;
            }
        }

        #endregion GetOwnedGames

        #region Login
        private void login_StateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading == false)
            {
                var b = (ChromiumWebBrowser)sender;

                b.Dispatcher.Invoke(() =>
                {
                    if (b.Address.Contains("/on_login_success"))
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

        LoginWindow loginWindow;
        public bool Login(Window parent = null)
        {
            var loginUrl = string.Empty;
            var mainPage = Web.DownloadString("https://www.gog.com/").Split('\n');
            foreach (var line in mainPage)
            {
                if (line.TrimStart().StartsWith("var galaxyAccounts"))
                {
                    var match = Regex.Match(line, "'(.*)','(.*)'");
                    if (match.Success)
                    {
                        loginUrl = match.Groups[1].Value;
                    }
                }
            }

            loginSuccess = false;
            loginWindow = new LoginWindow()
            {
                Height = 465,
                Width = 400
            };
            loginWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;            
            loginWindow.Browser.LoadingStateChanged += login_StateChanged;                        
            loginWindow.Owner = parent;
            loginWindow.Browser.Address = loginUrl;
            loginWindow.ShowDialog();
            return loginSuccess;
        }
        #endregion Login
    }        
}
