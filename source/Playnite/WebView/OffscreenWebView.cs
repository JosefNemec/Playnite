using CefSharp;
using Playnite.SDK;
using Playnite.SDK.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.WebView
{
    public class OffscreenWebView : WebViewBase, IWebView
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private AutoResetEvent browserInitializedEvent = new AutoResetEvent(false);
        private AutoResetEvent loadCompleteEvent = new AutoResetEvent(false);
        private CefSharp.OffScreen.ChromiumWebBrowser browser;
        private readonly string userAgent;

        public bool CanExecuteJavascriptInMainFrame => browser.CanExecuteJavascriptInMainFrame;
        public Window WindowHost { get; }

        public event EventHandler NavigationChanged;
        public event EventHandler<WebViewLoadingChangedEventArgs> LoadingChanged;

        public OffscreenWebView()
        {
            Initialize();
        }

        public OffscreenWebView(WebViewSettings settings)
        {
            this.userAgent = settings.UserAgent;
            Initialize(new BrowserSettings
            {
                Javascript = settings.JavaScriptEnabled ? CefState.Enabled : CefState.Disabled
            });
        }

        private void Initialize(BrowserSettings settings = null)
        {
            browser = new CefSharp.OffScreen.ChromiumWebBrowser(automaticallyCreateBrowser: false);
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
            browser.LoadingStateChanged -= Browser_LoadingStateChanged;
            browser.BrowserInitialized -= Browser_BrowserInitialized;
            browser.Dispose();
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
    }
}
