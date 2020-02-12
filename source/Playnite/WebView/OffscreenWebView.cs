using CefSharp;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.WebView
{
    public class OffscreenWebView : IWebView
    {
        private AutoResetEvent browserInitializedEvent = new AutoResetEvent(false);
        private AutoResetEvent loadCompleteEvent = new AutoResetEvent(false);

        private CefSharp.OffScreen.ChromiumWebBrowser browser;

        public event EventHandler NavigationChanged;

        public OffscreenWebView()
        {
            browser = new CefSharp.OffScreen.ChromiumWebBrowser(automaticallyCreateBrowser: false);
            browser.LoadingStateChanged += Browser_LoadingStateChanged;
            browser.BrowserInitialized += Browser_BrowserInitialized;
            browser.CreateBrowser();
            browserInitializedEvent.WaitOne(5000);
        }

        public OffscreenWebView(WebViewSettings settings)
        {
            browser = new CefSharp.OffScreen.ChromiumWebBrowser(automaticallyCreateBrowser: false);
            browser.LoadingStateChanged += Browser_LoadingStateChanged;
            browser.BrowserInitialized += Browser_BrowserInitialized;
            var brwSet = new BrowserSettings
            {
                Javascript = settings.JavaScriptEnabled ? CefState.Enabled : CefState.Disabled,
                ApplicationCache = settings.CacheEnabled ? CefState.Enabled : CefState.Disabled
            };
            browser.CreateBrowser(null, brwSet);
            browserInitializedEvent.WaitOne(5000);
        }

        private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading == false)
            {
                loadCompleteEvent.Set();
            }

            NavigationChanged?.Invoke(this, new EventArgs());
        }

        private void Browser_BrowserInitialized(object sender, EventArgs e)
        {
            browserInitializedEvent.Set();
        }

        public void Close()
        {
        }

        public void Dispose()
        {
            browser?.Dispose();
        }

        public string GetCurrentAddress()
        {
            return browser.Address;
        }

        public string GetPageText()
        {
            return browser.GetTextAsync().GetAwaiter().GetResult();
        }

        public Task<string> GetPageTextAsync()
        {
            return browser.GetTextAsync();
        }

        public string GetPageSource()
        {
            return browser.GetSourceAsync().GetAwaiter().GetResult();
        }

        public Task<string> GetPageSourceAsync()
        {
            return browser.GetSourceAsync();
        }

        public void NavigateAndWait(string url)
        {
            browser.Load(url);
            loadCompleteEvent.WaitOne(20000);
        }

        public void Navigate(string url)
        {
            browser.Load(url);
        }

        public void Open()
        {
            throw new NotImplementedException();
        }

        public bool? OpenDialog()
        {
            throw new NotImplementedException();
        }

        public void DeleteDomainCookies(string domain)
        {
            using (var destoyer = new CookieDestroyer(domain))
            {
                using (var manager = Cef.GetGlobalCookieManager())
                {
                    manager.VisitAllCookies(destoyer);
                }
            }
        }

        public void DeleteCookies(string url, string name)
        {
            using (var manager = Cef.GetGlobalCookieManager())
            {
                manager.DeleteCookies(url, name);
            }
        }

        public void SetCookies(string url, string domain, string name, string value, string path, DateTime expires)
        {
            using (var manager = Cef.GetGlobalCookieManager())
            {
                manager.SetCookie(url, new Cookie()
                {
                    Domain = domain,
                    Name = name,
                    Value = value,
                    Expires = expires,
                    Creation = DateTime.Now,
                    HttpOnly = false,
                    LastAccess = DateTime.Now,
                    Secure = false,
                    Path = path
                });
            }
        }
    }
}
