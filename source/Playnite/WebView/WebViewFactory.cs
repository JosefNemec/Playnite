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
            return new WebView(width, height);
        }

        public IWebView CreateView(int width, int height, Color background)
        {
            return new WebView(width, height);
        }
    }
}
