using Playnite.SDK.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        [Obsolete("AppCache was removed from CEF")]
        public bool CacheEnabled { get; set; } = true;

        /// <summary>
        /// User agent to be used for specific browser instance. Leave empty to use default.
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Gets or sets window width.
        /// </summary>
        public int WindowWidth { get; set; } = 0;

        /// <summary>
        /// Gets or sets window height.
        /// </summary>
        public int WindowHeight { get; set; } = 0;

        /// <summary>
        /// Gets or sets window background color.
        /// </summary>
        public Color WindowBackground { get; set; }
    }

    /// <summary>
    /// Represents JavaScript evaluation resut.
    /// </summary>
    public class JavaScriptEvaluationResult
    {
        /// <summary>
        /// Gets or sets error message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets value indicating whether the javascript executed successfully.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets result of script evaluation.
        /// </summary>
        public object Result { get; set; }
    }

    /// <summary>
    /// Describes web view object.
    /// </summary>
    public interface IWebView : IDisposable
    {
        /// <summary>
        /// Gets a flag that indicates if you can execute javascript in the main frame.
        /// </summary>
        bool CanExecuteJavascriptInMainFrame { get; }

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
        /// Deletes all cookies from domains matching specified regular expression.
        /// </summary>
        /// <param name="domainRegex"></param>
        void DeleteDomainCookiesRegex(string domainRegex);

        /// <summary>
        /// Deletes cookies.
        /// </summary>
        /// <param name="url">Cookie URL.</param>
        /// <param name="name">Cookie name.</param>
        void DeleteCookies(string url, string name);

        /// <summary>
        /// Gets all cookies.
        /// </summary>
        /// <returns>List of cookies.</returns>
        List<HttpCookie> GetCookies();

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
        /// Sets cookie data.
        /// </summary>
        /// <param name="url">Cookie URL.</param>
        /// <param name="cookie">Cookie data.</param>
        void SetCookies(string url, HttpCookie cookie);

        /// <summary>
        /// Closes view.
        /// </summary>
        void Close();

        /// <summary>
        /// Occurs when web view loading changes, for example when page is loaded.
        /// </summary>
        event EventHandler<WebViewLoadingChangedEventArgs> LoadingChanged;

        /// <summary>
        /// Evaluates JavaScript script in the browser instance.
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        Task<JavaScriptEvaluationResult> EvaluateScriptAsync(string script);

        /// <summary>
        /// Gets window host for the web view. Doesn't apply to off-screen views.
        /// </summary>
        Window WindowHost { get; }
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

        /// <summary>
        /// Creates new web view.
        /// </summary>
        /// <param name="settings">Browser view settings.</param>
        /// <returns></returns>
        IWebView CreateView(WebViewSettings settings);
    }

    /// <summary>
    /// Represents web view cookie object.
    /// </summary>
    public class HttpCookie
    {
        /// <summary>
        /// Creates new instance of <see cref="HttpCookie"/>.
        /// </summary>
        public HttpCookie()
        {
        }

        /// <summary>
        /// The cookie name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The cookie value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The cookie domain.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// The cookie path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The cookie expire date.
        /// </summary>
        public DateTime? Expires { get; set; }

        /// <summary>
        /// The cookie creation date.
        /// </summary>
        public DateTime Creation { get; set; }

        /// <summary>
        /// If true the cookie will only be sent for HTTPS requests.
        /// </summary>
        public bool Secure { get; set; }

        /// <summary>
        /// If true the cookie will only be sent for HTTP requests.
        /// </summary>
        public bool HttpOnly { get; set; }

        /// <summary>
        /// The cookie last access date. This is automatically populated by the system on access.
        /// </summary>
        public DateTime LastAccess { get; set; }

        /// <summary>
        /// Same site
        /// </summary>
        public CookieSameSite SameSite { get; set; }

        /// <summary>
        /// Priority
        /// </summary>
        public CookiePriority Priority { get; set; }
    }

    /// <summary>
    /// Cookie same site values.
    /// </summary>
    /// <remarks>
    /// See https://source.chromium.org/chromium/chromium/src/+/master:net/cookies/cookie_constants.h
    /// </remarks>
    public enum CookieSameSite
    {
        /// <summary>
        /// Unspecified
        /// </summary>
        Unspecified = 0,
        /// <summary>
        /// Cookies will be sent in all contexts, i.e sending cross-origin is allowed. None
        /// used to be the default value, but recent browser versions made Lax the default
        /// value to have reasonably robust defense against some classes of cross-site request
        /// forgery (CSRF) attacks.
        /// </summary>
        NoRestriction = 1,
        /// <summary>
        /// Cookies are allowed to be sent with top-level navigations and will be sent along
        /// with GET request initiated by third party website. This is the default value
        /// in modern browsers.
        /// </summary>
        LaxMode = 2,
        /// <summary>
        /// Cookies will only be sent in a first-party context and not be sent along with
        /// requests initiated by third party websites.
        /// </summary>
        StrictMode = 3
    }

    /// <summary>
    /// Cookie priority values.
    /// </summary>
    public enum CookiePriority
    {
        /// <summary>
        /// Low Priority
        /// </summary>
        Low = -1,
        /// <summary>
        /// Medium Priority
        /// </summary>
        Medium = 0,
        /// <summary>
        /// High Priority
        /// </summary>
        High = 1
    }
}
