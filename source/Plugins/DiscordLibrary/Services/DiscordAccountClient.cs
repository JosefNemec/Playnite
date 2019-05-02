using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Media;
using DiscordLibrary.Models;
using Newtonsoft.Json;
using Playnite.SDK;

namespace DiscordLibrary.Services
{
    internal class DiscordAccountClient
    {
        private IWebView webView;

        private readonly string loginUrl = @"https://discordapp.com/login";

        public DiscordAccountClient(IWebView webView)
        {
            this.webView = webView;
        }

        public void Login()
        {
            webView.NavigationChanged += (s, e) =>
            {
                var address = webView.GetCurrentAddress();
                if (address != loginUrl)
                {
                    webView.Close();
                }
            };

            webView.Navigate(loginUrl);
            webView.OpenDialog();
        }

        internal string GetToken()
        {
            var token = string.Empty;
            // Usually, Discord deletes the localStorage object, but we can use a URL like robots.txt that doesn't run scripts
            webView.NavigateAndWait("https://discordapp.com/robots.txt");
            token = webView.EvaluateScript("(() => localStorage.getItem('token')") as string;
            if (!string.IsNullOrEmpty(token) && !string.Equals(token, "undefined"))
            {
                return token.Trim('"');
            }
            return null;
        }

        internal static List<LibraryItem> GetLibrary(string token)
        {
            var httpClient = new WebClient();
            httpClient.Headers.Add("Authorization", token);
            var stringData = httpClient.DownloadString(@"https://discordapp.com/api/v6/users/@me/library");
            return JsonConvert.DeserializeObject<List<LibraryItem>>(stringData);
        }

        internal static List<LastPlayed> GetLastPlayed(string token)
        {
            var httpClient = new WebClient();
            httpClient.Headers.Add("Authorization", token);
            var stringData = httpClient.DownloadString(@"https://discordapp.com/api/v6/users/@me/activities/statistics/applications");
            return JsonConvert.DeserializeObject<List<LastPlayed>>(stringData);
        }
    }
}