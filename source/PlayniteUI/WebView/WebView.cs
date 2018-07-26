using CefSharp;
using Playnite.SDK;
using PlayniteUI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlayniteUI.WebView
{
    public class WebView : IWebView
    {
        private AutoResetEvent loadCompleteEvent = new AutoResetEvent(false);

        private WebViewWindow window;

        public WebView(int width, int height)
        {
            window = new WebViewWindow();
            window.Browser.LoadingStateChanged += Browser_LoadingStateChanged;
            window.Width = width;
            window.Height = height;
            window.Show();
        }

        private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading == false)
            {
                loadCompleteEvent.Set();
            }
        }

        public void Close()
        {
            window.Close();
        }

        public void Dispose()
        {
            window?.Close();
            window?.Browser.Dispose();
        }

        public string GetCurrentAddress()
        {
            return window.Browser.Address;
        }

        public string GetPageText()
        {
            return window.Browser.GetTextAsync().GetAwaiter().GetResult();
        }

        public string GetPageSource()
        {
            return window.Browser.GetSourceAsync().GetAwaiter().GetResult();
        }

        public void Navigate(string url)
        {
            window.Browser.Load(url);
            loadCompleteEvent.WaitOne(10000);
        }
    }
}
