using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleNetLibrary.Services
{
    public class BattleNetAccountClient
    {
        private const string libraryUrl = @"battle.net/account/management/";
        private ILogger logger = LogManager.GetLogger();
        private IWebView webView;

        public BattleNetAccountClient(IWebView webView)
        {
            this.webView = webView;
        }

        public string GetOwnedGames()
        {
            webView.NavigateAndWait(libraryUrl);
            return webView.GetPageSource();
        }

        public void Login()
        {
            webView.NavigationChanged += (s, e) =>
            {
                if (webView.GetCurrentAddress().EndsWith(libraryUrl))
                {
                    webView.Close();
                }
            };

            webView.Navigate(@"https://battle.net/account/management/?logout");
            webView.OpenDialog();
        }

        public bool GetIsUserLoggedIn()
        {
            webView.NavigateAndWait(libraryUrl);
            return !webView.GetCurrentAddress().Contains("battle.net/login");
        }
    }
}
