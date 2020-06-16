using BattleNetLibrary.Models;
using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BattleNetLibrary.Services
{
    public class BattleNetAccountClient
    {
        private const string apiStatusUrl = @"https://account.blizzard.com/api/";
        private const string gamesUrl = @"https://account.blizzard.com/api/games-and-subs";
        private const string classicGamesUrl = @"https://account.blizzard.com/api/classic-games";
        private static ILogger logger = LogManager.GetLogger();
        private IWebView webView;

        public BattleNetAccountClient(IWebView webView)
        {
            this.webView = webView;
        }

        public List<GamesAndSubs.GameAccount> GetOwnedGames()
        {
            webView.NavigateAndWait(gamesUrl);
            var textGames = webView.GetPageText();
            var games = Serialization.FromJson<GamesAndSubs>(textGames);
            return games.gameAccounts;
        }

        public List<ClassicGames.ClassicGame> GetOwnedClassicGames()
        {
            webView.NavigateAndWait(classicGamesUrl);
            var textGames = webView.GetPageText();
            var games = Serialization.FromJson<ClassicGames>(textGames);
            return games.classicGames;
        }

        public void Login()
        {
            var apiUrls = GetDefaultApiStatus();
            webView.NavigationChanged += (s, e) =>
            {
                var address = webView.GetCurrentAddress();
                if (address.Equals(@"https://account.blizzard.com/overview", StringComparison.OrdinalIgnoreCase))
                {
                    webView.Close();
                }
            };

            webView.Navigate(apiUrls.logoutUri);
            webView.OpenDialog();
        }

        public bool GetIsUserLoggedIn()
        {
            return GetApiStatus().authenticated;
        }

        public static BattleNetApiStatus GetDefaultApiStatus()
        {
            using (var webClient = new WebClient())
            {
                try
                {
                    webClient.DownloadData(apiStatusUrl);
                }
                catch (WebException exception) // Response is always 401
                {
                    string responseText = string.Empty;
                    var responseStream = exception.Response?.GetResponseStream();
                    if (responseStream != null)
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            responseText = reader.ReadToEnd();
                        }
                    }

                    var deserialized = Serialization.FromJson<BattleNetApiStatus>(responseText);
                    return  deserialized;
                }

                return null;
            }
        }

        public BattleNetApiStatus GetApiStatus()
        {
            // This refreshes authentication cookie
            webView.NavigateAndWait("https://account.blizzard.com:443/oauth2/authorization/account-settings");
            webView.NavigateAndWait(apiStatusUrl);
            var textStatus = webView.GetPageText();
            return Serialization.FromJson<BattleNetApiStatus>(textStatus);
        }
    }
}
