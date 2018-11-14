using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteUI.WebView
{
    public class WebViewFactory : IWebViewFactory
    {
        public IWebView CreateOffscreenView()
        {
            return new OffscreenWebView();
        }

        public IWebView CreateView(int width, int height)
        {
            return new WebView(width, height);
        }
    }
}
