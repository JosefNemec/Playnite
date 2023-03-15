using CefSharp;
using DiscordRPC;
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
        // This needs to be done before calling cookie visit methods because we won't get any callbacks if no cookies exist,
        // which would lead to a deadlock. There's no way how to tell beforehand if any cookies exist or not.
        private void MakeSureCookiesExist(ICookieManager manager)
        {
            using (var setHandler = new SetCookieHandler())
            {
                if (manager.SetCookie("https://playnite.test", new Cookie { Domain = "playnite.test", Name = "playnite.test", Value = "playnite.test" }, setHandler))
                {
                    setHandler.Finished.WaitOne();
                }
            }
        }

        public List<HttpCookie> GetCookies()
        {
            using (var manager = Cef.GetGlobalCookieManager())
            using (var visitor = new StandardCookieVisitor())
            {
                MakeSureCookiesExist(manager);
                if (manager.VisitAllCookies(visitor))
                {
                    visitor.Finished.WaitOne();
                }

                return visitor.Cookies;
            }
        }

        public void DeleteDomainCookies(string domain)
        {
            using (var manager = Cef.GetGlobalCookieManager())
            using (var destoyer = new CookieDestroyer(domain, false))
            {
                MakeSureCookiesExist(manager);
                if (manager.VisitAllCookies(destoyer))
                {
                    destoyer.Finished.WaitOne();
                }
            }
        }

        public void DeleteDomainCookiesRegex(string domainRegex)
        {
            using (var manager = Cef.GetGlobalCookieManager())
            using (var destoyer = new CookieDestroyer(domainRegex, true))
            {
                MakeSureCookiesExist(manager);
                if (manager.VisitAllCookies(destoyer))
                {
                    destoyer.Finished.WaitOne();
                }
            }
        }

        public void DeleteCookies(string url, string name)
        {
            using (var manager = Cef.GetGlobalCookieManager())
            using (var deleteHandle = new DeleteCookiesHandler())
            {
                MakeSureCookiesExist(manager);
                if (manager.DeleteCookies(url, name, deleteHandle))
                {
                    deleteHandle.Finished.WaitOne();
                }
            }
        }

        public void SetCookies(string url, string domain, string name, string value, string path, DateTime expires)
        {
            using (var manager = Cef.GetGlobalCookieManager())
            using (var setHandler = new SetCookieHandler())
            {
                if (manager.SetCookie(url, new Cookie
                {
                    Domain = domain,
                    Name = name,
                    Value = value,
                    Expires = expires,
                    HttpOnly = false,
                    Secure = false,
                    Path = path
                }, setHandler))
                {
                    setHandler.Finished.WaitOne();
                }
            }
        }

        public void SetCookies(string url, HttpCookie cookie)
        {
            using (var manager = Cef.GetGlobalCookieManager())
            using (var setHandler = new SetCookieHandler())
            {
                if (manager.SetCookie(url, new Cookie()
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
                }, setHandler))
                {
                    setHandler.Finished.WaitOne();
                }
            }
        }
    }
}
