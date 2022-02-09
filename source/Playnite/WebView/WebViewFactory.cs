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
        private ExtensionManifest extensionOwner;

        public WebViewFactory(PlayniteSettings settings)
        {
            appSettings = settings;
        }

        public WebViewFactory(PlayniteSettings settings, ExtensionManifest extensionOwner) : this(settings)
        {
            this.extensionOwner = extensionOwner;
        }

        public IWebView CreateOffscreenView()
        {
            return new OffscreenWebView(extensionOwner);
        }

        public IWebView CreateOffscreenView(WebViewSettings settings)
        {
            return new OffscreenWebView(settings, extensionOwner);
        }

        public IWebView CreateView(int width, int height)
        {
            return new WebView(width, height, appSettings.UseCompositionWebViewRenderer, extensionOwner);
        }

        public IWebView CreateView(int width, int height, Color background)
        {
            return new WebView(width, height, background, string.Empty, appSettings.UseCompositionWebViewRenderer, extensionOwner);
        }

        public IWebView CreateView(WebViewSettings settings)
        {
            return new WebView(settings.WindowWidth, settings.WindowHeight, settings.WindowBackground, settings.UserAgent, appSettings.UseCompositionWebViewRenderer, extensionOwner);
        }
    }
}
