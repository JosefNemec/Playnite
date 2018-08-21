using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    public interface IWebView : IDisposable
    {
        void Open();

        bool? OpenDialog();

        void NavigateAndWait(string url);

        void Navigate(string url);

        string GetPageText();

        string GetPageSource();

        string GetCurrentAddress();

        void Close();

        event EventHandler NavigationChanged;
    }

    public interface IWebViewFactory
    {
        IWebView CreateOffscreenView();

        IWebView CreateView(int width, int height);
    }
}
