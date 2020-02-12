using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.WebView
{
    public class CookieDestroyer : ICookieVisitor
    {
        private readonly string domainName;

        public CookieDestroyer(string domainName)
        {
            this.domainName = domainName;
        }

        public void Dispose()
        {
        }

        public bool Visit(Cookie cookie, int count, int total, ref bool deleteCookie)
        {
            if (cookie.Domain == domainName)
            {
                deleteCookie = true;
            }

            return true;
        }
    }
}
