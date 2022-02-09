using CefSharp;
using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.WebView
{
    public class OffscreenWebView : WebViewBase, IWebView
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private AutoResetEvent browserInitializedEvent = new AutoResetEvent(false);
        private AutoResetEvent loadCompleteEvent = new AutoResetEvent(false);
        private CefSharp.OffScreen.ChromiumWebBrowser browser;
        private RequestContext requestContext;
        private readonly string userAgent;

        public bool CanExecuteJavascriptInMainFrame => browser.CanExecuteJavascriptInMainFrame;
        public event EventHandler NavigationChanged;
        public event EventHandler<WebViewLoadingChangedEventArgs> LoadingChanged;

        public OffscreenWebView(ExtensionManifest extensionOwner)
        {
            Initialize(null, extensionOwner);
        }

        public OffscreenWebView(WebViewSettings settings, ExtensionManifest extensionOwner)
        {
            this.userAgent = settings.UserAgent;
            Initialize(new BrowserSettings
            {
                Javascript = settings.JavaScriptEnabled ? CefState.Enabled : CefState.Disabled
            }, extensionOwner);
        }

        private void Initialize(BrowserSettings settings = null, ExtensionManifest extensionOwner = null)
        {
            if (extensionOwner != null)
            {
                var cachePath = Path.Combine(PlaynitePaths.BrowserPluginInstancesCachePath, Paths.GetSafePathName(extensionOwner.Id));
                FileSystem.CreateDirectory(cachePath, false);
                requestContext = new RequestContext(new RequestContextSettings
                {
                    CachePath = cachePath,
                    PersistUserPreferences = true
                });
            }

            browser = new CefSharp.OffScreen.ChromiumWebBrowser(automaticallyCreateBrowser: false, requestContext: requestContext);
            if (!userAgent.IsNullOrEmpty())
            {
                browser.RequestHandler = new CustomRequestHandler(userAgent);
            }

            browser.LoadingStateChanged += Browser_LoadingStateChanged;
            browser.BrowserInitialized += Browser_BrowserInitialized;
            if (settings != null)
            {
                browser.CreateBrowser(null, settings);
            }
            else
            {
                browser.CreateBrowser();
            }

            if (!browserInitializedEvent.WaitOne(30000))
            {
                logger.Error("Failed to initialize OffscreenWebView in timely manner.");
            }
        }

        private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading == false)
            {
                loadCompleteEvent.Set();
            }

            LoadingChanged?.Invoke(this, new WebViewLoadingChangedEventArgs { IsLoading = e.IsLoading });
            NavigationChanged?.Invoke(this, new EventArgs());
        }

        private async void Browser_BrowserInitialized(object sender, EventArgs e)
        {
            browserInitializedEvent.Set();
            if (!userAgent.IsNullOrEmpty())
            {
                using (var client = browser.GetDevToolsClient())
                {
                    await client.Network.SetUserAgentOverrideAsync(userAgent);
                    await client.Emulation.SetUserAgentOverrideAsync(userAgent);
                }
            }
        }

        public void Close()
        {
        }

        public void Dispose()
        {
            browser?.Dispose();
            requestContext?.Dispose();
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

        public async Task<JavaScriptEvaluationResult> EvaluateScriptAsync(string script)
        {
            var res = await browser.EvaluateScriptAsync(script);
            return new JavaScriptEvaluationResult
            {
                Message = res.Message,
                Result = res.Result,
                Success = res.Success
            };
        }

        public void DeleteDomainCookies(string domain)
        {
            DeleteDomainCookiesBase(domain, browser);
        }

        public void DeleteCookies(string url, string name)
        {
            DeleteCookiesBase(url, name, browser);
        }

        public List<HttpCookie> GetCookies()
        {
            return GetCookiesBase(browser);
        }

        public void SetCookies(string url, string domain, string name, string value, string path, DateTime expires)
        {
            SetCookiesBase(url, domain, name, value, path, expires, browser);
        }

        public void SetCookies(string url, HttpCookie cookie)
        {
            SetCookiesBase(url, cookie, browser);
        }
    }
}
