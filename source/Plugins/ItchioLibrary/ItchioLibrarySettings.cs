using Newtonsoft.Json;
using Playnite;
using Playnite.SDK;
using PlayniteUI.Commands;
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

        public bool ImportInstalledGames { get; set; } = Itch.IsInstalled;

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

            var settings = api.LoadPluginSettings<ItchioLibrarySettings>(library);
            if (settings != null)
            {
                LoadValues(settings);
            }
        }

        public void BeginEdit()
        {
            editingClone = this.CloneJson();
        }

        public void CancelEdit()
        {
            LoadValues(editingClone);
        }

        public void EndEdit()
        {
            api.SavePluginSettings(library, this);
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
                        api.Resources.FindString("LOCItchioClientNotInstalledError"), "");
                    return;
                }

                api.Dialogs.ShowMessage(api.Resources.FindString("LOCItchioSignInNotif"));
                Itch.StartClient();
                api.Dialogs.ShowMessage(api.Resources.FindString("LOCItchioSignInWaitMessage"));
                OnPropertyChanged(nameof(IsUserLoggedIn));
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                api.Dialogs.ShowErrorMessage(api.Resources.FindString("LOCNotLoggedInError"), "");
                logger.Error(e, "Failed to authenticate itch.io user.");
            }
        }
    }
}
