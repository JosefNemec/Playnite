using Newtonsoft.Json;
using Playnite;
using Playnite.SDK;
using Playnite.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLibrary.Services;
using Playnite.Common;

namespace TwitchLibrary
{
    public class TwitchLibrarySettings : ObservableObject, ISettings
    {
        private static ILogger logger = LogManager.GetLogger();
        private TwitchLibrarySettings editingClone;
        private TwitchLibrary library;
        private IPlayniteAPI api;

        #region Settings

        public int Version { get; set; }

        public bool ImportInstalledGames { get; set; } = true;

        public bool ConnectAccount { get; set; } = false;

        public bool ImportUninstalledGames { get; set; } = false;

        public bool StartGamesWithoutLauncher { get; set; } = false;

        #endregion Settings

        [JsonIgnore]
        public bool IsUserLoggedIn
        {
            get
            {
                var token = library.GetAuthToken();
                if (token.IsNullOrEmpty())
                {
                    return false;
                }
                else
                {
                    try
                    {
                        AmazonEntitlementClient.GetAccountEntitlements(token);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
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

        public TwitchLibrarySettings()
        {
        }

        public TwitchLibrarySettings(TwitchLibrary library, IPlayniteAPI api)
        {
            this.library = library;
            this.api = api;

            var settings = library.LoadPluginSettings<TwitchLibrarySettings>();
            if (settings != null)
            {
                if (settings.Version == 0)
                {
                    logger.Debug("Updating Twitch settings from version 0.");
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
            errors = null;
            return true;
        }

        private void LoadValues(TwitchLibrarySettings source)
        {
            source.CopyProperties(this, false, null, true);
        }

        public class Cookie
        {
            public string value { get; set; }
        }

        private void Login()
        {
            try
            {
                if (!Twitch.IsInstalled)
                {
                    api.Dialogs.ShowErrorMessage(
                        string.Format(api.Resources.GetString("LOCClientNotInstalledError"), "Twitch"),
                        "");
                    return;
                }

                api.Dialogs.ShowMessage(string.Format(api.Resources.GetString("LOCSignInExternalNotif"), "Twitch"));
                Twitch.StartClient();
                api.Dialogs.ShowMessage(api.Resources.GetString("LOCSignInExternalWaitMessage"));
                OnPropertyChanged(nameof(IsUserLoggedIn));
            }
            catch (Exception e) when (!Environment.IsDebugBuild)
            {
                logger.Error(e, "Failed to authenticate user.");
            }
        }
    }
}
