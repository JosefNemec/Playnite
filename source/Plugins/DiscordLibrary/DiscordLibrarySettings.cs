using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using DiscordLibrary.Services;
using Newtonsoft.Json;
using Playnite;
using Playnite.SDK;
using Playnite.Commands;

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

        public string ApiToken { get; set; } = null;

        #endregion Settings

        [JsonIgnore]
        public bool IsUserLoggedIn
        {
            get
            {
                using (var view = api.WebViews.CreateOffscreenView())
                {
                    ApiToken = new DiscordAccountClient(view).GetToken();
                    return ApiToken != null;
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

        public DiscordLibrarySettings()
        {
        }

        public DiscordLibrarySettings(DiscordLibrary library, IPlayniteAPI api)
        {
            this.library = library;
            this.api = api;

            var settings = library.LoadPluginSettings<DiscordLibrarySettings>();
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

        #endregion

        private void Login()
        {
            try
            {
                using (var view = api.WebViews.CreateView(400, 600, Colors.Black))
                {
                    var client = new DiscordAccountClient(view);
                    client.Login();
                }

                OnPropertyChanged(nameof(IsUserLoggedIn));
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                api.Dialogs.ShowErrorMessage(api.Resources.GetString("LOCNotLoggedInError"), "");
                logger.Error(e, "Failed to authenticate user.");
            }
        }
    }
}
