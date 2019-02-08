using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumbleBundleLibrary
{
    class HumbleBundleLoginApi
    {
        private const string loginUrl = "https://www.humblebundle.com/login";
        private const string homeUrl = "https://www.humblebundle.com/home/library";

        private IWebView webView;

        public HumbleBundleLoginApi(IWebView view)
        {
            this.webView = view;
        }

        public void Execute()
        {
            webView.NavigationChanged += (s, e) =>
            {
                string currentAddress = webView.GetCurrentAddress();
                Console.WriteLine("Address: {0}", currentAddress);
                if (currentAddress.StartsWith(homeUrl))
                {
                    webView.Close();
                    return;
                }
            };

            webView.Navigate(loginUrl);
            webView.OpenDialog();
        }

        public bool GetIsUserLoggedIn()
        {
            webView.NavigateAndWait(homeUrl);
            var currentUrl = webView.GetCurrentAddress();
            return currentUrl == homeUrl;
        }
    }
}
