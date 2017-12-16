using CefSharp;
using CefSharp.Wpf;
using NLog;
using Playnite.Controls;
using Playnite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.Providers.BattleNet
{
    public class WebApiClient
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
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

        #region GetLoginRequired
        private void getLoginRequired_StateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading == false)
            {
                var b = (CefSharp.OffScreen.ChromiumWebBrowser)sender;
                Console.WriteLine(b.Address);
                if (b.Address.Contains("battle.net/login") || !b.Address.StartsWith("http"))
                {
                    loginRequired = true;
                }
                else
                {
                    loginRequired = false;
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
                browser.Load(libraryUrl);
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
                var b = (CefSharp.OffScreen.ChromiumWebBrowser)sender;
                gamesList = await b.GetSourceAsync();
                gamesGotEvent.Set();                
            }
        }

        private string gamesList = string.Empty;
        private AutoResetEvent gamesGotEvent = new AutoResetEvent(false);
        public string GetOwnedGames()
        {
            if (!browser.IsBrowserInitialized)
            {
                browserInitializedEvent.WaitOne(5000);
            }

            try
            {
                gamesList = string.Empty;
                browser.LoadingStateChanged += getOwnedGames_StateChanged;
                browser.Load(libraryUrl);
                gamesGotEvent.WaitOne(10000);
                return gamesList;
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
                    if (b.Address.EndsWith(libraryUrl))
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
        private string loginUrl = "battle.net/account/management/?logout";
        private string libraryUrl = @"battle.net/account/management/";

        LoginWindow loginWindow;
        public bool Login(Window parent = null)
        {
            loginSuccess = false;
            loginWindow = new LoginWindow()
            {
                Height = 500,
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
