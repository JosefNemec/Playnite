using CefSharp;
using DiscordRPC;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Playnite.WebView
{
    public class CustomResourceRequestHandler : CefSharp.Handler.ResourceRequestHandler
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly WebViewSettings settings;
        private readonly MemoryStream contentStream;

        public CustomResourceRequestHandler(WebViewSettings settings)
        {
            this.settings = settings;
            if (settings.PassResourceContentStreamToCallback)
                contentStream = new MemoryStream();
        }

        public static Playnite.SDK.WebViewModels.Request ConvertRequest(IRequest request)
        {
            var result = new SDK.WebViewModels.Request
            {
                Method = request.Method,
                ResourceType = (Playnite.SDK.WebViewModels.ResourceType)request.ResourceType,
                Url = request.Url,
                Headers = new Dictionary<string, string>()
            };

            foreach (string header in request.Headers)
                result.Headers.Add(header, request.Headers[header]);

            return result;
        }

        public static Playnite.SDK.WebViewModels.Response ConvertResponse(IResponse response)
        {
            var result = new SDK.WebViewModels.Response
            {
                Charset = response.Charset,
                MimeType = response.MimeType,
                StatusCode = response.StatusCode,
                StatusText = response.StatusText,
                Headers = new Dictionary<string, string>()
            };

            foreach (string header in response.Headers)
                result.Headers.Add(header, response.Headers[header]);

            return result;
        }

        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            if (!settings.UserAgent.IsNullOrWhiteSpace())
                request.SetHeaderByName("user-agent", settings.UserAgent, true);
            return CefReturnValue.Continue;
        }

        protected override IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            if (settings.PassResourceContentStreamToCallback)
                return new CefSharp.ResponseFilter.StreamResponseFilter(contentStream);
            else
                return base.GetResourceResponseFilter(chromiumWebBrowser, browser, frame, request, response);
        }

        protected override void OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            if (settings.ResourceLoadedCallback != null)
            {
                var args = new WebViewResourceLoadedCallback(
                    ConvertRequest(request),
                    ConvertResponse(response),
                    (SDK.WebViewModels.UrlRequestStatus)status,
                    receivedContentLength);
                if (settings.PassResourceContentStreamToCallback)
                    args.ResponseContent = contentStream;

                try
                {
                    settings.ResourceLoadedCallback(args);
                }
                catch (Exception e)
                {
                    logger.Error(e, "Web view resource callback failed.");
                }
            }

            base.OnResourceLoadComplete(chromiumWebBrowser, browser, frame, request, response, status, receivedContentLength);
        }

        protected override void Dispose()
        {
            base.Dispose();
            contentStream?.Dispose();
        }
    }

    public class CustomRequestHandler : CefSharp.Handler.RequestHandler
    {
        private readonly WebViewSettings settings;

        public CustomRequestHandler(WebViewSettings settings)
        {
            this.settings = settings;
        }

        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            return new CustomResourceRequestHandler(settings);
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
