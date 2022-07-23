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
using Playnite.SDK.Events;
using CefSharp.Wpf.Rendering.Experimental;

namespace Playnite.WebView
{
    public class WebView : WebViewBase, IWebView
    {
        private readonly SynchronizationContext context;
        private AutoResetEvent loadCompleteEvent = new AutoResetEvent(false);
        private WebViewWindow window;
        private readonly string userAgent;

        public bool CanExecuteJavascriptInMainFrame => window.Browser.CanExecuteJavascriptInMainFrame;
        public event EventHandler NavigationChanged;
        public event EventHandler<WebViewLoadingChangedEventArgs> LoadingChanged;
        public Window WindowHost => window;

        public WebView(int width, int height, bool useCompositionRenderer = false) : this(width, height, Colors.Transparent, "", useCompositionRenderer)
        {
        }

        public WebView(int width, int height, Color background, string userAgent, bool useCompositionRenderer = false)
        {
            context = SynchronizationContext.Current;
            window = new WebViewWindow();
            window.Browser.LoadingStateChanged += Browser_LoadingStateChanged;
            window.Browser.TitleChanged += Browser_TitleChanged;

            this.userAgent = userAgent;
            if (!userAgent.IsNullOrEmpty())
            {
                window.Browser.IsBrowserInitializedChanged += Browser_IsBrowserInitializedChanged;
                window.Browser.RequestHandler = new CustomRequestHandler(userAgent);
            }

            if (useCompositionRenderer)
            {
                window.Browser.RenderHandler = new CompositionTargetRenderHandler(window.Browser, window.Browser.DpiScaleFactor, window.Browser.DpiScaleFactor);
            }

            window.Owner = WindowManager.CurrentWindow;
            window.Width = width;
            window.Height = height;
            window.PanelContent.Background = new SolidColorBrush(background);
        }

        private async void Browser_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((e.NewValue is bool init) && init == true && !userAgent.IsNullOrEmpty())
            {
                using (var client = window.Browser.GetDevToolsClient())
                {
                    await client.Network.SetUserAgentOverrideAsync(userAgent);
                    await client.Emulation.SetUserAgentOverrideAsync(userAgent);
                }
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
            window.Browser.LoadingStateChanged -= Browser_LoadingStateChanged;
            window.Browser.TitleChanged -= Browser_TitleChanged;
            window.Browser.IsBrowserInitializedChanged -= Browser_IsBrowserInitializedChanged;
            window.Close();
            window.Browser.Dispose();
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

        public async Task<JavaScriptEvaluationResult> EvaluateScriptAsync(string script)
        {
            var res = await window.Browser.EvaluateScriptAsync(script);
            return new JavaScriptEvaluationResult
            {
                Message = res.Message,
                Result = res.Result,
                Success = res.Success
            };
        }
    }
}
