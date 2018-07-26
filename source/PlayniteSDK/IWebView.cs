using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    public interface IWebView : IDisposable
    {
        void Navigate(string url);

        string GetPageText();

        string GetPageSource();

        string GetCurrentAddress();

        void Close();
    }

    public interface IWebViewFactory
    {
        IWebView CreateOffscreenView();

        IWebView CreateView(int width, int height);
    }
}
