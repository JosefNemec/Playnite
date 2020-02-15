using HumbleLibrary.Models;
using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HumbleLibrary.Services
{
    public class HumbleAccountClient
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IWebView webView;
        private const string loginUrl = @"https://www.humblebundle.com/login?goto=%2Fhome%2Flibrary&qs=hmb_source%3Dnavbar";
        private const string libraryUrl = @"https://www.humblebundle.com/home/library?hmb_source=navbar";
        private const string logoutUrl = @"https://www.humblebundle.com/logout?goto=/";
        private const string orderUrlMask = @"https://www.humblebundle.com/api/v1/order/{0}?all_tpkds=true";

        public HumbleAccountClient(IWebView webView)
        {
            this.webView = webView;
        }

        public void Login()
        {
            webView.NavigationChanged += (s, e) =>
            {
                if (webView.GetCurrentAddress() == libraryUrl)
                {
                    webView.Close();
                }
            };

            webView.DeleteDomainCookies(".humblebundle.com");
            webView.DeleteDomainCookies("www.humblebundle.com");
            webView.Navigate(loginUrl);
            webView.OpenDialog();
        }

        public bool GetIsUserLoggedIn()
        {
            webView.NavigateAndWait(libraryUrl);
            return webView.GetPageSource().Contains("\"gamekeys\":");
        }

        internal List<string> GetLibraryKeys()
        {
            webView.NavigateAndWait(libraryUrl);
            var libSource = webView.GetPageSource();
            var match = Regex.Match(libSource, @"""gamekeys"":\s*(\[.+\])");
            if (match.Success)
            {
                var strKeys = match.Groups[1].Value;
                return Serialization.FromJson<List<string>>(strKeys);
            }
            else
            {
                throw new Exception("User is not authenticated.");
            }
        }

        internal List<Order> GetOrders(List<string> gamekeys)
        {
            var orders = new List<Order>();
            foreach (var key in gamekeys)
            {
                webView.NavigateAndWait(string.Format(orderUrlMask, key));
                var strContent = webView.GetPageText();
                orders.Add(Serialization.FromJson<Order>(strContent));
            }

            return orders;
        }

        internal List<Order> GetOrders(string cachePath)
        {
            var orders = new List<Order>();
            foreach (var cacheFile in Directory.GetFiles(cachePath))
            {
                orders.Add(Serialization.FromJsonFile<Order>(cacheFile));
            }

            return orders;
        }
    }
}
