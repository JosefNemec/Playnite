using GogLibrary.Models;
using Newtonsoft.Json;
using Playnite.Common.Web;
using Playnite.SDK;
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
            loginUrl = Regex.Replace(loginUrl, $"&gog_lc=.+$", "&gog_lc=" + Gog.EnStoreLocaleString);
            webView.NavigationChanged += (s, e) =>
            {
                if (webView.GetCurrentAddress().Contains("/on_login_success"))
                {
                    webView.Close();
                }
            };

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
            var stringLibContent = string.Empty;
            var games = new List<LibraryGameResponse>();

            try
            {
                var url = string.Format(baseUrl, account.username, 1);
                webView.NavigateAndWait(url);
                stringLibContent = webView.GetPageText();
                var libraryData = JsonConvert.DeserializeObject<PagedResponse<LibraryGameResponse>>(stringLibContent);
                if (libraryData == null)
                {
                    logger.Error("GOG library content is empty.");
                    return null;
                }

                games.AddRange(libraryData._embedded.items);
                if (libraryData.pages > 1)
                {
                    for (int i = 2; i <= libraryData.pages; i++)
                    {
                        webView.NavigateAndWait(string.Format(baseUrl, account.username, i));
                        stringLibContent = webView.GetPageText();
                        var pageData = JsonConvert.DeserializeObject<PagedResponse<LibraryGameResponse>>(stringLibContent);
                        games.AddRange(pageData._embedded.items);
                    }
                }

                return games;
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to library from new API for account {account.username}, falling back to legacy.");
                logger.Debug(stringLibContent);
                return GetOwnedGames();
            }
        }

        public List<LibraryGameResponse> GetOwnedGames()
        {
            var games = new List<LibraryGameResponse>();
            var baseUrl = @"https://www.gog.com/account/getFilteredProducts?hiddenFlag=0&mediaType=1&page={0}&sortBy=title";
            webView.NavigateAndWait(string.Format(baseUrl, 1));
            var gamesList = webView.GetPageText();

            var libraryData = JsonConvert.DeserializeObject<GetOwnedGamesResult>(gamesList);
            if (libraryData == null)
            {
                logger.Error("GOG library content is empty.");
                return null;
            }

            games.AddRange(libraryData.products.Select(a => new LibraryGameResponse()
            {
                game = new LibraryGameResponse.Game()
                {
                    id = a.id.ToString(),
                    title = a.title,
                    url = a.url,
                    image = a.image
                }
            }));

            if (libraryData.totalPages > 1)
            {
                for (int i = 2; i <= libraryData.totalPages; i++)
                {
                    webView.NavigateAndWait(string.Format(baseUrl, i));
                    gamesList = webView.GetPageText();
                    var pageData = libraryData = JsonConvert.DeserializeObject<GetOwnedGamesResult>(gamesList);                    
                    games.AddRange(pageData.products.Select(a => new LibraryGameResponse()
                    {
                        game = new LibraryGameResponse.Game()
                        {
                            id = a.id.ToString(),
                            title = a.title,
                            url = a.url,
                            image = a.image
                        }
                    }));
                }
            }

            return games;
        }
    }
}
