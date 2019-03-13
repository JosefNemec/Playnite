using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordLibrary.Services;
using Newtonsoft.Json;
using Playnite;
using Playnite.SDK;
using PlayniteUI.Commands;

namespace DiscordLibrary
{
    class DiscordLibrarySettings : ObservableObject, ISettings
    {
        private static ILogger logger = LogManager.GetLogger();
        private DiscordLibrarySettings editingClone;
        private DiscordLibrary library;
        private IPlayniteAPI api;

        #region Settings      

        public bool ImportInstalledGames { get;  set; } = Discord.IsInstalled;

        public bool ImportUninstalledGames { get;  set; } = false;

        #endregion Settings

        [JsonIgnore]
        public bool IsUserLoggedIn
        {
            get
            {
                return new DiscordAccountClient(api, library).GetIsUserLoggedIn();
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

        public DiscordLibrarySettings()
        {
        }

        public DiscordLibrarySettings(DiscordLibrary library, IPlayniteAPI api)
        {
            this.library = library;
            this.api = api;

            var settings = api.LoadPluginSettings<DiscordLibrarySettings>(library);
            if (settings != null)
            {
                LoadValues(settings);
            }
        }

        private void LoadValues(DiscordLibrarySettings source)
        {
            source.CopyProperties(this, false, null, true);
        }

        #region ISettings

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

        #endregion

        private void Login()
        {
            try
            {
                var client = new DiscordAccountClient(api, library);
                client.Login();
                OnPropertyChanged(nameof(IsUserLoggedIn));
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                api.Dialogs.ShowErrorMessage(api.Resources.FindString("LOCNotLoggedInError"), "");
                logger.Error(e, "Failed to authenticate user.");
            }
        }
    }
}
