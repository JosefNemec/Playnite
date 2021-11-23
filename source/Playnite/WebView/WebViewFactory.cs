using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Playnite.WebView
{
    public class WebViewFactory : IWebViewFactory
    {
        private PlayniteSettings appSettings;

        public WebViewFactory(PlayniteSettings settings)
        {
            appSettings = settings;
        }

        public IWebView CreateOffscreenView()
        {
            return new OffscreenWebView();
        }

        public IWebView CreateOffscreenView(WebViewSettings settings)
        {
            return new OffscreenWebView(settings);
        }

        public IWebView CreateView(int width, int height)
        {
            return new WebView(width, height, appSettings.UseCompositionWebViewRenderer);
        }

        public IWebView CreateView(int width, int height, Color background)
        {
            return new WebView(width, height, background, string.Empty, appSettings.UseCompositionWebViewRenderer);
        }

        public IWebView CreateView(WebViewSettings settings)
        {
            return new WebView(settings.WindowWidth, settings.WindowHeight, settings.WindowBackground, settings.UserAgent, appSettings.UseCompositionWebViewRenderer);
        }
    }
}
