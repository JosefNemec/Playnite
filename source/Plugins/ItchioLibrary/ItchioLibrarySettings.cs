using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ItchioLibrary
{
    public class ItchioLibrarySettings : ObservableObject, ISettings
    {
        private readonly ILogger logger = LogManager.GetLogger();
        private ItchioLibrarySettings editingClone;
        private readonly ItchioLibrary library;
        private readonly IPlayniteAPI playniteApi;

        #region Settings      

        public bool ImportInstalledGames { get; set; } = Itch.IsInstalled;

        public bool ConnectAccount { get; set; } = false;

        public bool ImportUninstalledGames { get; set; } = false;

        #endregion Settings

        [JsonIgnore]
        public bool IsUserLoggedIn
        {
            get
            {
                if (!Itch.IsInstalled)
                {
                    return false;
                }

                using (var butler = new Butler())
                {
                    return butler.GetProfiles().Count > 0;
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

        public ItchioLibrarySettings()
        {
        }

        public ItchioLibrarySettings(ItchioLibrary library, IPlayniteAPI playniteApi)
        {
            this.library = library;
            this.playniteApi = playniteApi;

            var settings = library.LoadPluginSettings<ItchioLibrarySettings>();
            if (settings != null)
            {
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

        private void LoadValues(ItchioLibrarySettings source)
        {
            source.CopyProperties(this, false, null, true);
        }

        private void Login()
        {
            try
            {
                if (!Itch.IsInstalled)
                {
                    playniteApi.Dialogs.ShowErrorMessage(
                        playniteApi.Resources.GetString("LOCItchioClientNotInstalledError"), "");
                    return;
                }

                playniteApi.Dialogs.ShowMessage(playniteApi.Resources.GetString("LOCItchioSignInNotif"));
                Itch.StartClient();
                playniteApi.Dialogs.ShowMessage(playniteApi.Resources.GetString("LOCItchioSignInWaitMessage"));
                OnPropertyChanged(nameof(IsUserLoggedIn));
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                playniteApi.Dialogs.ShowErrorMessage(playniteApi.Resources.GetString("LOCNotLoggedInError"), "");
                logger.Error(e, "Failed to authenticate itch.io user.");
            }
        }
    }
}