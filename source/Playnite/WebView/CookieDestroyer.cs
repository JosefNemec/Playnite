using CefSharp;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.WebView
{
    public class CookieDestroyer : ICookieVisitor
    {
        private readonly string domainName;
        private readonly bool useRegex;

        internal readonly AutoResetEvent Finished = new AutoResetEvent(false);

        public CookieDestroyer(string domainName, bool useRegex)
        {
            this.domainName = domainName;
            this.useRegex = useRegex;
        }

        public void Dispose()
        {
        }

        public bool Visit(Cookie cookie, int count, int total, ref bool deleteCookie)
        {
            if (useRegex && Regex.IsMatch(cookie.Domain, domainName))
            {
                deleteCookie = true;
            }
            else if (cookie.Domain == domainName)
            {
                deleteCookie = true;
            }

            if (count == total - 1)
            {
                Finished.Set();
            }

            return true;
        }
    }

    public class StandardCookieVisitor : ICookieVisitor
    {
        public readonly List<HttpCookie> Cookies = new List<HttpCookie>();
        internal readonly AutoResetEvent Finished = new AutoResetEvent(false);

        public StandardCookieVisitor()
        {
        }

        public void Dispose()
        {
        }

        public bool Visit(Cookie cookie, int count, int total, ref bool deleteCookie)
        {
            Cookies.Add(new HttpCookie
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
            });

            if (count == total - 1)
            {
                Finished.Set();
            }

            return true;
        }
    }

    public class DeleteCookiesHandler : IDeleteCookiesCallback
    {
        internal readonly AutoResetEvent Finished = new AutoResetEvent(false);
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }

        public void OnComplete(int numDeleted)
        {
            Finished.Set();
        }
    }

    public class SetCookieHandler : ISetCookieCallback
    {
        internal readonly AutoResetEvent Finished = new AutoResetEvent(false);
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }

        public void OnComplete(bool success)
        {
            Finished.Set();
        }
    }
}
