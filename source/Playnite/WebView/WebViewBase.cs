using CefSharp;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.WebView
{
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
                    Creation = cookie.Creation
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
                    Creation = DateTime.Now,
                    HttpOnly = false,
                    LastAccess = DateTime.Now,
                    Secure = false,
                    Path = path
                });
            }
        }
    }
}
