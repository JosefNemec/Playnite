using Newtonsoft.Json;
using Playnite;
using Playnite.SDK;
using Playnite.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItchioLibrary
{
    public class ItchioLibrarySettings : ObservableObject, ISettings
    {
        private static ILogger logger = LogManager.GetLogger();
        private ItchioLibrarySettings editingClone;
        private ItchioLibrary library;
        private IPlayniteAPI api;

        #region Settings   

        public int Version { get; set; }

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

        public ItchioLibrarySettings(ItchioLibrary library, IPlayniteAPI api)
        {
            this.library = library;
            this.api = api;

            var settings = library.LoadPluginSettings<ItchioLibrarySettings>();
            if (settings != null)
            {
                if (settings.Version == 0)
                {
                    logger.Debug("Updating itch settings from version 0.");
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
                    api.Dialogs.ShowErrorMessage(
                        api.Resources.GetString("LOCItchioClientNotInstalledError"), "");
                    return;
                }

                api.Dialogs.ShowMessage(api.Resources.GetString("LOCItchioSignInNotif"));
                Itch.StartClient();
                api.Dialogs.ShowMessage(api.Resources.GetString("LOCItchioSignInWaitMessage"));
                OnPropertyChanged(nameof(IsUserLoggedIn));
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                api.Dialogs.ShowErrorMessage(api.Resources.GetString("LOCNotLoggedInError"), "");
                logger.Error(e, "Failed to authenticate itch.io user.");
            }
        }
    }
}
