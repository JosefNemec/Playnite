using GogLibrary.Models;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GogLibrary.Services
{
    public class GogAccountClient
    {
        private ILogger logger = LogManager.GetLogger();
        private IWebView webView;

        public GogAccountClient(IWebView webView)
        {
            this.webView = webView;
        }

        public bool GetIsUserLoggedIn()
        {
            webView.NavigateAndWait(@"https://www.gog.com/account/getFilteredProducts?hiddenFlag=0&mediaType=1&page=1&sortBy=title");
            return webView.GetCurrentAddress().Contains("getFilteredProducts");
        }

        public void Login()
        {
            var loginUrl = Gog.GetLoginUrl();
            webView.NavigationChanged += (s, e) =>
            {
                if (webView.GetCurrentAddress().Contains("/on_login_success"))
                {
                    webView.Close();
                }
            };

            ForceWebLanguage(Gog.EnStoreLocaleString);
            webView.Navigate(loginUrl);
            webView.OpenDialog();
        }

        public void ForceWebLanguage(string localeCode)
        {
            webView.Navigate(@"https://www.gog.com/user/changeLanguage/" + localeCode);
        }

        public AccountBasicRespose GetAccountInfo()
        {
            webView.NavigateAndWait(@"https://menu.gog.com/v1/account/basic");
            var stringInfo = webView.GetPageText();
            var accountInfo = JsonConvert.DeserializeObject<AccountBasicRespose>(stringInfo);
            return accountInfo;
        }

        public List<LibraryGameResponse> GetOwnedGamesFromPublicAccount(string accountName)
        {
            var baseUrl = @"https://www.gog.com/u/{0}/games/stats?sort=recent_playtime&order=desc&page={1}";
            var url = string.Format(baseUrl, accountName, 1);
            var gamesList = HttpDownloader.DownloadString(url);
            var games = new List<LibraryGameResponse>();
            var libraryData = JsonConvert.DeserializeObject<PagedResponse<LibraryGameResponse>>(gamesList);

            if (libraryData == null)
            {
                logger.Error("GOG library content is empty or private.");
                return null;
            }

            games.AddRange(libraryData._embedded.items);

            if (libraryData.pages > 1)
            {
                for (int i = 2; i <= libraryData.pages; i++)
                {
                    gamesList = HttpDownloader.DownloadString(string.Format(baseUrl, accountName, i));
                    var pageData = JsonConvert.DeserializeObject<PagedResponse<LibraryGameResponse>>(gamesList);
                    games.AddRange(pageData._embedded.items);
                }
            }

            return games;
        }

        public List<LibraryGameResponse> GetOwnedGames(AccountBasicRespose account)
        {
            var baseUrl = @"https://www.gog.com/u/{0}/games/stats?sort=recent_playtime&order=desc&page={1}";
            var url = string.Format(baseUrl, account.username, 1);
            webView.NavigateAndWait(url);
            var gamesList = webView.GetPageText();
            var games = new List<LibraryGameResponse>();
            var libraryData = JsonConvert.DeserializeObject<PagedResponse<LibraryGameResponse>>(gamesList);

            if (libraryData == null)
            {
                logger.Error("GOG library content is empty or private.");
                return null;
            }

            games.AddRange(libraryData._embedded.items);

            if (libraryData.pages > 1)
            {
                for (int i = 2; i <= libraryData.pages; i++)
                {
                    webView.NavigateAndWait(string.Format(baseUrl, account.username, i));
                    gamesList = webView.GetPageText();
                    var pageData = JsonConvert.DeserializeObject<PagedResponse<LibraryGameResponse>>(gamesList);
                    games.AddRange(pageData._embedded.items);
                }
            }

            return games;
        }
    }
}
