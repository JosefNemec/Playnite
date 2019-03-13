using System;
using System.Windows.Media;
using Playnite.SDK;

namespace DiscordLibrary.Services
{
    internal class DiscordAccountClient
    {
        private IPlayniteAPI api;
        private DiscordLibrary library;

        private readonly string loginUrl = @"https://discordapp.com/login";

        public DiscordAccountClient(IPlayniteAPI api, DiscordLibrary library)
        {
            this.api = api;
            this.library = library;
        }

        public void Login()
        {
            var loginPageSource = string.Empty;

            // TODO: log out by clearing localstorage

            using (var view = api.WebViews.CreateView(675, 600, Colors.Black))
            {
                view.NavigationChanged += async (s, e) =>
                {
                    var address = view.GetCurrentAddress();
                    if (address != loginUrl)
                    {
                        loginPageSource = await view.GetPageSourceAsync();
                        //TODO: extract token from localstorage
                        view.Close();
                    }
                };

                view.Navigate(loginUrl);
                view.OpenDialog();
            }
        }

        internal bool GetIsUserLoggedIn()
        {
            return false;
        }
    }
}