using CefSharp;
using Playnite.Windows;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace Playnite.WebView
{
    public class WebView : IWebView
    {
        private readonly SynchronizationContext context;

        private AutoResetEvent loadCompleteEvent = new AutoResetEvent(false);

        private WebViewWindow window;

        public event EventHandler NavigationChanged;

        public WebView(int width, int height) : this(width, height, Colors.Transparent)
        {
        }

        public WebView(int width, int height, Color background)
        {
            context = SynchronizationContext.Current;
            window = new WebViewWindow();
            window.Browser.LoadingStateChanged += Browser_LoadingStateChanged;
            window.Browser.TitleChanged += Browser_TitleChanged;
            window.Owner = WindowManager.CurrentWindow;
            window.Width = width;
            window.Height = height;
            window.Background = new SolidColorBrush(background);
        }

        private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading == false)
            {
                loadCompleteEvent.Set();
            }

            NavigationChanged?.Invoke(this, new EventArgs());
        }

        private void Browser_TitleChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            string titlePrefix = args.NewValue as string;
            string titleSuffix = "Playnite";

            window.Title = string.IsNullOrEmpty(titlePrefix) ? titleSuffix : string.Format("{0} - {1}", titlePrefix, titleSuffix);
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

        public Task<string> GetPageTextAsync()
        {
            return window.Browser.GetTextAsync();
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

        public Task<string> GetPageSourceAsync()
        {
            return window.Browser.GetSourceAsync();
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
