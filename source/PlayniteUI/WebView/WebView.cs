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
        private readonly SynchronizationContext context;

        private AutoResetEvent loadCompleteEvent = new AutoResetEvent(false);

        private WebViewWindow window;

        public event EventHandler NavigationChanged;

        public WebView(int width, int height)
        {
            context = SynchronizationContext.Current;
            window = new WebViewWindow();
            window.Browser.LoadingStateChanged += Browser_LoadingStateChanged;            
            window.Owner = PlayniteWindows.CurrentWindow;
            window.Width = width;
            window.Height = height;
        }

        private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading == false)
            {
                loadCompleteEvent.Set();
            }

            NavigationChanged?.Invoke(this, new EventArgs());
        }

        public void Close()
        {
            context.Send(a => window.Close(), null);
        }

        public void Dispose()
        {            
            window?.Close();
            window?.Browser.Dispose();
        }

        public string GetCurrentAddress()
        {
            var address = string.Empty;
            context.Send(a => address = window.Browser.Address, null);
            return address;
        }

        public string GetPageText()
        {
            var text = string.Empty;
            context.Send(a => text = window.Browser.GetTextAsync().GetAwaiter().GetResult(), null);
            return text;
        }

        public string GetPageSource()
        {
            var text = string.Empty;
            context.Send(a => text = window.Browser.GetSourceAsync().GetAwaiter().GetResult(), null);
            return text;
        }

        public void NavigateAndWait(string url)
        {
            context.Send(a => window.Browser.Address = url, null);            
            loadCompleteEvent.WaitOne(20000);
        }

        public void Navigate(string url)
        {
            context.Send(a => window.Browser.Address = url, null);
        }

        public void Open()
        {
            window.Show();
        }

        public bool? OpenDialog()
        {
            return window.ShowDialog();
        }
    }
}
