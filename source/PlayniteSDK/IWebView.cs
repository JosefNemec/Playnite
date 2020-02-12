using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Playnite.SDK
{
    /// <summary>
    /// Represents browser view settings.
    /// </summary>
    public class WebViewSettings
    {
        /// <summary>
        /// Gets or sets value indicating whether JavaScript exection is enabled.
        /// </summary>
        public bool JavaScriptEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets value indicating whether cache is enabled.
        /// </summary>
        public bool CacheEnabled { get; set; } = true;
    }

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
        /// Gets page text.
        /// </summary>
        /// <returns>Page text.</returns>
        Task<string> GetPageTextAsync();

        /// <summary>
        /// Gets document source.
        /// </summary>
        /// <returns>Document source.</returns>
        string GetPageSource();

        /// <summary>
        /// Gets document source.
        /// </summary>
        /// <returns>Document source task.</returns>
        Task<string> GetPageSourceAsync();

        /// <summary>
        /// Gets current URL address.
        /// </summary>
        /// <returns>URL address.</returns>
        string GetCurrentAddress();

        /// <summary>
        /// Deletes all cookies from specified domain.
        /// </summary>
        /// <param name="domain">Cookie domain.</param>
        void DeleteDomainCookies(string domain);

        /// <summary>
        /// Deletes cookies.
        /// </summary>
        /// <param name="url">Cookie URL.</param>
        /// <param name="name">Cookie name.</param>
        void DeleteCookies(string url, string name);

        /// <summary>
        /// Sets cookie data.
        /// </summary>
        /// <param name="url">Cookie URL.</param>
        /// <param name="domain">Cookie domain.</param>
        /// <param name="name">Cookie name.</param>
        /// <param name="value">Cookie value.</param>
        /// <param name="path">Cookie url path.</param>
        /// <param name="expires">Expiration date.</param>
        void SetCookies(string url, string domain, string name, string value, string path, DateTime expires);

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
        /// Creates new offscreen web view with specific settings.
        /// </summary>
        /// <param name="settings">Browser view settings.</param>
        /// <returns></returns>
        IWebView CreateOffscreenView(WebViewSettings settings);

        /// <summary>
        /// Creates new web view.
        /// </summary>
        /// <param name="width">View widht.</param>
        /// <param name="height">View height.</param>
        /// <returns>Web view.</returns>
        IWebView CreateView(int width, int height);

        /// <summary>
        /// Creates new web view.
        /// </summary>
        /// <param name="width">View widht.</param>
        /// <param name="height">View height.</param>
        /// <param name="background">View background color.</param>
        /// <returns>Web view.</returns>
        IWebView CreateView(int width, int height, Color background);
    }
}
