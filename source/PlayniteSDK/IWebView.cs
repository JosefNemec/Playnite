using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Describes web view object.
    /// </summary>
    public interface IWebView : IDisposable
    {
        /// <summary>
        /// Open view.
        /// </summary>
        void Open();

        /// <summary>
        /// Open view as modal dialog.
        /// </summary>
        /// <returns></returns>
        bool? OpenDialog();

        /// <summary>
        /// Navigates to url and wait for page to be loaded.
        /// </summary>
        /// <param name="url">URL to load.</param>
        void NavigateAndWait(string url);

        /// <summary>
        /// Navigates to url.
        /// </summary>
        /// <param name="url">URL to load.</param>
        void Navigate(string url);

        /// <summary>
        /// Gets page text.
        /// </summary>
        /// <returns>Page text.</returns>
        string GetPageText();

        /// <summary>
        /// Gets document source.
        /// </summary>
        /// <returns>Document source.</returns>
        string GetPageSource();

        /// <summary>
        /// Gets current URL address.
        /// </summary>
        /// <returns>URL address.</returns>
        string GetCurrentAddress();

        /// <summary>
        /// Closes view.
        /// </summary>
        void Close();

        /// <summary>
        /// Occurs when web view navigatates to a new page.
        /// </summary>
        event EventHandler NavigationChanged;
    }

    /// <summary>
    /// Describes web view factory provider.
    /// </summary>
    public interface IWebViewFactory
    {
        /// <summary>
        /// Creates new offscreen web view.
        /// </summary>
        /// <returns>Offscreen web view.</returns>
        IWebView CreateOffscreenView();

        /// <summary>
        /// Creates new web view.
        /// </summary>
        /// <param name="width">View widht.</param>
        /// <param name="height">View height.</param>
        /// <returns>Web view.</returns>
        IWebView CreateView(int width, int height);
    }
}
