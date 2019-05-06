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

namespace TwitchLibrary
{
    public class TwitchLibrarySettings : ObservableObject, ISettings
    {
        private static ILogger logger = LogManager.GetLogger();
        private TwitchLibrarySettings editingClone;
        private TwitchLibrary library;
        private IPlayniteAPI api;

        #region Settings      

        public bool ImportInstalledGames { get; set; } = true;

        public bool ImportUninstalledGames { get; set; } = false;

        #endregion Settings

        [JsonIgnore]
        public bool IsUserLoggedIn
        {
            get
            {
                if (library.LoginData == null)
                {
                    return false;
                }

                try
                {
                    library.GetLibraryGames();
                    return true;
                }
                catch
                {
                    return false;
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

        private void Login()
        {
            try
            {
                using (var view = api.WebViews.CreateView(400, 600))
                {
                    var api = new TwitchAccountClient(view, library.TokensPath);
                    api.Login();
                }

                OnPropertyChanged(nameof(IsUserLoggedIn));
            }
            catch (Exception e) when (!Environment.IsDebugBuild)
            {
                logger.Error(e, "Failed to authenticate user.");
            }
        }
    }
}
