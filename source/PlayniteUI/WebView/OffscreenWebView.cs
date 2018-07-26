using CefSharp;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlayniteUI.WebView
{
    public class OffscreenWebView : IWebView
    {
        private AutoResetEvent browserInitializedEvent = new AutoResetEvent(false);
        private AutoResetEvent loadCompleteEvent = new AutoResetEvent(false);

        private CefSharp.OffScreen.ChromiumWebBrowser browser;


        public OffscreenWebView()
        {
            browser = new CefSharp.OffScreen.ChromiumWebBrowser(automaticallyCreateBrowser: false);
            browser.LoadingStateChanged += Browser_LoadingStateChanged;
            browser.BrowserInitialized += Browser_BrowserInitialized;
            browser.CreateBrowser(IntPtr.Zero);
            browserInitializedEvent.WaitOne(5000);
        }

        private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading == false)
            {
                loadCompleteEvent.Set();
            }
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

        public string GetPageSource()
        {
            return browser.GetSourceAsync().GetAwaiter().GetResult();
        }

        public void Navigate(string url)
        {
            browser.Load(url);
            loadCompleteEvent.WaitOne(10000);
        }
    }
}
