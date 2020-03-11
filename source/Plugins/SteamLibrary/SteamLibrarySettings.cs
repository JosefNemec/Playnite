using Playnite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Commands;
using SteamLibrary.Models;
using Playnite.SDK;
using System.Windows.Media;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Steam;

namespace SteamLibrary
{
    public enum AuthStatus
    {
        Ok,
        Checking,
        AuthRequired,
        PrivateAccount,
        Failed
    }

    public class SteamLibrarySettings : ObservableObject, ISettings
    {
        private static ILogger logger = LogManager.GetLogger();
        private SteamLibrarySettings editingClone;
        private SteamLibrary library;
        private IPlayniteAPI api;

        #region Settings

        public int Version { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        private bool isPrivateAccount;
        public bool IsPrivateAccount
        {
            get => isPrivateAccount;
            set
            {
                isPrivateAccount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AuthStatus));
            }
        }

        private string apiKey = string.Empty;
        public string ApiKey
        {
            get => apiKey;
            set
            {
                apiKey = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AuthStatus));
            }
        }

        public bool DownloadVerticalCovers { get; set; } = true;

        public bool ImportInstalledGames { get; set; } = true;

        public bool ConnectAccount { get; set; } = false;

        public bool ImportUninstalledGames { get; set; } = false;

        public BackgroundSource BackgroundSource { get; set; } = BackgroundSource.Image;

        #endregion Settings

        [JsonIgnore]
        public AuthStatus AuthStatus
        {
            get
            {
                if (UserId.IsNullOrEmpty())
                {
                    return AuthStatus.AuthRequired;
                }

                try
                {
                    if (IsPrivateAccount)
                    {
                        if (UserId.IsNullOrEmpty() || ApiKey.IsNullOrEmpty())
                        {
                            return AuthStatus.PrivateAccount;
                        }

                        try
                        {
                            var games = library.GetPrivateOwnedGames(ulong.Parse(UserId), ApiKey);
                            if (games?.response?.games.HasItems() == true)
                            {
                                return AuthStatus.Ok;
                            }
                        }
                        catch (System.Net.WebException e)
                        {
                            if (e.Status == System.Net.WebExceptionStatus.ProtocolError)
                            {
                                return AuthStatus.PrivateAccount;
                            }
                        }
                    }
                    else
                    {
                        var games = library.ServicesClient.GetSteamLibrary(UserId);
                        if (games.HasItems())
                        {
                            return AuthStatus.Ok;
                        }
                        else
                        {
                            return AuthStatus.PrivateAccount;
                        }
                    }
                }
                catch (Exception e) when (!Debugger.IsAttached)
                {
                    logger.Error(e, "Failed to check Steam auth status.");
                    return AuthStatus.Failed;
                }

                return AuthStatus.AuthRequired;
            }
        }

        [JsonIgnore]
        public RelayCommand<object> LoginCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                Login();
            });
        }

        [JsonIgnore]
        public bool IsFirstRunUse { get; set; }

        [JsonIgnore]
        public List<LocalSteamUser> SteamUsers { get; set; }

        [JsonIgnore]
        public RelayCommand<LocalSteamUser> ImportSteamCategoriesCommand
        {
            get => new RelayCommand<LocalSteamUser>((a) =>
            {
                ImportSteamCategories(a);
            });
        }

        [JsonIgnore]
        public RelayCommand<LocalSteamUser> ImportSteamLastActivityCommand
        {
            get => new RelayCommand<LocalSteamUser>((a) =>
            {
                ImportSteamLastActivity(a);
            });
        }

        public SteamLibrarySettings()
        {
        }

        public SteamLibrarySettings(SteamLibrary library, IPlayniteAPI api)
        {
            this.library = library;
            this.api = api;

            var settings = library.LoadPluginSettings<SteamLibrarySettings>();
            if (settings != null)
            {
                if (settings.Version == 0)
                {
                    logger.Debug("Updating Steam settings from version 0.");
                    if (settings.ImportUninstalledGames)
                    {
                        settings.ConnectAccount = true;
                    }
                }

                settings.Version = 1;
                LoadValues(settings);
            }
        }

        public void BeginEdit()
        {
            editingClone = this.GetClone();
        }

        public void CancelEdit()
        {
            LoadValues(editingClone);
        }

        public void EndEdit()
        {
            library.SavePluginSettings(this);
        }

        public bool VerifySettings(out List<string> errors)
        {
            if (IsPrivateAccount && ApiKey.IsNullOrEmpty())
            {
                errors = new List<string>{ "Steam API key must be specified when using private accounts!" };
                return false;
            }

            errors = null;
            return true;
        }

        private void LoadValues(SteamLibrarySettings source)
        {
            source.CopyProperties(this, false, null, true);
        }

        public void ImportSteamCategories(LocalSteamUser user)
        {
            var accId = user == null ? 0 : user.Id;
            library.ImportSteamCategories(accId);
        }

        public void ImportSteamLastActivity(LocalSteamUser user)
        {
            var accId = user == null ? 0 : user.Id;
            library.ImportSteamLastActivity(accId);
        }

        private void Login()
        {
            try
            {
                var steamId = string.Empty;
                var userName = "Unknown";
                using (var view = api.WebViews.CreateView(675, 440, Colors.Black))
                {
                    view.NavigationChanged += async (s, e) =>
                    {
                        var address = view.GetCurrentAddress();
                        if (address.Contains(@"steamcommunity.com"))
                        {
                            var source = await view.GetPageSourceAsync();
                            var idMatch = Regex.Match(source, @"g_steamID = ""(\d+)""");
                            if (idMatch.Success)
                            {
                                steamId = idMatch.Groups[1].Value;
                            }

                            var userMatch = Regex.Match(source, @"personaname"":""(.+?)""");
                            if (userMatch.Success)
                            {
                                userName = userMatch.Groups[1].Value;
                            }

                            if (idMatch.Success)
                            {
                                view.Close();
                            }
                        }
                    };

                    view.DeleteDomainCookies(".steamcommunity.com");
                    view.Navigate(@"https://steamcommunity.com/login/home/?goto=");
                    view.OpenDialog();
                }

                if (!steamId.IsNullOrEmpty())
                {
                    UserId = steamId;
                }

                if (!userName.IsNullOrEmpty())
                {
                    UserName = userName;
                }

                OnPropertyChanged(nameof(AuthStatus));
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                api.Dialogs.ShowErrorMessage(api.Resources.GetString("LOCNotLoggedInError"), "");
                logger.Error(e, "Failed to authenticate user.");
            }
        }
    }
}