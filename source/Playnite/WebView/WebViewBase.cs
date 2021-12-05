using CefSharp;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.WebView
{
    public class CustomResourceRequestHandler : CefSharp.Handler.ResourceRequestHandler
    {
        private readonly string userAgent;

        public CustomResourceRequestHandler(string userAgent)
        {
            this.userAgent = userAgent;
        }

        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            request.SetHeaderByName("user-agent", userAgent, true);
            return CefReturnValue.Continue;
        }
    }

    public class CustomRequestHandler : CefSharp.Handler.RequestHandler
    {
        private readonly CustomResourceRequestHandler handler;

        public CustomRequestHandler(string userAgent)
        {
            handler = new CustomResourceRequestHandler(userAgent);
        }

        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            return handler;
        }
    }

    public class WebViewBase
    {
        public List<HttpCookie> GetCookies()
        {
            var cookies = new List<HttpCookie>();
            using (var manager = Cef.GetGlobalCookieManager())
            {
                manager.VisitAllCookiesAsync().GetAwaiter().GetResult().ForEach(cookie =>
                cookies.Add(new HttpCookie
                {
                    Name = cookie.Name,
                    Value = cookie.Value,
                    Domain = cookie.Domain,
                    Path = cookie.Path,
                    Expires = cookie.Expires,
                    Creation = cookie.Creation,
                    HttpOnly = cookie.HttpOnly,
                    LastAccess = cookie.LastAccess,
                    Priority = (CookiePriority)(int)cookie.Priority,
                    SameSite = (CookieSameSite)(int)cookie.SameSite,
                    Secure = cookie.Secure
                }));
            }

            return cookies;
        }

        public void DeleteDomainCookies(string domain)
        {
            using (var destoyer = new CookieDestroyer(domain))
            {
                using (var manager = Cef.GetGlobalCookieManager())
                {
                    manager.VisitAllCookies(destoyer);
                }
            }
        }

        public void DeleteCookies(string url, string name)
        {
            using (var manager = Cef.GetGlobalCookieManager())
            {
                manager.DeleteCookies(url, name);
            }
        }

        public void SetCookies(string url, string domain, string name, string value, string path, DateTime expires)
        {
            using (var manager = Cef.GetGlobalCookieManager())
            {
                manager.SetCookie(url, new Cookie()
                {
                    Domain = domain,
                    Name = name,
                    Value = value,
                    Expires = expires,
                    HttpOnly = false,
                    Secure = false,
                    Path = path
                });
            }
        }

        public void SetCookies(string url, HttpCookie cookie)
        {
            using (var manager = Cef.GetGlobalCookieManager())
            {
                manager.SetCookie(url, new Cookie()
                {
                    Domain = cookie.Domain,
                    Expires = cookie.Expires,
                    HttpOnly = cookie.HttpOnly,
                    Secure = cookie.Secure,
                    SameSite = (CefSharp.Enums.CookieSameSite)(int)cookie.SameSite,
                    Priority = (CefSharp.Enums.CookiePriority)(int)cookie.Priority,
                    Name = cookie.Name,
                    Path = cookie.Path,
                    Value = cookie.Value
                });
            }
        }
    }
}
